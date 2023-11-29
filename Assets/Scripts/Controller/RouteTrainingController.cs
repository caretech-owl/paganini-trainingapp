using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NatSuite.Recorders.Clocks;
using UnityEngine;


public class RouteTrainingController : MonoBehaviour
{

    public WayfindInstruction Wayfind;    
    public LocationUtilsConfig RouteValidationConfig;
    public RouteTrainingTracking RouteTracking;
    public TrainingProgressBar RouteProgressBar;

    private RouteSharedData SharedData;

    private POIWatcher POIWatch;
    private LocationUtils RouteValidation;
    private Text2SpeechUtils SpeechUtils;
    private WalkEventManager WalkEventHandler;
    private RouteWalkLogger RouteWalkLog;

    private RouteWalk CurrentRouteWalk;    
    private bool CurrentlyTraining = false;
    private RealtimeClock clock;

    public GPSDebug GPSDebugDisplay;
    public SocketsAPI RealTimeAPI;

    // Start is called before the first frame update
    void Start()
    {
        RouteValidation = LocationUtils.Instance;
        RouteValidation.Initialise(RouteValidationConfig);

        POIWatch = POIWatcher.Instance;
        POIWatch.InitialiseWatcher(RouteValidation);

        SpeechUtils = Text2SpeechUtils.Instance;
        SpeechUtils.Initialise();

        RouteWalkLog = RouteWalkLogger.Instance;

        POIWatch.OnEnteredPOI -= POIWatch_OnEnteredPOI;
        POIWatch.OnLeftPOI -= POIWatch_OnLeftPOI;
        POIWatch.OnOffTrack -= POIWatch_OnOffTrack;
        POIWatch.OnOffTrackEnd -= POIWatch_OnOffTrackEnd;
        POIWatch.OnDecisionStart -= POIWatch_OnDecisionStart;
        POIWatch.OnDecisionEnd -= POIWatch_OnDecisionEnd;
        POIWatch.OnDecisionEnd -= POIWatch_OnDecisionEnd;
        POIWatch.OnSegmentStart -= POIWatch_OnSegmentStart;
        POIWatch.OnAlongTrack -= POIWatch_OnAlongTrack;
        POIWatch.OnArrived -= POIWatch_OnArrived;
        POIWatch.OnWalkStatusChange -= POIWatch_OnWalkStatusChange;
        POIWatch.OnLog -= POIWatch_OnLog;
        POIWatch.OnInvalidPathpoint -= POIWatch_OnInvalidPathpoint;
        POIWatch.OnUserLocationChanged -= POIWatch_OnUserLocationChanged;

        POIWatch.OnEnteredPOI += POIWatch_OnEnteredPOI;        
        POIWatch.OnLeftPOI += POIWatch_OnLeftPOI;        
        POIWatch.OnOffTrack += POIWatch_OnOffTrack;
        POIWatch.OnOffTrackEnd += POIWatch_OnOffTrackEnd;
        POIWatch.OnDecisionStart += POIWatch_OnDecisionStart;
        POIWatch.OnDecisionEnd += POIWatch_OnDecisionEnd;
        POIWatch.OnSegmentStart += POIWatch_OnSegmentStart;
        POIWatch.OnSegmentEnd += POIWatch_OnSegmentEnd;
        POIWatch.OnAlongTrack += POIWatch_OnAlongTrack;        
        POIWatch.OnArrived += POIWatch_OnArrived;
        POIWatch.OnWalkStatusChange += POIWatch_OnWalkStatusChange;        
        POIWatch.OnLog += POIWatch_OnLog;        
        POIWatch.OnInvalidPathpoint += POIWatch_OnInvalidPathpoint;
        POIWatch.OnUserLocationChanged += POIWatch_OnUserLocationChanged;

        SharedData = RouteSharedData.Instance;
        SharedData.OnDataDownloaded -= RouteSharedData_OnDataDownloaded;
        SharedData.OnDataDownloaded += RouteSharedData_OnDataDownloaded;

        WalkEventHandler = new ();

    }



    void OnDestroy()
    {
        POIWatch.OnEnteredPOI -= POIWatch_OnEnteredPOI;
        POIWatch.OnLeftPOI -= POIWatch_OnLeftPOI;
        POIWatch.OnOffTrack -= POIWatch_OnOffTrack;
        POIWatch.OnOffTrackEnd -= POIWatch_OnOffTrackEnd;
        POIWatch.OnDecisionStart -= POIWatch_OnDecisionStart;
        POIWatch.OnDecisionEnd -= POIWatch_OnDecisionEnd;
        POIWatch.OnDecisionEnd -= POIWatch_OnDecisionEnd;
        POIWatch.OnSegmentStart -= POIWatch_OnSegmentStart;
        POIWatch.OnAlongTrack -= POIWatch_OnAlongTrack;
        POIWatch.OnArrived -= POIWatch_OnArrived;
        POIWatch.OnWalkStatusChange -= POIWatch_OnWalkStatusChange;
        POIWatch.OnLog -= POIWatch_OnLog;
        POIWatch.OnInvalidPathpoint -= POIWatch_OnInvalidPathpoint;
        POIWatch.OnUserLocationChanged -= POIWatch_OnUserLocationChanged;

        SharedData.OnDataDownloaded -= RouteSharedData_OnDataDownloaded;
    }

    private void POIWatch_OnUserLocationChanged(object sender, LocationChangedArgs e)
    {
        if (!RouteTracking.RunSimulation || RouteTracking.SaveSimulatedWalk) {
            CurrentRouteWalk.WalkCompletionPercentage = (float)e.SegmentInfo.ClosestSegmentIndex / (float)SharedData.PathpointList.Count;
            CurrentRouteWalk.InsertDirty();
        }
        
    }

    private void POIWatch_OnDecisionEnd(object sender, DecisionArgs e)
    {
        WalkEventHandler.ProcessDecisionStatusChange(e);
    }

    private void POIWatch_OnDecisionStart(object sender, DecisionArgs e)
    {
        WalkEventHandler.ProcessDecisionStatusChange(e);
    }

    private void POIWatch_OnSegmentEnd(object sender, SegmentCompletedArgs e)
    {
        WalkEventHandler.ProcessSegmentCompleteStatusChange(e);
    }

    private void POIWatch_OnSegmentStart(object sender, SegmentCompletedArgs e)
    {
        WalkEventHandler.ProcessSegmentCompleteStatusChange(e);
    }

    private void POIWatch_OnInvalidPathpoint(object sender, PathpointInvalidArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnInvalidPathpoint Accuracy: {e.Accuracy} Speed: {e.Speed}");
        Debug.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnInvalidPathpoint Accuracy: {e.Accuracy} Speed: {e.Speed}");

        SendPathpointTrace(e.Point, POIWatcher.POIState.Invalid);
    }

    private void POIWatch_OnLog(object sender, EventArgs<string> e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnLog: {e.Data}");
    }

    private void POIWatch_OnWalkStatusChange(object sender, WalkingStatusArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnWalkStatusChange IsWalking: {e.IsWalking} Pace: {e.Pace}");
        Debug.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnWalkStatusChange IsWalking: {e.IsWalking} Pace: {e.Pace}");

        WalkEventHandler.ProcessWalkStatusChange(e);
    }

    private void POIWatch_OnArrived(object sender, POIArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnArrived distance {e.Distance}");

        CurrentlyTraining = false;

        if (!RouteTracking.RunSimulation)
        {
            CurrentRouteWalk.WalkCompletion = RouteWalk.RouteWalkCompletion.Completed;
            CurrentRouteWalk.EndDateTime = DateTime.Now; 
            CurrentRouteWalk.InsertDirty();
        }

        LoadArrived();

        HapticUtils.VibrateForNudge();

        // We save the local route walk data
        SaveLocalRouteWalk();
    }

    private void POIWatch_OnAlongTrack(object sender, ValidationArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnAlongTrack {e.SegmentInfo}");
        //Wayfind.LoadOnTrack();
        LoadInstruction(POIWatch.CurrentPOIIndex);

        Debug.Log("POIWatch_OnAlongTrack");

        HapticUtils.VibrateForNotification();
        
    }

    private void POIWatch_OnOffTrack(object sender, ValidationArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnOffTrack {e.SegmentInfo} issue: {e.Issue}");
        Wayfind.LoadOffTrack(e.Issue.ToString());

        WalkEventHandler.ProcessOfftrackStatusChange(e);

        Debug.Log($"POIWatch_OnOffTrack {e.Issue}");

        HapticUtils.VibrateForAlert();

        SpeechUtils.Speak("You are off-track!");
    }

    private void POIWatch_OnOffTrackEnd(object sender, ValidationArgs e)
    {        
        WalkEventHandler.ProcessOfftrackStatusChange(e);
    }

    private void POIWatch_OnLeftPOI(object sender, POIArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnLeftPOI distance {e.Distance}");
        // Let's watch out for the next POI along the route
        POIWatch.NextPOI();
        // Let's load the wayfinding instructions for the next
        LoadInstruction(POIWatch.CurrentPOIIndex);

        Debug.Log("POIWatch_OnLeftPOI");

        HapticUtils.VibrateForNudge();
    }

    private void POIWatch_OnEnteredPOI(object sender, POIArgs e)
    {
        //Confirm that we are finally here
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnEnteredPOI distance {e.Distance}");
        
        // If we are in the first landmark, the OnLeftPOI was not triggered (there was no previous POI)
        // so let's load the instruction photos as well, not only the directions
        Wayfind.LoadInstructionDirection(POIWatch.CurrentPOIIndex == 0);

        Debug.Log("POIWatch_OnEnteredPOI");

        HapticUtils.VibrateForNudge();

        SpeechUtils.Speak("Careful, look at the instructions!");
    }

    private void SendPathpointTrace(Pathpoint pathpoint, POIWatcher.POIState eventType = POIWatcher.POIState.None)
    {
        PathpointTraceMessage message = new PathpointTraceMessage();
        message.seq = POIWatch.GetLocationIndex();
        message.pathpoint = pathpoint.ToAPI();
        if (RouteTracking.RunSimulation) RealTimeAPI.SendPathpointTrace(message, eventType.ToString());
    }

    private void RouteSharedData_OnDataDownloaded(object sender, EventArgs e)
    {
        LoadTrainingComponents();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTraining()
    {
        SharedData.DownloadRouteDefinition();

        // We save any pending route walk
        SaveLocalRouteWalk();

        clock = new RealtimeClock();
    }

    public void LoadTrainingComponents()
    {
        SharedData.LoadRouteFromDatabase();

        LoadRoadSafety();

        LoadStartingPoint();

        LoadLocationTracking();

        POIWatch.LoadTargetPOIs(SharedData.POIList);

        // delete later
        if (RouteTracking.RunSimulation) {
            Wayfind.EnableStartTraining();
        }

        RouteProgressBar.Initialise();

    }

    public void ConfirmUserReady()
    {
        if (!RouteTracking.RunSimulation || RouteTracking.SaveSimulatedWalk)
        {
            // save route walk
            var lastWalk = RouteWalk.GetWithMinId(x => x.Id);

            CurrentRouteWalk = new RouteWalk();
            CurrentRouteWalk.Id = (lastWalk != null ? lastWalk.Id : 0) - 1;
            CurrentRouteWalk.StartDateTime = DateTime.Now;
            CurrentRouteWalk.RouteId = SharedData.CurrentRoute.Id;
            CurrentRouteWalk.InsertDirty();
        }

        CurrentlyTraining = true;
        POIWatch.NextPOI();

        //GPSDebugDisplay.gameObject.SetActive(true);
        
    }

    public void OnLocationChanged(Pathpoint pathpoint)
    {
        if (CurrentlyTraining)
        {
            // Let's initialise the pathpoint log, and inform the
            // walk event manager about it
            PathpointLog CurrentLog = new PathpointLog(pathpoint);
            var lastLog = PathpointLog.GetWithMinId(x => x.Id);
            CurrentLog.Id = (lastLog != null ? lastLog.Id : 0) - 1;

            WalkEventHandler.SetCurrentPathpointLog(CurrentLog);

            // We process the current location with POI Watch, which identify
            // navigation events. Some statistics are returned in the log
            PathpointLog log = POIWatch.UpdateUserLocation(pathpoint);
            if (log != null && (!RouteTracking.RunSimulation || RouteTracking.SaveSimulatedWalk))
            {
                log.Id = CurrentLog.Id;
                log.RouteWalkId = CurrentRouteWalk.Id;
                // Store raw
                log.Latitude = CurrentLog.Latitude;
                log.Longitude = CurrentLog.Longitude;
                log.Timestamp = CurrentLog.Timestamp;
                log.Accuracy = CurrentLog.Accuracy;
                log.Altitude = CurrentLog.Altitude;
                log.TotalWalkingTime = clock.timestamp;
                // End Store raw
                log.InsertDirty();
            }

            if (log != null) {
                var point = new Pathpoint
                {
                    Latitude = log.Latitude,
                    Longitude = log.Longitude,
                    Timestamp = log.Timestamp,
                    Accuracy = log.Accuracy,
                    Altitude = log.Altitude
                };
                //var point = new Pathpoint
                //{
                //    Latitude = pathpoint.Latitude,
                //    Longitude = pathpoint.Longitude,
                //    Timestamp = pathpoint.Timestamp,
                //    Accuracy = pathpoint.Accuracy,
                //    Altitude = pathpoint.Altitude
                //};

                SendPathpointTrace(point, POIWatch.GetCurrentState());
            }

            DebugMetrics(pathpoint);

        }
        //TODO: Move to POIWatcher
        else if (pathpoint.Accuracy < 30 && RouteValidation.IsPathpointOnTarget(pathpoint, SharedData.POIList[0]))
        {
            Wayfind.EnableStartTraining();
        }
 
    }

    public void DebugMetrics(Pathpoint pathpoint)
    {
        if (CurrentlyTraining && GPSDebugDisplay != null)
        {
            GPSDebugDisplay.DisplayGPS(pathpoint);
            GPSDebugDisplay.Ping();

            var distance = LocationUtils.HaversineDistance(pathpoint, SharedData.POIList[POIWatch.CurrentPOIIndex]);
            var segmentInfo = RouteValidation.CalculateMinDistanceAndBearing(pathpoint);

            GPSDebugDisplay.DisplayMetrics(distance, segmentInfo);

        }
    }



    private void LoadInstruction(int i)
    {
        var item = SharedData.PreparePOIData(i);
        Wayfind.LoadInstruction(item);
    }

    private void LoadStartingPoint()
    {
        var item = SharedData.PreparePOIData(0);
        Wayfind.LoadStart(item);
    }

    private void LoadArrived()
    {
        Wayfind.LoadArrived();
    }

    private void LoadRoadSafety()
    {
        RouteValidation.LoadRoutePathpoints(SharedData.PathpointList);
    }

    private void LoadLocationTracking()
    {
        RouteTracking.StartTracking();     
    }


    private void SaveLocalRouteWalk()
    {
        RouteWalkLog.UploadRouteWalksForRoute(SharedData.CurrentRoute);
    }

    // Terminate correctly the route.
    private void OnApplicationQuit()
    {
        if (CurrentRouteWalk.WalkCompletion != RouteWalk.RouteWalkCompletion.Completed)
        {
            CurrentRouteWalk.WalkCompletion = RouteWalk.RouteWalkCompletion.Cancelled;
            CurrentRouteWalk.EndDateTime = DateTime.Now;
            CurrentRouteWalk.InsertDirty();
        }
    }

}


