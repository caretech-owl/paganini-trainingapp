using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private RouteWalk CurrentRouteWalk;    
    private bool CurrentlyTraining = false;

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

        POIWatch.OnEnteredPOI -= POIWatch_OnEnteredPOI;
        POIWatch.OnLeftPOI -= POIWatch_OnLeftPOI;
        POIWatch.OnOffTrack -= POIWatch_OnOffTrack;
        POIWatch.OnAlongTrack -= POIWatch_OnAlongTrack;
        POIWatch.OnArrived -= POIWatch_OnArrived;
        POIWatch.OnWalkStatusChange -= POIWatch_OnWalkStatusChange;
        POIWatch.OnLog -= POIWatch_OnLog;
        POIWatch.OnInvalidPathpoint -= POIWatch_OnInvalidPathpoint;

        POIWatch.OnEnteredPOI += POIWatch_OnEnteredPOI;        
        POIWatch.OnLeftPOI += POIWatch_OnLeftPOI;        
        POIWatch.OnOffTrack += POIWatch_OnOffTrack;        
        POIWatch.OnAlongTrack += POIWatch_OnAlongTrack;        
        POIWatch.OnArrived += POIWatch_OnArrived;
        POIWatch.OnWalkStatusChange += POIWatch_OnWalkStatusChange;        
        POIWatch.OnLog += POIWatch_OnLog;        
        POIWatch.OnInvalidPathpoint += POIWatch_OnInvalidPathpoint;

        SharedData = RouteSharedData.Instance;
        SharedData.OnDataDownloaded -= RouteSharedData_OnDataDownloaded;
        SharedData.OnDataDownloaded += RouteSharedData_OnDataDownloaded;

    }

    void OnDestroy()
    {
        POIWatch.OnEnteredPOI -= POIWatch_OnEnteredPOI;
        POIWatch.OnLeftPOI -= POIWatch_OnLeftPOI;
        POIWatch.OnOffTrack -= POIWatch_OnOffTrack;
        POIWatch.OnAlongTrack -= POIWatch_OnAlongTrack;
        POIWatch.OnArrived -= POIWatch_OnArrived;
        POIWatch.OnWalkStatusChange -= POIWatch_OnWalkStatusChange;
        POIWatch.OnLog -= POIWatch_OnLog;
        POIWatch.OnInvalidPathpoint -= POIWatch_OnInvalidPathpoint;

        SharedData.OnDataDownloaded -= RouteSharedData_OnDataDownloaded;
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
    }

    private void POIWatch_OnArrived(object sender, POIArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnArrived distance {e.Distance}");

        CurrentlyTraining = false;
        LoadArrived();

        HapticUtils.VibrateForNudge();
    }

    private void POIWatch_OnAlongTrack(object sender, ValidationArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnAlongTrack dist:{e.MinDistanceToSegment} bear:{e.MinBearingDifference} seg-b:{e.SegmentHeading} usr-b: {e.UserHeading}");
        //Wayfind.LoadOnTrack();
        LoadInstruction(POIWatch.CurrentPOIIndex);

        Debug.Log("POIWatch_OnAlongTrack");

        HapticUtils.VibrateForNotification();
        
    }

    private void POIWatch_OnOffTrack(object sender, ValidationArgs e)
    {
        GPSDebugDisplay.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnOffTrack dist:{e.MinDistanceToSegment} bear:{e.MinBearingDifference} seg-b:{e.SegmentHeading} usr-b: {e.UserHeading} issue: {e.Issue}");
        Wayfind.LoadOffTrack(e.Issue.ToString());

        Debug.Log($"POIWatch_OnOffTrack {e.Issue}");

        HapticUtils.VibrateForAlert();

        SpeechUtils.Speak("You are off-track!");
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
        if (!RouteTracking.RunSimulation)
        {
            // save route walk
            var lastWalk = RouteWalk.GetWithMinId(x => x.Id);

            CurrentRouteWalk = new RouteWalk();
            CurrentRouteWalk.Id = (lastWalk != null ? lastWalk.Id : 0) - 1;
            CurrentRouteWalk.StartDateTime = DateTime.Now;
            CurrentRouteWalk.RouteId = SharedData.CurrentRoute.Id;
            CurrentRouteWalk.Insert();
        }

        CurrentlyTraining = true;
        POIWatch.NextPOI();

        //GPSDebugDisplay.gameObject.SetActive(true);
        
    }

    public void OnLocationChanged(Pathpoint pathpoint)
    {
        if (CurrentlyTraining)
        {
            PathpointLog log = POIWatch.UpdateUserLocation(pathpoint);
            if (log != null && !RouteTracking.RunSimulation)
            {
                var lastLog = PathpointLog.GetWithMinId(x => x.Id);
                log.RouteWalkId = CurrentRouteWalk.Id;
                log.Id = (lastLog != null ? lastLog.Id : 0) - 1;
                // Store raw
                log.Latitude = pathpoint.Latitude;
                log.Longitude = pathpoint.Longitude;
                log.Timestamp = pathpoint.Timestamp;
                log.Accuracy = pathpoint.Accuracy;
                log.Altitude = pathpoint.Altitude;
                // End Store raw
                log.Insert();
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
            (double minDis, double minBearing, double userHeading, double segmentHeading, int closestSegmentIndex) = RouteValidation.CalculateMinDistanceAndBearing(pathpoint);

            GPSDebugDisplay.DisplayMetrics(distance, minDis, minBearing, userHeading, segmentHeading);

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


}
