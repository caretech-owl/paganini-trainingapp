using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class WalkingDetector
{
    private Matrix<double> state;
    private Matrix<double> errorCovariance;
    private Matrix<double> processNoise;
    private Matrix<double> measurementNoise;
    private Matrix<double> transitionMatrix;
    private Matrix<double> measurementMatrix;

    private double walkingThreshold; // This threshold should be defined according to the distance unit you are using
    private bool initialised = false;
    private List<Pathpoint> gpsHistory;

    private LocationUtilsConfig config;

    public WalkingDetector(LocationUtilsConfig config)
    {
        this.config = config;
        // Initialize the Kalman filter
        state = Matrix<double>.Build.Dense(4, 1);
        errorCovariance = Matrix<double>.Build.DenseIdentity(4);
        processNoise = Matrix<double>.Build.Dense(4, 4);
        measurementNoise = Matrix<double>.Build.Dense(2, 2);
        transitionMatrix = Matrix<double>.Build.Dense(4, 4);
        measurementMatrix = Matrix<double>.Build.Dense(2, 4);

        // Set the process and measurement noise matrices
        processNoise[0, 0] = 0.1;
        processNoise[1, 1] = 0.1;
        processNoise[2, 2] = 0.1;
        processNoise[3, 3] = 0.1;
        //measurementNoise[0, 0] = 0.1;
        //measurementNoise[1, 1] = 0.1;
        // EXPERIMENTAL! Update the measurement noise matrix based on expected GPS accuracy
        //measurementNoise[0, 0] = Math.Pow(expectedGPSAccuracy / 100, 2);
        //measurementNoise[1, 1] = Math.Pow(expectedGPSAccuracy / 100, 2);
        updateMeasurementNoise(config.ExpectedGPSAccuracy);


        // Set the transition matrix
        transitionMatrix[0, 0] = 1;
        transitionMatrix[0, 2] = 1;
        transitionMatrix[1, 1] = 1;
        transitionMatrix[1, 3] = 1;
        transitionMatrix[2, 2] = 1;
        transitionMatrix[3, 3] = 1;

        // Set the measurement matrix
        measurementMatrix[0, 0] = 1;
        measurementMatrix[1, 1] = 1;


        walkingThreshold = EstimateWalkingThreshold(config.ExpectedGPSAccuracy / 1000);

        gpsHistory = new List<Pathpoint>();
    }

    public void SetInitialState(Pathpoint initialLocation)
    {
        state[0, 0] = initialLocation.Latitude;
        state[1, 0] = initialLocation.Longitude;
        state[2, 0] = 0; // Initial velocity in latitude
        state[3, 0] = 0; // Initial velocity in longitude
    }


    public Pathpoint ApplyKalmanFilter(Pathpoint rawLocation)
    {

        if (!initialised)
        {
            SetInitialState(rawLocation);
            initialised = true;
            return rawLocation;
        }

        // EXPERIMENTAL! Update the measurement noise matrix based on current GPS accuracy
        //measurementNoise[0, 0] = Math.Pow(rawLocation.Accuracy / 100, 2);
        //measurementNoise[1, 1] = Math.Pow(rawLocation.Accuracy / 100, 2);
        updateMeasurementNoise(rawLocation.Accuracy);

        // Convert the raw location to the measurement matrix format
        var measurement = Matrix<double>.Build.Dense(2, 1);
        measurement[0, 0] = rawLocation.Latitude;
        measurement[1, 0] = rawLocation.Longitude;

        // Predict the next state
        state = transitionMatrix * state;
        errorCovariance = transitionMatrix * errorCovariance * transitionMatrix.Transpose() + processNoise;

        // Compute the Kalman gain
        var innovation = measurement - measurementMatrix * state;
        var innovationCovariance = measurementMatrix * errorCovariance * measurementMatrix.Transpose() + measurementNoise;
        var kalmanGain = errorCovariance * measurementMatrix.Transpose() * innovationCovariance.Inverse();

        // Update the state and error covariance
        state += kalmanGain * innovation;
        errorCovariance = (Matrix<double>.Build.DenseIdentity(4) - kalmanGain * measurementMatrix) * errorCovariance;

        // Create a new Pathpoint instance with the filtered values
        var filteredLocation = new Pathpoint
        {
            Latitude = state[0, 0],
            Longitude = state[1, 0],
            Accuracy = rawLocation.Accuracy
        };

        return filteredLocation;
    }

    private void updateMeasurementNoise(double accuracy)
    {
        measurementNoise[0, 0] = Math.Pow(accuracy / 100, 2);
        measurementNoise[1, 1] = Math.Pow(accuracy / 100, 2);

        //measurementNoise[0, 0] = Math.Pow(accuracy /10, 2);
        //measurementNoise[1, 1] = Math.Pow(accuracy /10, 2);
    }

    /// <summary>
    /// Updates the history of GPS measurements with the given point, removing old measurements that fall outside the configured time window
    /// and ensuring that the history does not exceed the configured maximum size.
    /// </summary>
    /// <param name="point">The new GPS measurement point to add to the history.</param>
    private void UpdateHistoryMeasurement(Pathpoint point)
    {
        gpsHistory.Add(point);

        // Remove old measurements that fall outside the time window
        long cutoffTime = point.Timestamp - config.MaxWalkingGPSHistoryValidMilliseconds;
        gpsHistory.RemoveAll(p => p.Timestamp < cutoffTime);

        // Ensure that the history does not exceed the configured maximum size
        if (gpsHistory.Count > config.WalkingGPSHistorySize)
        {
            int excessCount = gpsHistory.Count - config.WalkingGPSHistorySize;
            gpsHistory.RemoveRange(0, excessCount);
        }
    }

    /// <summary>
    /// Calculates the average moving speed over the time window defined by the GPS history.
    /// </summary>
    /// <returns>The average moving speed in meters per second.</returns>
    private double CalculateMovingSpeed()
    {
        // Calculate total distance and time elapsed over the time window
        double totalDistance = 0;
        double totalTime = 0;
        for (int i = 1; i < gpsHistory.Count; i++)
        {
            Pathpoint prevPoint = gpsHistory[i - 1];
            Pathpoint currPoint = gpsHistory[i];
            double distance = LocationUtils.HaversineDistance(prevPoint, currPoint);
            double timeElapsed = (currPoint.Timestamp - prevPoint.Timestamp) / 1000.0; // Convert milliseconds to seconds
            totalDistance += distance;
            totalTime += timeElapsed;
        }

        // Calculate average speed over the time window
        double averageSpeed = totalDistance / totalTime;
        return averageSpeed;
    }


    /// <summary>
    /// Determines whether the user is walking based on the moving speed calculated from the GPS history.
    /// </summary>
    /// <param name="currentLocation">The current GPS measurement point.</param>
    /// <param name="averageSpeed">Output parameter to store the calculated average speed.</param>
    /// <returns>True if the user is walking; otherwise, false.</returns>
    public bool IsWalkingBasedOnSpeed(Pathpoint currentLocation, out double averageSpeed)
    {
        // Add current GPS measurement to history
        UpdateHistoryMeasurement(currentLocation);

        //Debug.Log("Walking History: "+ gpsHistory.Count);

        averageSpeed = -1;

        // If there are not enough measurements in the history, return false
        if (gpsHistory.Count < 2)
            return false;

        // Calculate average speed over the time window
        averageSpeed = CalculateMovingSpeed();

        // Determine if the user is walking based on average speed
        bool walking = averageSpeed >= config.MinWalkingSpeedThreshold;

        return walking;
    }



    public (bool, double) IsWalking(Pathpoint currentLocation)
    {
        if (!initialised)
        {
            SetInitialState(currentLocation);
            initialised = true;
            return (false, 0);
        }

        walkingThreshold = EstimateWalkingThreshold(currentLocation.Accuracy / 1000);

        // Predict the next state
        state = transitionMatrix * state;
        errorCovariance = transitionMatrix * errorCovariance * transitionMatrix.Transpose() + processNoise;

        // Compute the Kalman gain
        var measurement = Matrix<double>.Build.Dense(2, 1);
        measurement[0, 0] = currentLocation.Latitude;
        measurement[1, 0] = currentLocation.Longitude;

        var innovation = measurement - measurementMatrix * state;
        var innovationCovariance = measurementMatrix * errorCovariance * measurementMatrix.Transpose() + measurementNoise;
        var kalmanGain = errorCovariance * measurementMatrix.Transpose() * innovationCovariance.Inverse();

        // Update the state and error covariance
        state += kalmanGain * innovation;
        errorCovariance = (Matrix<double>.Build.DenseIdentity(4) - kalmanGain * measurementMatrix) * errorCovariance;

        double deltaX = currentLocation.Latitude - state[0, 0];
        double deltaY = currentLocation.Longitude - state[1, 0];


        double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

        //Debug.Log($"Distance: {distance} Threshold: {walkingThreshold} Pathpoint.Lat: {currentLocation.Latitude} Pathpoint.Lon: {currentLocation.Longitude}");

        return (distance > walkingThreshold, distance / walkingThreshold);
    }

    public static double EstimateWalkingThreshold(double expectedAccuracyInMeters)
    {
        // The approximate number of meters per degree of latitude
        const double metersPerDegreeLatitude = 111000;

        // Calculate the threshold based on the expected accuracy
        double threshold = expectedAccuracyInMeters / metersPerDegreeLatitude;

        // Return the threshold value for latitude and longitude differences
        return threshold;
    }

    public void DebugErrorAnalysis(Pathpoint currentLocation)
    {
        // Predict the next state without updating the filter
        var predictedState = transitionMatrix * state;
        var predictedErrorCovariance = transitionMatrix * errorCovariance * transitionMatrix.Transpose() + processNoise;

        // Compute the predicted measurement
        var predictedMeasurement = measurementMatrix * predictedState;

        // Compute the difference between predicted and actual measurement
        var measurement = Matrix<double>.Build.Dense(2, 1);
        measurement[0, 0] = currentLocation.Latitude;
        measurement[1, 0] = currentLocation.Longitude;
        var measurementError = measurement - predictedMeasurement;

        Console.WriteLine("------- Error Analysis -------");
        Console.WriteLine($"Predicted Measurement: Lat={predictedMeasurement[0, 0]}, Lon={predictedMeasurement[1, 0]}");
        Console.WriteLine($"Actual Measurement: Lat={measurement[0, 0]}, Lon={measurement[1, 0]}");
        Console.WriteLine($"Measurement Error: Lat={measurementError[0, 0]}, Lon={measurementError[1, 0]}");
    }

}