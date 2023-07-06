using System;
using System.Collections.Generic;
using UnityEngine;

public class ValidationArgs: EventArgs
{
    public Pathpoint Point;
    public double MinDistanceToSegment { get; set; }
    public double MinBearingDifference { get; set; }
    public double UserHeading { get; set; }
    public double SegmentHeading { get; set; }
    public LocationUtils.NavigationIssue Issue { get; set; }

    public ValidationArgs(Pathpoint point, double minDistanceToSegment, double minBearingDifference, double userHeading, double segmentHeading, LocationUtils.NavigationIssue issue = LocationUtils.NavigationIssue.None)
    {
        Point = point;
        MinDistanceToSegment = minDistanceToSegment;
        MinBearingDifference = minBearingDifference;
        UserHeading = userHeading;
        SegmentHeading = segmentHeading;
        Issue = issue;
    }
}

public class EventArgs<T> : EventArgs
{
    public T Data { get; set; }

    public EventArgs(T data)
    {
        Data = data;
    }
}

public class POIArgs : EventArgs
{
    public Pathpoint Point;
    public double Distance { get; set; }

    public POIArgs(Pathpoint point, double distance)
    {
        Point = point;
        Distance = distance;
    }
}

public class WalkingStatusArgs : EventArgs
{
    public Pathpoint Point;
    public bool IsWalking { get; set; }
    public double Pace { get; set; }

    public WalkingStatusArgs(Pathpoint point, bool isWalking, double pace)
    {
        Point = point;
        IsWalking = isWalking;
        Pace = pace;
    }
}

public class LocationChangedArgs : EventArgs
{

    public Pathpoint Point;
    public double MinDistanceToSegment { get; set; }
    public double MinBearingDifference { get; set; }
    public double UserHeading { get; set; }
    public double SegmentHeading { get; set; }
    public double ClosestSegmentIndex { get; set; }    
    public bool IsWalking { get; set; }
    public double Pace { get; set; }

    public LocationChangedArgs(Pathpoint point, double minDistanceToSegment, double minBearingDifference, double userHeading, double segmentHeading, int closestSegmentIndex, bool isWalking, double pace)
    {
        Point = point;
        MinDistanceToSegment = minDistanceToSegment;
        MinBearingDifference = minBearingDifference;
        UserHeading = userHeading;
        SegmentHeading = segmentHeading;
        ClosestSegmentIndex = closestSegmentIndex;
        IsWalking = isWalking;
        Pace = pace;
    }

}


public class PathpointInvalidArgs : EventArgs
{
    public Pathpoint Point;
    public double Accuracy { get; set; }
    public double Speed { get; set; }

    public PathpointInvalidArgs(Pathpoint point, double accuracy, double speed)
    {
        Point = point;
        Accuracy = accuracy;
        Speed = speed;
    }
}


public class POIWatcher : PersistentLazySingleton<POIWatcher>
{        
    public event EventHandler<ValidationArgs> OnOffTrack;
    public event EventHandler<ValidationArgs> OnAlongTrack;
    public event EventHandler<POIArgs> OnLeftPOI;
    public event EventHandler<POIArgs> OnEnteredPOI;
    public event EventHandler<POIArgs>  OnArrived;
    public event EventHandler<WalkingStatusArgs> OnWalkStatusChange;
    public event EventHandler<PathpointInvalidArgs> OnInvalidPathpoint;
    public event EventHandler<EventArgs<string>> OnLog;
    public event EventHandler<LocationChangedArgs> OnUserLocationChanged;

    private Pathpoint CurrentTargetPOI;
    private List<Pathpoint> POIList;
    private LocationUtils routeValidation;
    public int CurrentPOIIndex = -1;
    public bool IsWalking = false;

    public enum POIState { None, OnPOI, LeftPOI, OffTrack, OnTrack, Arrived, Invalid }
    private POIState currentState = POIState.None;
    private int LocationCount = 0;

    private WalkingDetector walkingDetector;
    LocationQualityControl locationQualityControl;

    public void InitialiseWatcher(LocationUtils routeValidation)
    {
        this.routeValidation = routeValidation;
        walkingDetector = new WalkingDetector(20);
        locationQualityControl = new LocationQualityControl(expectedGPSAccuracy: 20, slidingWindowSize: 5, maxSpeed: 10, maxAccuracy: 30);

        currentState = POIState.None;
        LocationCount = 0;
        CurrentPOIIndex = -1;
        CurrentTargetPOI = null;
        IsWalking = false;
    }

    public void LoadTargetPOIs(List <Pathpoint> list)
    {
        POIList = list;
    }

    public void NextPOI()
    {
        CurrentPOIIndex++;
        CurrentTargetPOI = POIList[CurrentPOIIndex];
    }

    public int GetLocationIndex()
    {
        return LocationCount;
    }    

    public PathpointLog UpdateUserLocation(Pathpoint pathpoint)
    {
        bool isWalking = false;
        bool justLeft = false;
        double walkPace = 0;

        LocationCount++;

        (Pathpoint userLocation, double valAccuracy, double valSpeed) = locationQualityControl.ProcessLocation(pathpoint);


        if (userLocation == null) {
            OnInvalidPathpoint?.Invoke(this, new PathpointInvalidArgs(pathpoint, valAccuracy, valSpeed));
            return null;
        }

        PathpointLog pathpointLog = new PathpointLog(userLocation);
        pathpointLog.Id = LocationCount;
        pathpointLog.SegPOIStartId = GetPathpointIdByIndex(CurrentPOIIndex - 1);
        pathpointLog.SegPOIEndId = GetPathpointIdByIndex(CurrentPOIIndex);

        (isWalking, walkPace) = walkingDetector.IsWalking(pathpoint);
        if (IsWalking != isWalking)
        {
            OnWalkStatusChange?.Invoke(this, new WalkingStatusArgs(userLocation, isWalking, walkPace));
            IsWalking = isWalking;
        }


        var currentDistance = LocationUtils.HaversineDistance(userLocation, CurrentTargetPOI);

        if (routeValidation.IsPathpointOnTarget(userLocation, CurrentTargetPOI))
        {
            if (CurrentPOIIndex+1 == POIList.Count)
            {
                currentState = POIState.Arrived;
                OnArrived?.Invoke(this, new POIArgs(userLocation, currentDistance));
            }
            else if (currentState != POIState.OnPOI)
            {
                currentState = POIState.OnPOI;
                OnEnteredPOI?.Invoke(this, new POIArgs(userLocation, currentDistance));
            }

            pathpointLog.TargetPOIId = CurrentTargetPOI.Id;

            OnLog?.Invoke(this, new EventArgs<string>($"Still on On Target: " + currentDistance));
        }
        else
        {
            if (currentState == POIState.OnPOI)
            {
                justLeft = true;
                currentState = POIState.LeftPOI;
                //OnLeftPOI?.Invoke(this, new EventArgs<double>(d));
            }
        }


        // compute metrics
        routeValidation.UpdateUserLocationHistory(userLocation);
        (double minDistanceToSegment, double minBearingDifference, double userHeading, double segmentHeading, int closestSegmentIndex) = routeValidation.CalculateMinDistanceAndBearing(userLocation);        

        Debug.Log($"[{LocationCount}] {CurrentPOIIndex} ({Math.Round(pathpoint.Accuracy).ToString().PadLeft(2)}|{Math.Round(userLocation.Accuracy).ToString().PadRight(2)}) - distance: {Math.Round(currentDistance, 3).ToString("F3")} S-Index: {closestSegmentIndex} S-Bear: {Math.Round(segmentHeading, 3).ToString("F3")} U-Bear: {Math.Round(userHeading,3).ToString("F3")} | Min-Seg: {Math.Round(minDistanceToSegment,3).ToString("F3")} Min-Bear: {Math.Round(minBearingDifference,3).ToString("F3")} | isWalking: {isWalking} pace: {Math.Round(walkPace,3).ToString("F3")}");




        // We are not on a POI and we stopped. We do not check off-track then
        if (currentState != POIState.OnPOI && !isWalking)
        {
            return pathpointLog;
        }

        // We check if the landmark has possibly been skipped
        bool landarkSkipped = routeValidation.CheckLandmarkSkipped(currentDistance, CurrentPOIIndex);

        // Compute whether the user is off-track
        // Are we off-track?
        int? lastPOIIndex = null;
        if (justLeft)
        {
            lastPOIIndex = CurrentPOIIndex;
        }
        (bool offTrackDetected, LocationUtils.NavigationIssue issue) = routeValidation.IsUserOffTrack(userLocation, lastPOIIndex);

        if (currentState != POIState.OnPOI && currentState != POIState.Arrived)
        {            

            if (currentState != POIState.OffTrack && offTrackDetected)
            {
                currentState = POIState.OffTrack;
                OnOffTrack?.Invoke(this, new ValidationArgs(userLocation, minDistanceToSegment, minBearingDifference, userHeading, segmentHeading, issue));                
            }
            // We left the POI and we are not off track
            else if (justLeft && !offTrackDetected)
            {
                OnLeftPOI?.Invoke(this, new POIArgs(userLocation, currentDistance));
            }
            else if (currentState != POIState.OnTrack && !offTrackDetected)
            {
                // We correct the current POI based on the closes upcoming one
                if (currentState == POIState.OffTrack)
                {
                    var closestPOI = routeValidation.IdentifyClosestUpcomingPOI(userLocation, 2);
                    CurrentPOIIndex = GetPathpointIndexById(closestPOI.Id);
                    CurrentTargetPOI = POIList[CurrentPOIIndex];
                }                

                currentState = POIState.OnTrack;

                OnAlongTrack?.Invoke(this, new ValidationArgs(userLocation, minDistanceToSegment, minBearingDifference, userHeading, segmentHeading));
            }
            else if(currentState == POIState.OnTrack && !offTrackDetected && landarkSkipped)
            {
                var closestPOI = routeValidation.IdentifyClosestUpcomingPOI(userLocation, 2);

                if (closestPOI.Id != CurrentTargetPOI.Id)
                {
                    CurrentPOIIndex = GetPathpointIndexById(closestPOI.Id);
                    CurrentTargetPOI = POIList[CurrentPOIIndex];

                    Debug.Log($"ARE WE Skipping POI? CurrentPOIIndex: {CurrentPOIIndex}");
                }
                
            }
        }

        // Let's inform that there is a new valid user location (curated)
        OnUserLocationChanged?.Invoke(this, new LocationChangedArgs(userLocation, minDistanceToSegment, minBearingDifference, userHeading, segmentHeading, closestSegmentIndex, isWalking, walkPace));


        return pathpointLog;
    }

    public int GetPathpointIndexById(int pathpointId)
    {
        int index = -1;
        for (int i =0; i<POIList.Count; i++)
        {
            if(POIList[i].Id == pathpointId)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    private int? GetPathpointIdByIndex(int index)
    {
        if (index >0 && POIList[index] != null)
        {
            return POIList[index].Id;
        }
        return null;
    }

    public POIState GetCurrentState()
    {
        return currentState;
    }

    public Pathpoint GetCurrentTargetPathpoint()
    {
        return CurrentTargetPOI;
    }
}