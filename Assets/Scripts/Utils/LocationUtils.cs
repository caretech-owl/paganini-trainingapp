using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class LocationUtils : PersistentLazySingleton<LocationUtils>
{

    LocationUtilsConfig config;

    private const double EARTH_RADIUS = 6371e3; // Earth radius in meters

    private List<Pathpoint> Pathpoints { get; set; }
    public List<Pathpoint> UserLocationHistory { get; set; }
    private int lastClosestSegmentIndex;

    public enum NavigationIssue
    {
        Deviation = 1,
        WrongDirection = 2,
        MissedTurn = 3,
        WrongTurn = 4
    }

    public class SegmentDistanceAndBearing
    {
        public double MinDistanceToSegment { get; set; }
        public double MinBearingDifference { get; set; }
        public double UserHeading { get; set; }
        public double SegmentHeading { get; set; }
        public int ClosestSegmentIndex { get; set; }
        public int StartSegmentIndex { get; set; }
        public int EndSegmentIndex { get; set; }

        public override string ToString()
        {
            return $"dist:{MinDistanceToSegment} bear: {MinBearingDifference} seg - b:{SegmentHeading} usr - b: {UserHeading}";
        }
    }


    public void Initialise(LocationUtilsConfig locationUtilsConfig)
    {
        UserLocationHistory = new List<Pathpoint> { };
        lastClosestSegmentIndex = 0;

        config = locationUtilsConfig;
    }

    /// <summary>
    /// Clear the Location history 
    /// </summary>
    public void ClearLocationHistory()
    {
        UserLocationHistory.Clear();
    }

    /// <summary>
    /// Loads a list of Pathpoints as the route for off-track detection.
    /// </summary>
    /// <param name="pathpoints">A list of Pathpoint objects representing the route.</param>
    public void LoadRoutePathpoints(List<Pathpoint> pathpoints)
    {
        Pathpoints = pathpoints;
    }

    /// <summary>
    /// Determines if the Haversine distance between two coordinates is below the specified threshold.
    /// </summary>
    /// <param name="coord">The first coordinate as a Pathpoint object.</param>
    /// <param name="target">The second coordinate as a Pathpoint object.</param>
    /// <returns>True if the distance is below the threshold, false otherwise.</returns>
    public bool IsPathpointOnTarget(Pathpoint coord, Pathpoint target)
    {
        double haversineDistance = HaversineDistance(coord, target);

        // We consider the coordinates as ellipsis and take the sum of the ratios (accuracy of measurement),
        // as the distance where we could already be on the POI

        double accuracySum = coord.Accuracy + target.Accuracy;
        //double adjustedThreshold = CalculateAdjustedThreshold(accuracySum);
        double adjustedThreshold = CalculateAdaptiveThreshold(accuracySum);        

        return haversineDistance <= adjustedThreshold;

    }
    /// <summary>
    /// Calculates the adaptive scaling factor based on the accuracy sum and a given base.
    /// </summary>
    /// <param name="accuracySum">The sum of accuracies.</param>
    /// <param name="base">The base for the logarithmic function.</param>
    /// <param name="minExpectedAccuracy">The minimum expected accuracy. Defaults to 4.</param>
    /// <returns>The adaptive scaling factor.</returns>
    private double CalculateAdaptiveScalingFactor(double accuracySum, double @base)
    {
        //if (accuracySum < minExpectedAccuracy)
        //{
        //    return 0;
        //}

        //// Shifts the accuracySum so that the minExpected accuracy is at 0
        //accuracySum = Math.Max(accuracySum - minExpectedAccuracy, 1);

        // Normalizes the value
        double maxValue = Math.Log(accuracySum + 1, @base);
        return 1 - Math.Abs(1 / maxValue);
    }

    /// <summary>
    /// Calculates the adjusted threshold based on the accuracy sum.
    /// </summary>
    /// <param name="accuracySum">The sum of accuracies.</param>
    /// <returns>The adjusted threshold.</returns>
    private double CalculateAdjustedThreshold(double accuracySum)
    {
        double accuracyBasedProximity = accuracySum > config.onTargetMaxDistanceThreshold ? config.onTargetMaxDistanceThreshold : accuracySum;

        // Adjusts the threshold based on the sum of accuracies
        double adjustedThreshold = accuracyBasedProximity > config.onTargetMinDistanceThreshold ? accuracyBasedProximity : config.onTargetMinDistanceThreshold;

        return adjustedThreshold;
    }

    /// <summary>
    /// Calculates the adaptive threshold based on the accuracy sum.
    /// </summary>
    /// <param name="accuracySum">The sum of accuracies.</param>
    /// <returns>The adaptive threshold.</returns>
    private double CalculateAdaptiveThreshold(double accuracySum)
    {
        double shiftThresholdFactor = 3; // shift distribution
        double logDistributionBase = 7.2; // Determined as a good value experimentally
        double scalingFactor = CalculateAdaptiveScalingFactor(accuracySum, logDistributionBase);
        double adjustedThreshold = config.onTargetMinDistanceThreshold + (shiftThresholdFactor + scalingFactor * accuracySum);

        //Debug.Log($"AccuracySum: {accuracySum} ScalingFactor: {scalingFactor} Adaptive threshould: {adjustedThreshold} ");

        // we cap the values
        if (adjustedThreshold < config.onTargetMinDistanceThreshold)
        {
            adjustedThreshold = config.onTargetMinDistanceThreshold;
        }
        else if (adjustedThreshold > config.onTargetMaxDistanceThreshold)
        {
            adjustedThreshold = config.onTargetMaxDistanceThreshold;
        }

        return adjustedThreshold;
    }




    /// <summary>
    /// Updates the user's location history by adding a new location and removing older locations if their bearing difference is above a specified threshold.
    /// </summary>
    /// <param name="newUserLocation">A Pathpoint object representing the user's new location.</param>
    /// <param name="bearingThreshold">The bearing difference threshold in degrees for removing older locations from the history. Default value is 30.</param>
    public void UpdateUserLocationHistory(Pathpoint newUserLocation, double bearingThreshold = 90)
    {
        // Calculate the bearing difference between the new point and the last point in the history
        double bearingDifference = 0;
        if (UserLocationHistory.Count > 1)
        {
            double newBearing = CalculateBearing(UserLocationHistory[UserLocationHistory.Count - 1], newUserLocation);
            double lastBearing = CalculateBearing(UserLocationHistory[UserLocationHistory.Count - 1], UserLocationHistory[Math.Max(0, UserLocationHistory.Count - 2)]);
            bearingDifference = Math.Abs(newBearing - lastBearing);
        }

        // If the bearing difference is above the threshold, remove older locations from the history
        if (bearingDifference > bearingThreshold)
        {
            //UserLocationHistory.Clear();
        }

        //Debug.Log("Bearing difference wrt history: " + bearingDifference);

        // Add the new location to the history
        UserLocationHistory.Add(newUserLocation);

        // Remove the oldest location if the history size exceeds the limit
        if (UserLocationHistory.Count > config.locationHistorySize)
        {
            UserLocationHistory.RemoveAt(0);
        }
    }


    /// <summary>
    /// Identifies the upcoming POI withing a certain search area
    /// </summary>
    /// <param name="userLocation">A Pathpoint object representing the user's current location.</param>
    /// <param name="area">POI search area from current location.</param>
    /// <returns> A pathpoint representing upcoming POI.</returns>
    public Pathpoint IdentifyClosestUpcomingPOI(Pathpoint userLocation, int area)
    {
        double minDistanceToSegment = double.MaxValue;
        int closestSegmentIndex = lastClosestSegmentIndex;

        int startSegmentIndex = GetPOISegmentIndex(lastClosestSegmentIndex, -1, area);
        int endSegmentIndex = GetPOISegmentIndex(lastClosestSegmentIndex, 1, area);


        for (int i = startSegmentIndex; i < endSegmentIndex; i++)
        {
            double distanceToSegment = CalculateDistanceToSegment(userLocation, Pathpoints[i], Pathpoints[i + 1]);
            if (distanceToSegment < minDistanceToSegment)
            {
                minDistanceToSegment = distanceToSegment;
                closestSegmentIndex = i;
            }
        }

        // Let's check the next POI from the closest segment
        closestSegmentIndex = GetPOISegmentIndex(closestSegmentIndex, 1, 1);

        return Pathpoints[closestSegmentIndex];
    }

    private int GetPOISegmentIndex(int startIndex, int increment, int searchArea)
    {
        int segmentIndex = startIndex; //-1;
        int area = 0;
        for (int i = startIndex; i >= 0 && i < Pathpoints.Count; i = i + increment)
        {
            if (Pathpoints[i].POIType != Pathpoint.POIsType.Point)
            {
                segmentIndex = i;
                area++;
            }
            if (area == searchArea) break;
        }

        return segmentIndex;
    }

    public static double CalculateBearingDifference(double bearing1, double bearing2)
    {
        double difference = Math.Abs(bearing1 - bearing2) % 360;
        if (difference > 180)
            difference = 360 - difference;

        return difference;
    }



    /// <summary>
    /// Calculates the minimum distance to the nearest route segment and the minimum bearing difference between the user's heading and the segment's heading.
    /// </summary>
    /// <param name="userLocation">A Pathpoint object representing the user's current location.</param>
    /// <returns>A tuple containing the minimum distance to the nearest route segment and the minimum bearing difference between the user's heading and the segment's heading.</returns>
    public SegmentDistanceAndBearing CalculateMinDistanceAndBearing(Pathpoint userLocation)
    {
        double userHeadingSum = 0;
        double userHeading = 0;
        double segmentHeading = 0;
        if (UserLocationHistory.Count > 1)
        {
            for (int i = 1; i < UserLocationHistory.Count; i++)
            {
                userHeadingSum += CalculateBearing(UserLocationHistory[i - 1], UserLocationHistory[i]);
            }
            userHeading = userHeadingSum / (UserLocationHistory.Count - 1);
        }

        double minDistanceToSegment = double.MaxValue;
        double minBearingDifference = double.MaxValue;
        int closestSegmentIndex = lastClosestSegmentIndex;

        int startSegmentIndex = Math.Max(0, lastClosestSegmentIndex - config.searchRadius);
        int endSegmentIndex = Math.Min(Pathpoints.Count - 1, lastClosestSegmentIndex + config.searchRadius);

        for (int i = startSegmentIndex; i < endSegmentIndex; i++)
        {
            double distanceToSegment = CalculateDistanceToSegment(userLocation, Pathpoints[i], Pathpoints[i + 1]);
            //segmentHeading = CalculateBearing(Pathpoints[i], Pathpoints[i + 1]);
            //minBearingDifference = CalculateBearingDifference(segmentHeading, userHeading);

            //Debug.Log($"[{i}-{i+1}] Lat1:{Pathpoints[i].Latitude} Lon1:{Pathpoints[i].Longitude} Lat2:{Pathpoints[i + 1].Latitude} Lon2:{Pathpoints[i+1].Longitude} Distance-Segment:{distanceToSegment} Segment-Heading: {segmentHeading} Diff: {minBearingDifference}");
            if (distanceToSegment < minDistanceToSegment)
            {
                minDistanceToSegment = distanceToSegment;
                closestSegmentIndex = i;

                segmentHeading = CalculateBearing(Pathpoints[i], Pathpoints[i + 1]);
                //minBearingDifference = Math.Abs(segmentHeading - userHeading);
                minBearingDifference = CalculateBearingDifference(segmentHeading, userHeading);
            }
        }

        //Debug.Log($"CalculateMinDistanceAndBearing:{closestSegmentIndex} minDistanceToSegment:{minDistanceToSegment} minBearingDifference:{minBearingDifference} userHeading:{userHeading} segmentHeading:{userHeading}");

        lastClosestSegmentIndex = closestSegmentIndex;

        return new SegmentDistanceAndBearing
        {
            MinDistanceToSegment = minDistanceToSegment,
            MinBearingDifference = minBearingDifference,
            UserHeading = userHeading,
            SegmentHeading = segmentHeading,
            ClosestSegmentIndex = lastClosestSegmentIndex
        };
    }


    private Pathpoint LastValidLocation = null;
    private double? lastValidBearing = null;
    private NavigationIssue? lastNavigationIssue = null;

    /// <summary>
    /// Determines if the user is off-track based on their distance to the nearest route segment and the difference between their heading and the segment's heading.
    /// </summary>
    /// <param name="userLocation">A Pathpoint object representing the user's current location.</param>
    /// <param name="segmentInfo">A structure containing information about the user's distance to the nearest route segment and bearing differences.</param>
    /// <param name="lastPOIIndex">The index of the last Point of Interest (POI) encountered by the user, if applicable.</param>
    /// <returns>A tuple containing a boolean indicating if the user is off-track and a navigation issue enum describing the type of navigation issue, if any.</returns>
    public (bool isOffTrack, NavigationIssue issue) IsUserOffTrack(Pathpoint userLocation, SegmentDistanceAndBearing segmentInfo, int? lastPOIIndex = null, bool currentlyOfftrack = false)
    {
        bool isOfftrack = false;
        NavigationIssue issue = NavigationIssue.Deviation;

        // is the user walking in the wrong direction?
        if (segmentInfo.MinBearingDifference > config.oppositeHeadingThreshold && LastValidLocation != null)
        {
            double distanceToValid = HaversineDistance(userLocation, LastValidLocation);
            if (LastValidLocation != null && distanceToValid > config.oppositeDistanceThreshold)
            {
                isOfftrack = true;
                issue = NavigationIssue.WrongDirection;
                Debug.Log($"WrongDirection? Confirmed.---------");
            }
            Debug.Log($"WrongDirection? DistanceToValid: {distanceToValid} > DistanceInWrongDirection:{config.oppositeDistanceThreshold} ?");
        }
        // is the user walking at a distance offTrackThreshould from the route (e.g., walking in parallel to the route)
        else if (segmentInfo.MinDistanceToSegment > config.offTrackThreshold ||
                (currentlyOfftrack && segmentInfo.MinDistanceToSegment > config.offTrackThresholdEnd))
        {
            isOfftrack = true;

            Debug.Log($"Deviation -> MinDistanceToSegment: {segmentInfo.MinDistanceToSegment} > offTrackThreshold:{config.offTrackThreshold} ?");
            Debug.Log($"Deviation -> currentlyOfftrack: {currentlyOfftrack} && MinDistanceToSegment: {segmentInfo.MinDistanceToSegment} > backOnTrackThreshold:{config.offTrackThresholdEnd} ?");

            //double? expectedBearing = CalculateMovingBearing((int)segmentInfo.ClosestSegmentIndex - 1, 2);
            //double poiBearing = CalculateBearing(Pathpoints[(int)segmentInfo.StartSegmentIndex], userLocation);
            //Debug.Log($"Deviation -> SegmentMovingBEaring: {expectedBearing} POIBearing: {poiBearing}");
        }
        // is the user featuring a bearing difference larger than the threshould? and away from the route
        else if (segmentInfo.MinBearingDifference > config.deviationHeadingThreshold && segmentInfo.MinDistanceToSegment > config.deviationDistanceThreshold)
        {

            //double? expectedBearing = CalculateMovingBearing((int)segmentInfo.ClosestSegmentIndex -1, 2);
            //double poiBearing = CalculateBearing(Pathpoints[(int)segmentInfo.StartSegmentIndex], userLocation);

            isOfftrack = true;

            Debug.Log($"Deviation -> MinBearingDifference: {segmentInfo.MinBearingDifference} > deviationHeadingThreshold:{config.deviationHeadingThreshold} && " +
                                    $"MinDistanceToSegment: {segmentInfo.MinDistanceToSegment} > offTrackThreshold:{config.deviationDistanceThreshold}  ?");

            //Debug.Log($"Deviation -> SegmentMovingBEaring: {expectedBearing} POIBearing: {poiBearing}");
        }

        // Let's keep the last valid location
        if (segmentInfo.MinBearingDifference < config.deviationDistanceThreshold && segmentInfo.MinDistanceToSegment < config.deviationDistanceThreshold)
        {
            LastValidLocation = userLocation;
            //Debug.Log("Last valid location!");
        }

        // Let's keep the last valid direction
        if (segmentInfo.MinBearingDifference < config.deviationHeadingThreshold)
        {
            lastValidBearing = segmentInfo.UserHeading;
        }

        // We try to detect the type of off-track in case the user just left a decision point (POI)
        if (lastPOIIndex != null &&
           // issue == NavigationIssue.Deviation &&
            (segmentInfo.MinBearingDifference > config.deviationHeadingThreshold ||
                                     segmentInfo.MinDistanceToSegment > config.deviationDistanceThreshold))
        {
            double? expectedBearing = CalculateMovingBearing((int)lastPOIIndex, 3);
            double? expectedBearing5 = CalculateMovingBearing((int)lastPOIIndex, 5);
            double poiBearing = CalculateBearing(Pathpoints[(int)lastPOIIndex], userLocation);

            //if (expectedBearing != null && CalculateBearingDifference(segmentInfo.UserHeading, (double)expectedBearing) < config.wrongTurnHeadingThreshold) 
            //{
            //    issue = NavigationIssue.WrongTurn;
            //}
            //else if (CalculateBearingDifference(segmentInfo.UserHeading, lastValidBearing.Value) < config.deviationHeadingThreshold) 
            //{
            //    issue = NavigationIssue.MissedTurn;
            //}

            var diffBearingValid = CalculateBearingDifference(segmentInfo.UserHeading, lastValidBearing.Value);
            var diffExpectedBearing = CalculateBearingDifference(segmentInfo.UserHeading, (double)expectedBearing);
            var diffExpectedBearing5 = CalculateBearingDifference(segmentInfo.UserHeading, (double)expectedBearing5);
            //var diffPOIBearing = CalculateBearingDifference(poiBearing, segmentInfo.SegmentHeading);
            //var diffPOIExpectedBearing = CalculateBearingDifference(poiBearing, (double)expectedBearing);

            if (expectedBearing != null && diffExpectedBearing > config.wrongTurnHeadingThreshold)
               // segmentInfo.MinBearingDifference > config.wrongTurnHeadingThreshold)
            {
                issue = NavigationIssue.WrongTurn;
                isOfftrack = true;
            }
            else if (diffBearingValid > config.deviationHeadingThreshold ||
                expectedBearing5 != null && diffExpectedBearing5 > config.deviationHeadingThreshold)
            {
                issue = NavigationIssue.MissedTurn;
                isOfftrack = true;
            }
            Debug.Log($"**** [{lastPOIIndex}] UserHeading: {segmentInfo.UserHeading} DiffBearingValid: {diffBearingValid}  DiffExpectedBearing: {diffExpectedBearing} POIBearing: {poiBearing} ExpectedBearing: {expectedBearing} ExpectedBearing5: {expectedBearing5} SegmentHeading: {segmentInfo.SegmentHeading} ");
            //Debug.Log($"**** DiffBearingValid: {diffBearingValid}  DiffExpectedBearing: {diffExpectedBearing} " +
            //    $"DiffPOIBearing: {diffPOIBearing} DiffPOIExpectedBearing: {diffPOIExpectedBearing} POIBearing: {poiBearing} ExpectedBearing: {expectedBearing} ");

        }
        //TODO: We can setup a threshould to not be OffTrack, meaning different threshoulds to be OffTrack, and then to go back to be on Track 
        lastNavigationIssue = isOfftrack ? issue : null;

        return (isOfftrack , issue);
    }

    Pathpoint lastOffTrackPathpoint = null;
    /// <summary>
    /// Determines if the user is off-track based on their distance to the nearest route segment and the difference between their heading and the segment's heading.
    /// </summary>
    /// <param name="userLocation">A Pathpoint object representing the user's current location.</param>
    /// <param name="segmentInfo">A structure containing information about the user's distance to the nearest route segment and bearing differences.</param>
    /// <param name="lastPOIIndex">The index of the last Point of Interest (POI) encountered by the user, if applicable.</param>
    /// <returns>A tuple containing a boolean indicating if the user is off-track and a navigation issue enum describing the type of navigation issue, if any.</returns>
    public (bool isOffTrack, NavigationIssue issue) IsUserOffTrackNew(Pathpoint userLocation, SegmentDistanceAndBearing segmentInfo, int? lastPOIIndex = null, bool currentlyOfftrack = false)
    {
        bool isOfftrack = false;
        NavigationIssue issue = NavigationIssue.Deviation;
        lastOffTrackPathpoint = userLocation;

        bool aboveAwayFromSegmentThreshold = IsDistanceAboveThreshold(segmentInfo.MinDistanceToSegment, config.offTrackThreshold, Pathpoints[segmentInfo.ClosestSegmentIndex], userLocation);
        bool aboveDeviationDistanceThreshold = IsDistanceAboveThreshold(segmentInfo.MinDistanceToSegment, config.deviationDistanceThreshold, Pathpoints[segmentInfo.ClosestSegmentIndex], userLocation);

        // Check if the user is walking in the wrong direction
        if (segmentInfo.MinBearingDifference > config.oppositeHeadingThreshold && LastValidLocation != null)
        {
            double distanceToValid = HaversineDistance(userLocation, LastValidLocation);
            bool metGoingOppossiteDistanceThreshold = IsDistanceAboveThreshold(distanceToValid, config.oppositeDistanceThreshold, LastValidLocation, userLocation);

            if (metGoingOppossiteDistanceThreshold)
            {
                isOfftrack = true;
                issue = NavigationIssue.WrongDirection;
                Debug.Log("WrongDirection? Confirmed.---------");
            }
            Debug.Log($"WrongDirection? DistanceToValid: {distanceToValid} > DistanceInWrongDirection:{config.oppositeDistanceThreshold} ?");
        }

        // Check if the user is walking at a distance offTrackThreshould from the route
        else if (aboveAwayFromSegmentThreshold || (currentlyOfftrack && segmentInfo.MinDistanceToSegment > config.offTrackThresholdEnd))
        {
            isOfftrack = true;
            Debug.Log($"Deviation -> MinDistanceToSegment: {segmentInfo.MinDistanceToSegment} > offTrackThreshold:{config.offTrackThreshold} ?");
            Debug.Log($"Deviation -> currentlyOfftrack: {currentlyOfftrack} && MinDistanceToSegment: {segmentInfo.MinDistanceToSegment} > backOnTrackThreshold:{config.offTrackThresholdEnd} ?");
        }

        // Check if the user is featuring a bearing difference larger than the threshold and away from the route
        else if (segmentInfo.MinBearingDifference > config.deviationHeadingThreshold &&
                 aboveDeviationDistanceThreshold)
        {
            isOfftrack = true;
            Debug.Log($"Deviation -> MinBearingDifference: {segmentInfo.MinBearingDifference} > deviationHeadingThreshold:{config.deviationHeadingThreshold} && " +
                                  $"MinDistanceToSegment: {segmentInfo.MinDistanceToSegment} > offTrackThreshold:{config.deviationDistanceThreshold} ?");
        }

        // Keep the last valid location
        if (segmentInfo.MinBearingDifference < config.deviationDistanceThreshold &&
            !aboveDeviationDistanceThreshold)
        {
            LastValidLocation = userLocation;
        }

        // Keep the last valid direction
        if (segmentInfo.MinBearingDifference < config.deviationHeadingThreshold)
        {
            lastValidBearing = segmentInfo.UserHeading;
        }

        // Detect the type of off-track in case the user just left a decision point (POI)
        if (lastPOIIndex != null && (segmentInfo.MinBearingDifference > config.deviationHeadingThreshold || segmentInfo.MinDistanceToSegment > config.deviationDistanceThreshold))
        {
            double? expectedBearing = CalculateMovingBearing((int)lastPOIIndex, 3);
            double? expectedBearing5 = CalculateMovingBearing((int)lastPOIIndex, 5);
            double poiBearing = CalculateBearing(Pathpoints[(int)lastPOIIndex], userLocation);

            var diffBearingValid = CalculateBearingDifference(segmentInfo.UserHeading, lastValidBearing.Value);
            var diffExpectedBearing = CalculateBearingDifference(segmentInfo.UserHeading, (double)expectedBearing);
            var diffExpectedBearing5 = CalculateBearingDifference(segmentInfo.UserHeading, (double)expectedBearing5);

            if (segmentInfo.MinBearingDifference > config.oppositeHeadingThreshold)
            {
                issue = NavigationIssue.WrongDirection;
                isOfftrack = true;
            }
            else if (expectedBearing != null && diffExpectedBearing > config.wrongTurnHeadingThreshold)
            {
                issue = NavigationIssue.WrongTurn;
                isOfftrack = true;
            }
            else if (diffBearingValid > config.deviationHeadingThreshold || (expectedBearing5 != null && diffExpectedBearing5 > config.deviationHeadingThreshold))
            {
                issue = NavigationIssue.MissedTurn;
                isOfftrack = true;
            }
            Debug.Log($"**** [{lastPOIIndex}] UserHeading: {segmentInfo.UserHeading} DiffBearingValid: {diffBearingValid}  DiffExpectedBearing: {diffExpectedBearing} POIBearing: {poiBearing} ExpectedBearing: {expectedBearing} ExpectedBearing5: {expectedBearing5} SegmentHeading: {segmentInfo.SegmentHeading} ");
        }

        lastNavigationIssue = isOfftrack ? issue : null;

        return (isOfftrack, issue);
    }

    double correctHeadingDistance = 0;
    Pathpoint firstCorrectHeadingPathpoint = null;
    

    public bool IsUserOnTrack(Pathpoint userLocation, SegmentDistanceAndBearing segmentInfo, NavigationIssue issue)
    {
        bool isOnTrack = false; 

        switch (issue)
        {
            case NavigationIssue.Deviation:
                // Handle the case when there are no navigation issues
                // Check if the user is within a certain distance threshold from the route segment
                // Example code:
                // isOnTrack = IsDistanceBelowThreshold(userLocation, threshold);

                bool isWithinDistanceOfftrackThreshold = !IsDistanceAboveThreshold(segmentInfo.MinDistanceToSegment, config.offTrackThresholdEnd, Pathpoints[segmentInfo.ClosestSegmentIndex], userLocation);
                bool isWithinBearingOfftrackThreshold = segmentInfo.MinBearingDifference < config.offTrackHeadingThresholdEnd;

                bool isWithinDistanceDeviationThreshold = !IsDistanceAboveThreshold(segmentInfo.MinDistanceToSegment, config.deviationDistanceThresholdEnd, Pathpoints[segmentInfo.ClosestSegmentIndex], userLocation);
                bool isWithinBearingDeviationThreshold = segmentInfo.MinBearingDifference < config.deviationHeadingThreshold;
                
                

                // Check if both distance and bearing difference are within acceptable ranges
                if (isWithinDistanceOfftrackThreshold && isWithinBearingOfftrackThreshold) {
                    isOnTrack = true;
                }
                else if (isWithinDistanceDeviationThreshold && isWithinBearingDeviationThreshold)
                {
                    isOnTrack = true;
                }

                break;
            case NavigationIssue.WrongDirection:
                // Handle the case when the user is in the wrong direction
                // Check if the user's direction aligns with the route segment's direction
                // Example code:
                // isOnTrack = IsDirectionAlignedWithSegment(userLocation, segmentDirection);

                if (segmentInfo.MinBearingDifference < config.maxCorrectHeadingThreshold)
                {
                    if (firstCorrectHeadingPathpoint == null)
                    {
                        firstCorrectHeadingPathpoint = userLocation;
                        correctHeadingDistance = 0;
                    }
                    else
                    {
                        double distanceToFirst = HaversineDistance(userLocation, firstCorrectHeadingPathpoint);
                        correctHeadingDistance += distanceToFirst;

                        bool isAboveCorrectDistanceThreshold =  IsDistanceAboveThreshold(distanceToFirst, config.oppositeDistanceThresholdEnd, firstCorrectHeadingPathpoint, userLocation);

                        if (isAboveCorrectDistanceThreshold)
                        {
                            isOnTrack = true;
                            correctHeadingDistance = 0;
                            firstCorrectHeadingPathpoint = null;
                        }
                    }
                }

                break;
            case NavigationIssue.MissedTurn:
            case NavigationIssue.WrongTurn:
                // Handle the case when the user missed a turn
                // Check if the user's current location is close to the missed turn point
                // Example code:
                // isOnTrack = IsCloseToMissedTurnPoint(userLocation, missedTurnPoint);

                // to be back on track, it can not be above the 'onPOI distance', otherwise the system will detect off track (wrong decision)
                // when leaving the point, but then immediately go back on track, since the threshoulds can be higher for deviation.
                // so here we are more stringent, and subjective the user to get to distances shorter than the the OnPOI threshould used.

                double threshold = config.onTargetMinDistanceThreshold;
                bool isWithinOnPOIDistanceThreshold = !IsDistanceAboveThreshold(segmentInfo.MinDistanceToSegment, threshold, Pathpoints[segmentInfo.ClosestSegmentIndex], userLocation);
                bool isWithinBearingThreshold = segmentInfo.MinBearingDifference < config.deviationHeadingThreshold;

                if (isWithinOnPOIDistanceThreshold && isWithinBearingThreshold)
                {
                    isOnTrack = true;
                }

                break;
            // Add more cases as needed for other navigation issues
            default:
                // Handle any other navigation issues or unknown issues
                break;
        }

        return isOnTrack;
    }




    private double minDistanceToPOI = float.MaxValue;
    private int distanceIncreaseCount = 0;
    private const int distanceIncreaseCountThreshold = 5; // Number of consecutive distance increases before considering the landmark skipped
    private const int distanceIncreaseThreshold = 10; // Distance in meters increased before considering landmark increased
    private int lastPOIIndex = -1;
    /// <summary>
    /// Checks if the landmark has been skipped based on the current distance.
    /// </summary>
    /// <param name="distance">The current distance to the landmark.</param>
    /// <returns>True if the landmark has been skipped; otherwise, false.</returns>
    public bool CheckLandmarkSkipped(double distance, int POIIndex)
    {
        if (POIIndex != lastPOIIndex)
        {
            minDistanceToPOI = float.MaxValue;
            distanceIncreaseCount = 0;
            lastPOIIndex = POIIndex;
        }

        if (distance < minDistanceToPOI)
        {
            // Update the minimum distance
            minDistanceToPOI = distance;
            distanceIncreaseCount = 0; // Reset the distance increase counter
        }
        else
        {
            distanceIncreaseCount++; // Increment the distance increase counter

            if (distanceIncreaseCount >= distanceIncreaseCountThreshold || (distance- minDistanceToPOI) > distanceIncreaseThreshold)
            {
                // Landmark skipped
                // Reset the minimum distance and distance increase counter
                minDistanceToPOI = float.MaxValue;
                distanceIncreaseCount = 0;

                return true;
            }
        }

        return false;
    }

    public int GetPathpointIndexById(int pathpointId)
    {
        int index = -1;
        for (int i = 0; i < Pathpoints.Count; i++)
        {
            if (Pathpoints[i].Id == pathpointId)
            {
                index = i;
                break;
            }
        }
        return index;
    }


    
    
    /// <summary>
    /// Checks if the distance between the user and the reference pathpoint is above a weighted threshold.
    /// </summary>
    /// <param name="distance">The distance between the user and the reference pathpoint.</param>
    /// <param name="threshold">The fixed threshold for off-track detection.</param>
    /// <param name="recPathpoint">The reference pathpoint with accuracy information.</param>
    /// <param name="userPathpoint">The user's current pathpoint with accuracy information.</param>
    /// <param name="weightedThreshold">The computed weighted threshold.</param>
    /// <returns>True if the distance is above the weighted threshold, false otherwise.</returns>
    public bool IsDistanceAboveThreshold(double distance, double threshold, Pathpoint recPathpoint, Pathpoint userPathpoint, out double weightedThreshold)
    {
        // Compute dynamic threshold based on combined uncertainty and fixed threshold
        double combinedAccuracy = recPathpoint.Accuracy + userPathpoint.Accuracy;
        weightedThreshold = threshold + combinedAccuracy * config.adaptiveOfftrackThresholdScalingFactor;

        // Compare user distance with weighted threshold
        return distance > weightedThreshold;
    }


    /// <summary>
    /// Checks if the distance between the user and the reference pathpoint is above a weighted threshold.
    /// </summary>
    /// <param name="distance">The distance between the user and the reference pathpoint.</param>
    /// <param name="threshold">The fixed threshold for off-track detection.</param>
    /// <param name="recPathpoint">The reference pathpoint with accuracy information.</param>
    /// <param name="userPathpoint">The user's current pathpoint with accuracy information.</param>
    /// <returns>True if the distance is above the weighted threshold, false otherwise.</returns>
    public bool IsDistanceAboveThreshold(double distance, double threshold, Pathpoint recPathpoint, Pathpoint userPathpoint)
    {
        // Compute weighted threshold
        double weightedThreshold;
        return IsDistanceAboveThreshold(distance, threshold, recPathpoint, userPathpoint, out weightedThreshold);
    }




    ///// <summary>
    ///// Calculates the moving bearing of 3 consecutive points given an index.
    ///// </summary>
    ///// <param name="index">An integer representing the index in the list of Pathpoints.</param>
    ///// <returns>The moving bearing in degrees.</returns>
    //public double? CalculateMovingBearing(int index)
    //{
    //    if (index > Pathpoints.Count - 3)
    //    {
    //        return null;
    //    }

    //    Pathpoint point1 = Pathpoints[index];
    //    Pathpoint point2 = Pathpoints[index + 1];
    //    Pathpoint point3 = Pathpoints[index + 2];

    //    double bearing1 = CalculateBearing(point1, point2) * Math.PI / 180; // converting to radians
    //    double bearing2 = CalculateBearing(point2, point3) * Math.PI / 180; // converting to radians

    //    // convert the bearings to cartesian coordinates
    //    double x = Math.Cos(bearing1) + Math.Cos(bearing2);
    //    double y = Math.Sin(bearing1) + Math.Sin(bearing2);

    //    // compute the average bearing
    //    double avgBearing = Math.Atan2(y, x);

    //    // convert back to degrees
    //    avgBearing = avgBearing * 180 / Math.PI;
    //    avgBearing = (avgBearing + 360) % 360;  // ensuring the bearing is within [0, 360)

    //    return avgBearing;
    //}

    /// <summary>
    /// Calculates the moving bearing average given an index and window size.
    /// </summary>
    /// <param name="index">An integer representing the index in the list of Pathpoints.</param>
    /// <param name="windowSize">The size of the window for calculating the average.</param>
    /// <returns>The moving bearing average in degrees.</returns>
    public double? CalculateMovingBearing(int index, int windowSize = 3)
    {
        if (index > Pathpoints.Count - windowSize)
        {
            return null;
        }

        double totalX = 0;
        double totalY = 0;

        for (int i = 0; i < windowSize - 1; i++)
        {
            Pathpoint point1 = Pathpoints[index + i];
            Pathpoint point2 = Pathpoints[index + i + 1];

            double bearing = CalculateBearing(point1, point2) * Math.PI / 180; // converting to radians
            totalX += Math.Cos(bearing);
            totalY += Math.Sin(bearing);
        }

        // compute the average bearing
        double avgBearing = Math.Atan2(totalY, totalX);

        // convert back to degrees
        avgBearing = avgBearing * 180 / Math.PI;
        avgBearing = (avgBearing + 360) % 360;  // ensuring the bearing is within [0, 360)

        return avgBearing;
    }


    /// Calculates the Haversine distance between two coordinates.
    /// </summary>
    /// <param name="coord1">The first coordinate as a Pathpoint object.</param>
    /// <param name="coord2">The second coordinate as a Pathpoint object.</param>
    /// <returns>The Haversine distance in meters between the two coordinates.</returns>
    public static double HaversineDistance(Pathpoint coord1, Pathpoint coord2)
    {
        double lat1 = coord1.Latitude * Math.PI / 180;
        double lon1 = coord1.Longitude * Math.PI / 180;
        double lat2 = coord2.Latitude * Math.PI / 180;
        double lon2 = coord2.Longitude * Math.PI / 180;

        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EARTH_RADIUS * c;
    }

    /// <summary>
    /// Calculates the initial bearing from one point to another.
    /// </summary>
    /// <param name="point1">A Pathpoint object representing the starting point.</param>
    /// <param name="point2">A Pathpoint object representing the destination point.</param>
    /// <returns>The initial bearing in degrees from point1 to point2.</returns>
    public double CalculateBearing(Pathpoint point1, Pathpoint point2)
    {
        double lat1 = point1.Latitude * Math.PI / 180;
        double lon1 = point1.Longitude * Math.PI / 180;
        double lat2 = point2.Latitude * Math.PI / 180;
        double lon2 = point2.Longitude * Math.PI / 180;

        double dLon = lon2 - lon1;
        double y = Math.Sin(dLon) * Math.Cos(lat2);
        double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

        double bearing = Math.Atan2(y, x) * 180 / Math.PI;
        bearing = (bearing + 360) % 360;

        return bearing;
    }

    /// <summary>
    /// Calculates the distance from a point to the nearest point on a line segment defined by two points.
    /// </summary>
    /// <param name="point">A Pathpoint object representing the point of interest.</param>
    /// <param name="segmentStart">A Pathpoint object representing the start of the line segment.</param>
    /// <param name="segmentEnd">A Pathpoint object representing the end of the line segment.</param>
    /// <returns>The distance in meters from the point to the nearest point on the line segment.</returns>
    private double CalculateDistanceToSegment(Pathpoint point, Pathpoint segmentStart, Pathpoint segmentEnd)
    {
        // Calculate the cross-track distance
        double crossTrackDistance = CrossTrackDistance(point, segmentStart, segmentEnd);

        // Calculate the along-track distance
        double alongTrackDistance = AlongTrackDistance(point, segmentStart, segmentEnd);

        double distanceSegmentStartToEnd = HaversineDistance(segmentStart, segmentEnd);

        // Check if the point is within the segment or if it's projecting outside of it
        if (alongTrackDistance < 0)
        {
            return HaversineDistance(point, segmentStart);
        }
        else if (alongTrackDistance > distanceSegmentStartToEnd)
        {
            return HaversineDistance(point, segmentEnd);
        }

        return Math.Abs(crossTrackDistance);
    }


    /// <summary>
    /// Calculates the cross-track distance from a point to a line segment defined by two points.
    /// </summary>
    /// <param name="point">A Pathpoint object representing the point of interest.</param>
    /// <param name="segmentStart">A Pathpoint object representing the start of the line segment.</param>
    /// <param name="segmentEnd">A Pathpoint object representing the end of the line segment.</param>
    /// <returns>The cross-track distance in meters from the point to the line segment.</returns>
    private double CrossTrackDistance(Pathpoint point, Pathpoint segmentStart, Pathpoint segmentEnd)
    {
        double distanceSegmentStartToPoint = HaversineDistance(segmentStart, point);
        double bearingSegmentStartToPoint = CalculateBearing(segmentStart, point);
        double bearingSegmentStartToEnd = CalculateBearing(segmentStart, segmentEnd);

        double crossTrackDistance = Math.Asin(Math.Sin(distanceSegmentStartToPoint / EARTH_RADIUS) *
                                              Math.Sin(bearingSegmentStartToPoint - bearingSegmentStartToEnd)) *
                                    EARTH_RADIUS;

        return crossTrackDistance;
    }

    /// <summary>
    /// Calculates the along-track distance from the start of a line segment to a point projected on the line segment.
    /// </summary>
    /// <param name="point">A Pathpoint object representing the point of interest.</param>
    /// <param name="segmentStart">A Pathpoint object representing the start of the line segment.</param>
    /// <param name="segmentEnd">A Pathpoint object representing the end of the line segment.</param>
    /// <returns>The along-track distance in meters from the start of the line segment to the projected point.</returns>
    private double AlongTrackDistance(Pathpoint point, Pathpoint segmentStart, Pathpoint segmentEnd)
    {
        double distanceSegmentStartToPoint = HaversineDistance(segmentStart, point);
        double crossTrackDistance = CrossTrackDistance(point, segmentStart, segmentEnd);

        double alongTrackDistance = Math.Acos(Math.Cos(distanceSegmentStartToPoint / EARTH_RADIUS) /
                                              Math.Cos(crossTrackDistance / EARTH_RADIUS)) *
                                    EARTH_RADIUS;

        return alongTrackDistance;
    }


}
