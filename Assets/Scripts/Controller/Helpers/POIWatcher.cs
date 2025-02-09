using System;
using System.Collections.Generic;
using MathNet.Numerics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using static LocationUtils;
using static PaganiniRestAPI;
using static PathpointPIM;

public class ValidationArgs: EventArgs
{
    public Pathpoint Point { get; set; }
    public SegmentDistanceAndBearing SegmentInfo { get; set; }
    public bool OnTrack { get; set; }
    public Pathpoint TargetPOI { get; set; }
    public LocationUtils.NavigationIssue Issue { get; set; }

    public double DistanceWalked { set; get; }
    public double WalkingPaceSum { set; get; }
    public double WalkingPace { set; get; }
    public double WalkingSteps { set; get; }
    public double MaxDistanceFromTrack { set; get; }


    public ValidationArgs() {
        DistanceWalked = 0;
        WalkingPaceSum = 0;
        WalkingPace = 0;
        WalkingSteps = 0;
        MaxDistanceFromTrack = 0;
    }

    public ValidationArgs(Pathpoint point, SegmentDistanceAndBearing segmentInfo, bool onTrack, LocationUtils.NavigationIssue issue = LocationUtils.NavigationIssue.Deviation, Pathpoint targetPOI = null)
    {
        Point = point;
        SegmentInfo = segmentInfo;
        Issue = issue;
        OnTrack = onTrack;
        TargetPOI = targetPOI;
    }
}

public class DecisionArgs : EventArgs
{
    public Pathpoint Point;
    public Pathpoint TargetPOI { get; set; }
    public double DistanceFromPOI { get; set; }
    public bool OnTargetPOI { get; set; }
    // performance stats
    public double DistanceWalked { set; get; }
    public double WalkingPaceSum { set; get; }
    public double WalkingPace { set; get; }
    public double WalkingSteps { set; get; }

    // data for decisionMade
    public bool? IsCorrectDecision { set; get; }
    public Pathpoint.NavDirection? DecisionExpected { set; get; }
    public NavigationIssue? NavIssue { set; get; }

    public DecisionArgs(Pathpoint point, Pathpoint targetPOI, double distance, bool onPOI)
    {
        DistanceWalked = 0;
        WalkingPaceSum = 0;
        WalkingPace = 0;
        WalkingSteps = 0;

        Point = point;
        TargetPOI = targetPOI;
        DistanceFromPOI = distance;
        OnTargetPOI = onPOI;
    }
}

public class SegmentCompletedArgs : EventArgs
{
    public Pathpoint Point;
    public double DistanceFromPOI { get; set; }
    public bool HasArrived { get; set; }
    public Pathpoint TargetPOI { get; set; }

    // performance stats
    public double DistanceWalked { set; get; }
    public double WalkingPaceSum { set; get; }
    public double WalkingPace { set; get; }
    public double WalkingSteps { set; get; }

    // data for segment SegmentCompleted
    public int? SegPOIStartId { set; get; }
    public int? SegExpectedPOIEndId { set; get; }
    public int? SegReachedPOIEndId { set; get; }
    public double DistanceCorrectlyWalked { set; get; }

    public SegmentCompletedArgs(Pathpoint point, double distance, bool hasArrived, Pathpoint targetPOI = null)
    {
        DistanceWalked = 0;
        WalkingPaceSum = 0;
        WalkingPace = 0;
        WalkingSteps = 0;
        DistanceCorrectlyWalked = 0;

        Point = point;
        DistanceFromPOI = distance;        
        HasArrived = hasArrived;
        TargetPOI = targetPOI;
    }
}

public class AdaptationTaskArgs : EventArgs
{
    public Pathpoint Point;
    
    public bool IsTaskStart{ get; set; }
    public bool IsAtPOIMode { get; set; }

    // performance stats
    public SupportMode? AdaptationSupportMode { set; get; }
    public bool? AdaptationIntroShown { set; get; }
    public bool? AdaptationTaskAccepted { set; get; }
    public bool? AdaptationTaskCompleted { set; get; }
    public bool? AdaptationTaskCorrect { set; get; }
    public bool? AdaptationDowngradedByUser { set; get; }
    public bool? AdaptationDowngradedBySystem { set; get; }
    public int? AdaptationPIMId;

    // data for segment, if in a segment (goto)
    public int? SegPOIStartId { set; get; }
    public int? SegExpectedPOIEndId { set; get; }
    public int? SegReachedPOIEndId { set; get; }

    // data for poi, if a decision instruction
    public Pathpoint TargetPOI { get; set; }

    public AdaptationTaskArgs()
    {

    }

    public AdaptationTaskArgs(Pathpoint point, bool isTaskStart)
    {
        Point = point;
        IsTaskStart = isTaskStart;
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
    public Pathpoint TargetPOI { get; set; }

    public WalkingStatusArgs(Pathpoint point, bool isWalking, double pace, Pathpoint targetPOI = null)
    {
        Point = point;
        IsWalking = isWalking;
        Pace = pace;
        TargetPOI = targetPOI;
    }
}

public class PauseStatusArgs : EventArgs
{
    public Pathpoint Point;
    public bool IsPaused { get; set; }
    public Pathpoint TargetPOI { get; set; }
    public long EventTimestamp { get; set; }


    public PauseStatusArgs(Pathpoint point, bool isPaused, Pathpoint targetPOI = null)
    {
        Point = point;
        IsPaused = isPaused;
        TargetPOI = targetPOI;
    }
}

public class InstructionSleepStatusArgs : EventArgs
{
    public Pathpoint Point;
    public bool IsSleeping { get; set; }
    public bool WasAwakenByUser { get; set; }

    public InstructionSleepStatusArgs(Pathpoint point, bool isSleeping, bool wasAwakenByuser = false)
    {
        Point = point;
        IsSleeping = isSleeping;
        WasAwakenByUser = wasAwakenByuser;
    }
}

public class LocationChangedArgs : EventArgs
{

    public Pathpoint Point;
    public SegmentDistanceAndBearing SegmentInfo;
    public SegmentCompletedArgs SegmentStats;
    //public double MinDistanceToSegment { get; set; }
    //public double MinBearingDifference { get; set; }
    //public double UserHeading { get; set; }
    //public double SegmentHeading { get; set; }
    //public double ClosestSegmentIndex { get; set; }    
    public bool IsWalking { get; set; }
    public double Pace { get; set; }

    public LocationChangedArgs(Pathpoint point, SegmentDistanceAndBearing segmentInfo, SegmentCompletedArgs segmentStats, bool isWalking, double pace)
    {
        Point = point;
        //MinDistanceToSegment = minDistanceToSegment;
        //MinBearingDifference = minBearingDifference;
        //UserHeading = userHeading;
        //SegmentHeading = segmentHeading;
        //ClosestSegmentIndex = closestSegmentIndex;
        SegmentInfo = segmentInfo;
        SegmentStats = segmentStats;
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
    public event EventHandler<ValidationArgs> OnOffTrackEnd;
    public event EventHandler<ValidationArgs> OnAlongTrack;
    public event EventHandler<DecisionArgs> OnDecisionStart;
    public event EventHandler<DecisionArgs> OnDecisionEnd;
    public event EventHandler<SegmentCompletedArgs> OnSegmentStart;
    public event EventHandler<SegmentCompletedArgs> OnSegmentEnd;
    public event EventHandler<POIArgs> OnLeftPOI;
    public event EventHandler<POIArgs> OnEnteredPOI;
    public event EventHandler<POIArgs>  OnArrived;
    public event EventHandler<WalkingStatusArgs> OnWalkStatusChange;
    public event EventHandler<PathpointInvalidArgs> OnInvalidPathpoint;
    public event EventHandler<EventArgs<string>> OnLog;
    public event EventHandler<LocationChangedArgs> OnUserLocationChanged;
    public event EventHandler<EventArgs<Pathpoint>> OnPOITargetChange; // non muted

    private Pathpoint CurrentTargetPOI;    
    private List<Pathpoint> POIList;
    private LocationUtils routeValidation;
    public int CurrentPOIIndex = -1;
    public bool IsWalking = false;
    private Pathpoint PreviousUserLocation;

    private Pathpoint CurrentInstructionTargetPOI;
    public bool WasPreviousMuted;

    public enum POIState { None, OnPOI, LeftPOI, OffTrack, OnTrack, Arrived, Invalid, AtStart }
    private POIState currentState = POIState.None;
    private POIState previousState = POIState.None;
    private int LocationCount = 0;
    private double TotalWalkingDistance;
    private NavigationIssue? CurrentNavigationIssue;

    // Stats for events
    private ValidationArgs OfftrackStats;
    private DecisionArgs DecisionStats;
    private SegmentCompletedArgs SegmentStats;

    private WalkingDetector walkingDetector;
    LocationQualityControl locationQualityControl;

    public void InitialiseWatcher(LocationUtils routeValidation, LocationUtilsConfig config)
    {
        this.routeValidation = routeValidation;
        walkingDetector = new WalkingDetector(config);
        //locationQualityControl = new LocationQualityControl(walkingDetector, slidingWindowSize: 5, maxSpeed: 10, maxAccuracy: 30);
        locationQualityControl = new LocationQualityControl(walkingDetector, config); 

        currentState = POIState.None;
        previousState = POIState.None;
        LocationCount = 0;
        CurrentPOIIndex = -1;
        CurrentTargetPOI = null;
        IsWalking = false;
        TotalWalkingDistance = 0;
        OfftrackStats = null;
        DecisionStats = null;
        SegmentStats = null;
        PreviousUserLocation = null;
    }

    public void LoadTargetPOIs(List <Pathpoint> list)
    {
        POIList = list;
    }

    public void NextPOI()
    {
        WasPreviousMuted = IsCurrentPOIMuted();

        CurrentPOIIndex++;
        CurrentTargetPOI = POIList[CurrentPOIIndex];

        // update target
        GetUpcomingNonMutedTarget();
    }

    public void SetStartingPoint(int startPOIIndex, POIState pOIState)
    {
        CurrentPOIIndex = startPOIIndex;
        CurrentTargetPOI = POIList[CurrentPOIIndex];
        currentState = pOIState;
        previousState = POIState.None;

        // update target
        GetUpcomingNonMutedTarget();
    }

    // We set a target for the instruction, different from the navigation
    public Pathpoint GetUpcomingNonMutedTarget()
    {
        int index = CurrentPOIIndex;

        var oldTarget = CurrentInstructionTargetPOI;

        do {
            CurrentInstructionTargetPOI = POIList[index];
            index++;
        }
        while (CurrentInstructionTargetPOI.CurrentInstructionMode != null &&
               CurrentInstructionTargetPOI.CurrentInstructionMode.AtPOIMode == PathpointPIM.SupportMode.Mute);


        if (oldTarget != CurrentInstructionTargetPOI)
        {
            OnPOITargetChange?.Invoke(this, new EventArgs<Pathpoint>(CurrentInstructionTargetPOI));
        }

        return CurrentInstructionTargetPOI;
    }

    public bool IsCurrentPOIMuted()
    {
        return CurrentTargetPOI.CurrentInstructionMode != null &&
            CurrentTargetPOI.CurrentInstructionMode.AtPOIMode == PathpointPIM.SupportMode.Mute;
    }

    public bool IsPreviousPOIMuted()
    {
        if (CurrentPOIIndex == 0) return false;

        Pathpoint PreviousTargetPOI = POIList[CurrentPOIIndex - 1];
        return PreviousTargetPOI.CurrentInstructionMode != null &&
            PreviousTargetPOI.CurrentInstructionMode.AtPOIMode == PathpointPIM.SupportMode.Mute;
    }

    public int GetLocationIndex()
    {
        return LocationCount;
    }

    public bool IsUserAtStartingPoint(Pathpoint pathpoint)
    {
        (Pathpoint userLocation, double valAccuracy, double valSpeed) = locationQualityControl.ProcessLocation(pathpoint);

        return userLocation != null && routeValidation.IsPathpointOnTarget(pathpoint, POIList[0]);

    }


    public PathpointLog UpdateUserLocation(Pathpoint pathpoint)
    {
        bool isWalking = false;
        bool justLeft = false;
        double walkPace = 0;

        LocationCount++;

        #region Check user location quality, and normalise it
        // Let's assess the quality of the incoming pathpoint
        (Pathpoint userLocation, double valAccuracy, double valSpeed) = locationQualityControl.ProcessLocation(pathpoint);

        // Is the pathpoint of low quality?
        if (userLocation == null) {
            OnInvalidPathpoint?.Invoke(this, new PathpointInvalidArgs(pathpoint, valAccuracy, valSpeed));
            return null;
        }

        #endregion

        #region Initialise pathpointlog, and check walking
        // We initialise the pathpoint log, with the route segments (poi start, end)
        PathpointLog pathpointLog = new PathpointLog(userLocation);
        pathpointLog.Id = LocationCount;        
        pathpointLog.SegPOIStartId = GetPathpointIdByIndex(CurrentPOIIndex - 1);
        pathpointLog.SegPOIEndId = GetPathpointIdByIndex(CurrentPOIIndex);

        // Let's verify the walking status of the user
        //(isWalking, walkPace) = walkingDetector.IsWalking(pathpoint);
        isWalking = walkingDetector.IsWalkingBasedOnSpeed(pathpoint,out walkPace);
        // Did the walking status is different from the previous state (walking status changed?)
        if (IsWalking != isWalking)
        {
            // If we are on a target POI, we also send this information as part of the event
            Pathpoint targetPOI = null;
            if (currentState == POIState.OnPOI)
            {
                targetPOI = CurrentTargetPOI;
            }
            OnWalkStatusChange?.Invoke(this, new WalkingStatusArgs(userLocation, isWalking, walkPace, targetPOI));
            IsWalking = isWalking;
        }

        // We log the walking status
        pathpointLog.IsWalking = isWalking;
        pathpointLog.WalkingPace = walkPace;

        #endregion

        #region User on a POI?
        // Let's calculate the distance to the Target POI
        var currentDistance = LocationUtils.HaversineDistance(userLocation, CurrentTargetPOI);

        
        // Are we on the Target POI?
        if (routeValidation.IsPathpointOnTarget(userLocation, CurrentTargetPOI))
        {
            // Have we arrived? (We are at the last POI)
            if (CurrentPOIIndex+1 == POIList.Count)
            {
                // We send segment stats, and inform that the segment is ending
                // Segment: from POI_i leave -> POI_j enter
                if (SegmentStats != null)
                {
                    SegmentStats.SegReachedPOIEndId = pathpointLog.SegPOIEndId;
                    SegmentStats.HasArrived = true;
                    OnSegmentEnd?.Invoke(this, SegmentStats);
                    SegmentStats = null; // we reset the statistics
                }
                
                SetCurrentState(POIState.Arrived);
                OnArrived?.Invoke(this, new POIArgs(userLocation, currentDistance));
            }
            // Have we just entered the target POI?
            else if (currentState != POIState.OnPOI)
            {
                if (currentState == POIState.OffTrack)
                {
                    // we send the off track stats
                    var args = new ValidationArgs(userLocation, null, true);
                    args.DistanceWalked = OfftrackStats.DistanceWalked;
                    args.WalkingSteps = OfftrackStats.WalkingSteps;
                    args.MaxDistanceFromTrack = OfftrackStats.MaxDistanceFromTrack;
                    args.WalkingPace = OfftrackStats.WalkingPace;

                    OnOffTrackEnd?.Invoke(this, args);
                }

                // we had made an error leaving the POI and now we are coming back
                if (previousState == POIState.OnPOI)
                {
                    SegmentStats = null; // we do not record the off-track walk as a segment
                }

                // We send segment stats, and inform that the segment is ending
                // Segment: from POI_i leave -> POI_j enter
                if (SegmentStats!= null)
                {
                    SegmentStats.SegReachedPOIEndId = pathpointLog.SegPOIEndId;
                    SegmentStats.HasArrived = true;
                    OnSegmentEnd?.Invoke(this, SegmentStats);
                    SegmentStats = null; // we reset the statistics
                }

                // We inform of a user decision about to be made
                DecisionStats = new DecisionArgs(userLocation, CurrentTargetPOI, currentDistance, true);
                DecisionStats.DecisionExpected = CurrentTargetPOI.Instruction;
                OnDecisionStart.Invoke(this, DecisionStats);

                SetCurrentState(POIState.OnPOI);
                OnEnteredPOI?.Invoke(this, new POIArgs(userLocation, currentDistance));
            }

            pathpointLog.TargetPOIId = CurrentTargetPOI.Id;

            CurrentNavigationIssue = null;

            // We are still on the target POI (we entered before)
            OnLog?.Invoke(this, new EventArgs<string>($"Still on On Target: " + currentDistance));
        }
        else
        {
            // Have we just left the Target POI (we are not in the POI but we were in the previuos update)
            if (currentState == POIState.OnPOI)
            {
                justLeft = true;
                SetCurrentState(POIState.LeftPOI);
                //OnLeftPOI?.Invoke(this, new EventArgs<double>(d));


                // We start the segment stats, and inform that a new segment is starting
                // Segment: from POI_i leave -> POI_j enter
                SegmentStats = new SegmentCompletedArgs(userLocation, currentDistance, false);
                // we fill up the next segment. the client havent' moved the poi target, but for logging
                // purposes we do it here so that it is recorded in the right segment
                pathpointLog.SegPOIStartId = GetPathpointIdByIndex(CurrentPOIIndex);
                pathpointLog.SegPOIEndId = GetPathpointIdByIndex(CurrentPOIIndex+1);
                SegmentStats.SegPOIStartId = pathpointLog.SegPOIStartId;
                SegmentStats.SegExpectedPOIEndId = pathpointLog.SegPOIEndId;
                OnSegmentStart?.Invoke(this, SegmentStats);

            }
        }
        #endregion

        if (currentState == POIState.AtStart && SegmentStats == null)
        {
            SegmentStats = new SegmentCompletedArgs(userLocation, currentDistance, false);
            SegmentStats.SegPOIStartId = pathpointLog.SegPOIStartId;
            SegmentStats.SegExpectedPOIEndId = pathpointLog.SegPOIEndId;
            OnSegmentStart?.Invoke(this, SegmentStats);
        }

        // Let's compute the walking distance increment from the previous location
        double walkedDistanceDelta = 0;
        if (PreviousUserLocation != null)
        {
            walkedDistanceDelta = LocationUtils.HaversineDistance(userLocation, PreviousUserLocation);
            TotalWalkingDistance += walkedDistanceDelta;

        }
        PreviousUserLocation = userLocation;
        pathpointLog.TotalWalkedDistance = TotalWalkingDistance;

        // compute metrics
        routeValidation.UpdateUserLocationHistory(userLocation);
        //(double minDistanceToSegment, double minBearingDifference, double userHeading, double segmentHeading, int closestSegmentIndex) = routeValidation.CalculateMinDistanceAndBearing(userLocation);        
        var segmentInfo = routeValidation.CalculateMinDistanceAndBearing(userLocation);

        Debug.Log($"[{LocationCount}] {CurrentPOIIndex} (Acc ERW{Math.Round(pathpoint.Accuracy).ToString().PadLeft(2)}| User {Math.Round(userLocation.Accuracy).ToString().PadRight(2)})" +
                  $" - POI distance: {Math.Round(currentDistance, 3).ToString("F3").PadLeft(6)} Closest Seg Index: {segmentInfo.ClosestSegmentIndex} Distance to Seg: {Math.Round(segmentInfo.MinDistanceToSegment, 3).ToString("F3")} | Seg-Bear: {Math.Round(segmentInfo.SegmentHeading, 3).ToString("F3")} User-Bear: {Math.Round(segmentInfo.UserHeading, 3).ToString("F3")}  Diff-Bear: {Math.Round(segmentInfo.MinBearingDifference, 3).ToString("F3")} | isWalking: {isWalking} Pace: {Math.Round(walkPace, 3).ToString("F3")} |" +
                  $" Tot Walk Distance: {Math.Round(TotalWalkingDistance, 3).ToString("F3")}");


        // We are not on a POI and we stopped. We do not check off-track then
        // Revisit this, to instead skip the off track and still do statistics
        //if (currentState != POIState.OnPOI && !isWalking && !justLeft)
        //{
        //    return pathpointLog;
        //}


        // We are not on a POI and we stopped. We do not check off-track then
        // Revisit this, to instead skip the off track and still do statistics
        if (currentState == POIState.OnPOI || isWalking || justLeft)
        {
            #region Off-track Checking
            // We check if the landmark has possibly been skipped
            bool landarkSkipped = routeValidation.CheckLandmarkSkipped(currentDistance, CurrentPOIIndex);

            // Compute whether the user is off-track
            int? lastPOIIndex = justLeft ? CurrentPOIIndex : null;
            int? lastPathpointIndex = null;
            // we calculate the index of the current POI to compute type of navigation issue (decision mistake),
            // but only with decision points (landmarks)
            if (justLeft && CurrentTargetPOI.POIType == Pathpoint.POIsType.Landmark)
            {
                lastPathpointIndex = routeValidation.GetPathpointIndexById(POIList[CurrentPOIIndex].Id);
            }
            //(bool offTrackDetected, LocationUtils.NavigationIssue issue) = routeValidation.IsUserOffTrack(userLocation,segmentInfo, lastPathpointIndex, currentState == POIState.OffTrack);

            bool offTrackDetected;
            LocationUtils.NavigationIssue issue = NavigationIssue.Deviation;
            if (currentState == POIState.OffTrack)
            {
                offTrackDetected =  !routeValidation.IsUserOnTrack(userLocation, segmentInfo, (NavigationIssue)CurrentNavigationIssue);
            }
            else
            {
                (offTrackDetected, issue) = routeValidation.IsUserOffTrackNew(userLocation, segmentInfo, lastPathpointIndex, currentState == POIState.OffTrack);
            }
        

            // Let's assess if there are any issues
            if (currentState != POIState.OnPOI && currentState != POIState.Arrived)
            {            
                if (currentState != POIState.OffTrack && offTrackDetected)
                {
                    // Did we make the incorrect decision leaving of a POI?
                    if (justLeft)
                    {
                        DecisionStats.DistanceFromPOI = currentDistance;
                        DecisionStats.Point = userLocation;
                        DecisionStats.OnTargetPOI = false;
                        DecisionStats.IsCorrectDecision = false;
                        DecisionStats.NavIssue = issue;

                        OnDecisionEnd?.Invoke(this, DecisionStats);
                    }

                    // Here we will keep stats of the offtrack state, which will be send once we are back on track.
                    OfftrackStats = new ValidationArgs();
                    SetCurrentState(POIState.OffTrack);
                    CurrentNavigationIssue = issue;
                    var targetPOI = lastPOIIndex != null ? POIList[(int)lastPOIIndex] : null;
                    OnOffTrack?.Invoke(this, new ValidationArgs(userLocation, segmentInfo, false, issue, targetPOI));                
                }
                // We left the POI and we are not off track
                else if (justLeft && !offTrackDetected)
                {
                    DecisionStats.DistanceFromPOI = currentDistance;
                    DecisionStats.Point = userLocation;
                    DecisionStats.OnTargetPOI = false;
                    DecisionStats.IsCorrectDecision = true;                
                    DecisionStats.NavIssue = null; 
                    OnDecisionEnd?.Invoke(this, DecisionStats);

                    OnLeftPOI?.Invoke(this, new POIArgs(userLocation, currentDistance));
                }
                // We are on track again
                else if (currentState != POIState.OnTrack && !offTrackDetected)
                {
                    // We correct the current POI based on the closes upcoming one
                    // since we might have skipped one, or gone around a POI
                    if (currentState == POIState.OffTrack)
                    {
                        var closestPOI = routeValidation.IdentifyClosestUpcomingPOI(userLocation, 2);
                        CurrentPOIIndex = GetPathpointIndexById(closestPOI.Id);
                        CurrentTargetPOI = POIList[CurrentPOIIndex];


                        // we send the offtrack stats                
                        var args = new ValidationArgs(userLocation, segmentInfo, true);
                        args.DistanceWalked = OfftrackStats.DistanceWalked;
                        args.WalkingSteps = OfftrackStats.WalkingSteps;
                        args.WalkingPace = OfftrackStats.WalkingPace;
                        args.MaxDistanceFromTrack = OfftrackStats.MaxDistanceFromTrack;

                        OnOffTrackEnd?.Invoke(this, args);
                    }                

                    // we are on track
                    SetCurrentState(POIState.OnTrack);
                    OnAlongTrack?.Invoke(this, new ValidationArgs(userLocation, segmentInfo, true));
                }
                //else if (currentState == POIState.OnTrack &&
                //    !offTrackDetected && landarkSkipped)
                //{
                else if (currentState == POIState.OnTrack && previousState != POIState.AtStart &&
                    !offTrackDetected && landarkSkipped)
                {
                    var closestPOI = routeValidation.IdentifyClosestUpcomingPOI(userLocation, 2);
                    // TODO: Check closest segment index. This is more robust than purely POI
                    //        Think of a zig zag thing.

                    if (closestPOI.Id != CurrentTargetPOI.Id)
                    {
                        CurrentPOIIndex = GetPathpointIndexById(closestPOI.Id);
                        CurrentTargetPOI = POIList[CurrentPOIIndex];

                        Debug.Log($"ARE WE Skipping POI? CurrentPOIIndex: {CurrentPOIIndex}");
                    }
                
                }
            }

            #endregion            
        }

        #region Compute statistics
        // We keep some statistics
        if (currentState == POIState.OffTrack)
        {
            OfftrackStats.DistanceWalked += walkedDistanceDelta;
            OfftrackStats.WalkingSteps++;
            OfftrackStats.WalkingPaceSum += walkPace;
            OfftrackStats.WalkingPace = OfftrackStats.WalkingPaceSum / OfftrackStats.WalkingSteps;
            // we compute how far did we get from the track
            if (OfftrackStats.MaxDistanceFromTrack < segmentInfo.MinDistanceToSegment)
            {
                OfftrackStats.MaxDistanceFromTrack = segmentInfo.MinDistanceToSegment;
            }
        }        
        else
        {
            OfftrackStats = null;
        }

        if (currentState == POIState.OnPOI)
        {
            DecisionStats.DistanceWalked += walkedDistanceDelta;
            DecisionStats.WalkingSteps++;
            DecisionStats.WalkingPaceSum += walkPace;
            DecisionStats.WalkingPace = DecisionStats.WalkingPaceSum / DecisionStats.WalkingSteps;
        }
        else
        {
            DecisionStats = null;
        }

        if (SegmentStats != null)
        {
            SegmentStats.DistanceWalked += walkedDistanceDelta;
            SegmentStats.WalkingSteps++;
            SegmentStats.WalkingPaceSum += walkPace;
            SegmentStats.WalkingPace = SegmentStats.WalkingPaceSum / SegmentStats.WalkingSteps;
            if (currentState != POIState.OffTrack)
            {
                SegmentStats.DistanceCorrectlyWalked += walkedDistanceDelta;
            }
        }

        #endregion

        // Let's inform that there is a new valid user location (curated)
        OnUserLocationChanged?.Invoke(this, new LocationChangedArgs(userLocation, segmentInfo, SegmentStats, isWalking, walkPace));

        return pathpointLog;
    }

    public (ValidationArgs OfftrackStats, DecisionArgs DecisionStats, SegmentCompletedArgs SegmentStats) GetNavigationStats()
    {
        return (OfftrackStats, DecisionStats, SegmentStats);
    }

    public PauseStatusArgs GetBasePauseStats()
    {
        PauseStatusArgs args = new PauseStatusArgs (PreviousUserLocation, false);

        if (currentState == POIState.OnPOI)
        {
            args.TargetPOI = CurrentTargetPOI;
        }

        args.EventTimestamp = DateUtils.UTCMilliseconds();

        return args;
    }

    public InstructionSleepStatusArgs GetBaseInstructionSleepStats()
    {
        InstructionSleepStatusArgs args = new InstructionSleepStatusArgs(PreviousUserLocation, false);
        return args;
    }

    public AdaptationTaskArgs GetBaseAdaptationTaskStats(AdaptationTaskArgs args)
    {
        args.Point = PreviousUserLocation;

        // start - onPOI event
        if (args.IsAtPOIMode && args.IsTaskStart)
        {
            args.TargetPOI = CurrentTargetPOI;
            args.AdaptationPIMId = CurrentTargetPOI.PathpointPIMId;
        }
        // end - onPOI events
        else if (args.IsAtPOIMode)
        {
            //args.TargetPOI = CurrentTargetPOI;
        }
        // start - segment event 
        else if (!args.IsAtPOIMode && args.IsTaskStart && SegmentStats != null)
        {
            args.SegPOIStartId = SegmentStats.SegPOIStartId;
            //args.SegExpectedPOIEndId = SegmentStats.SegExpectedPOIEndId;
            args.SegExpectedPOIEndId = CurrentInstructionTargetPOI.Id;
            args.AdaptationPIMId = CurrentTargetPOI.PathpointPIMId;
        }
        // end - segment event 
        else if (!args.IsAtPOIMode && !args.IsTaskStart)
        {
            args.SegReachedPOIEndId = CurrentTargetPOI.Id;
        }

        return args;
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

    private void SetCurrentState(POIState pOIState)
    {
        previousState = currentState;
        currentState = pOIState; 
    }

    private int? GetPathpointIdByIndex(int index)
    {
        if (index >=0 && POIList[index] != null)
        {
            return POIList[index].Id;
        }
        return null;
    }

    public NavigationIssue? GetNavigationIssue()
    {
        return CurrentNavigationIssue;
    }

    public POIState GetCurrentState()
    {
        return currentState;
    }

    public POIState GetPreviousState()
    {
        return previousState;
    }

    public Pathpoint GetCurrentTargetPathpoint()
    {
        return CurrentTargetPOI;
    }
}