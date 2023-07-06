using System;
using System.Collections.Generic;


public class LocationQualityControl
{
    private List<Pathpoint> locationHistory;
    private WalkingDetector walkingDetector;
    private int slidingWindowSize;
    private double maxSpeed;
    private double maxAccuracy;

    public LocationQualityControl(double expectedGPSAccuracy, int slidingWindowSize, double maxSpeed, double maxAccuracy)
    {
        locationHistory = new List<Pathpoint>();
        walkingDetector = new WalkingDetector(expectedGPSAccuracy);
        this.slidingWindowSize = slidingWindowSize;
        this.maxSpeed = maxSpeed;
        this.maxAccuracy = maxAccuracy;
    }

    public (Pathpoint, double, double) ProcessLocation(Pathpoint rawLocation)
    {
        // Apply Kalman filter        
        Pathpoint smoothedLocation = walkingDetector.ApplyKalmanFilter(rawLocation);

        // Add the smoothed location to the location history
        locationHistory.Add(smoothedLocation);

        // Keep only the last N locations
        if (locationHistory.Count > slidingWindowSize)
        {
            locationHistory.RemoveAt(0);
        }

        // Apply the sliding window approach to average the last N smoothed locations
        Pathpoint averagedLocation = AverageLastNLocations(slidingWindowSize);
        averagedLocation.Timestamp = rawLocation.Timestamp;

        //Debug.Log("Averaged Accuracy: " + averagedLocation.Accuracy);

        // Check the GPS accuracy and apply the speed-based filter
        (bool isValid, double accuracy, double speed) = IsValidLocation(averagedLocation);
        if (isValid)
        {
            return (averagedLocation, accuracy, speed);
        }
        else
        {
            return (null, accuracy, speed);
        }
    }

    private Pathpoint AverageLastNLocations(int n)
    {
        double totalLatitude = 0;
        double totalLongitude = 0;
        double totalAccuracy = 0;
        int count = 0;

        for (int i = Math.Max(0, locationHistory.Count - n); i < locationHistory.Count; i++)
        {
            totalLatitude += locationHistory[i].Latitude;
            totalLongitude += locationHistory[i].Longitude;
            totalAccuracy += locationHistory[i].Accuracy;
            count++;
        }

        return new Pathpoint
        {
            Latitude = totalLatitude / count,
            Longitude = totalLongitude / count,
            Accuracy = totalAccuracy / count
        };
    }


    private (bool, double, double) IsValidLocation(Pathpoint location)
    {
        bool isValid = true;
        double speed = 0;
        // Check GPS accuracy
        if (location.Accuracy > maxAccuracy)
        {
            isValid = false;
        }

        // Check speed-based filter
        if (locationHistory.Count >= 2)
        {
            Pathpoint previousLocation = locationHistory[locationHistory.Count - 2];
            speed = CalculateSpeed(previousLocation, location);
        }

        return (isValid && speed < maxSpeed, location.Accuracy, speed);
    }

    private double CalculateSpeed(Pathpoint p1, Pathpoint p2)
    {
        double distance = LocationUtils.HaversineDistance(p1, p2);

        DateTime dateTime1 = new DateTime(p1.Timestamp);
        DateTime dateTime2 = new DateTime(p2.Timestamp);

        //Debug.Log($"Timestamp 1:{p1.Timestamp} Timestamp 2: {p2.Timestamp} --> {p2.Timestamp - p1.Timestamp}");

        TimeSpan time = dateTime2 - dateTime1;
        double seconds = time.TotalSeconds;

        return distance / seconds;
    }


}

