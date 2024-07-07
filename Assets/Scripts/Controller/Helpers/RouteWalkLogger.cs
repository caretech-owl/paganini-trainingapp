using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PaganiniRestAPI;
using static PathpointPIM;

public class RouteWalkLogger : PersistentLazySingleton<RouteWalkLogger>
{
    public Route CurrentRoute;
    public List<RouteWalk> CurrentRouteWalkList;
    public RouteWalk CurrentRouteWalk;

    public event EventHandler OnDataUploaded;
    public event EventHandler OnDataUploadError;


    public RouteWalkLogger()
    {
    }

    public void UploadRouteWalksForRoute(Route route)
    {
        CurrentRoute = route;
        // Let's check the local route walks we have
        CurrentRouteWalkList = RouteWalk.GetAll( rw => rw.RouteId == route.Id);

        Debug.Log($"UploadRouteWalksForRoute: We found {CurrentRouteWalkList.Count} routewalks");

        if (CurrentRouteWalkList.Count > 0)
        {
            var routewalk = CurrentRouteWalkList[0];
            CurrentRouteWalkList.RemoveAt(0);

            UploadRouteWalk(routewalk);
            
        }
        else
        {
            TerminateOrContinueProcess();
        }

        
    }

    private void UploadRouteWalk(RouteWalk routeWalk)
    {
        Debug.Log($"UploadRouteWalk: Uploading RouteWalk={routeWalk.Id} FromAPI = {routeWalk.FromAPI}");

        CurrentRouteWalk = routeWalk;

        if (!routeWalk.FromAPI)
        {
            PaganiniRestAPI.RouteWalk.Create(routeWalk.RouteId, routeWalk.ToAPI(), OnRouteWalkCreateSuccess, OnCreateError);
            return;
        }

        UploadRoutePathLogs(CurrentRouteWalk);
        
    }

    private void UploadRoutePathLogs(RouteWalk routeWalk)
    {
        var logs = PathpointLog.GetAll(log => log.RouteWalkId == routeWalk.Id && !log.FromAPI);

        Debug.Log($"UploadRoutePathLog: Uploading for RouteWalk={routeWalk.Id} PathLogs # = {logs.Count}");

        if (logs.Count > 0)
        {
            List<IRouteWalkPathAPI> list = new List<IRouteWalkPathAPI>();
            foreach (var log in logs)
            {
                var res = log.ToAPI();
                // we explicitly keep the local reference, so that in the response have the association
                // between old and news ids
                res.local_rpath_id = log.Id; 
                list.Add(res);
            }

            var batch = new RouteWalkPathAPIBatch
            {
                routewalk_path = list.ToArray()
            };

            PaganiniRestAPI.RouteWalkPathLog.BatchCreate(routeWalk.Id, batch, OnRouteWalkPathLogCreateSuccess, OnCreateError);
            return;
        }

        UploadRouteWalkEvents(CurrentRouteWalk);

    }

    private void UploadRouteWalkEvents(RouteWalk routeWalk)
    {
        var walkEvents = RouteWalkEventLog.GetAll(we => we.RouteWalkId == routeWalk.Id && !we.FromAPI);

        Debug.Log($"UploadRouteWalkEvent: Uploading for RouteWalk={routeWalk.Id} Events # = {walkEvents.Count}");

        if (walkEvents.Count > 0)
        {
            List<IRouteWalkEventAPI> list = new List<IRouteWalkEventAPI>();
            foreach (var we in walkEvents)
            {
                list.Add(we.ToAPI());
            }

            var batch = new RouteWalkEventAPIBatch
            {
                routewalk_events = list.ToArray()
            };

            PaganiniRestAPI.RouteWalkEvent.BatchCreate(routeWalk.Id, batch, OnRouteWalkEventCreateSuccess, OnCreateError);
            return;
        }

        
        TerminateOrContinueProcess();

    }




    /* API handling events */

    private void OnRouteWalkCreateSuccess(RouteWalkAPIResult routeWalkRes)
    {
        // Delete Current Route walk
        RouteWalk.Delete(CurrentRouteWalk.Id);

        // let's keep track of the local id, before changing it
        int oldId = CurrentRouteWalk.Id;

        // Insert new one
        CurrentRouteWalk.Id = routeWalkRes.rw_id;
        CurrentRouteWalk.FromAPI = true;
        CurrentRouteWalk.IsDirty = false;
        CurrentRouteWalk.Insert();

        // Let's update the references in the local database, so that the data
        // is consistent in case of API problems
        RouteWalkEventLog.ChangeParent(oldId, CurrentRouteWalk.Id);
        PathpointLog.ChangeParent(oldId, CurrentRouteWalk.Id);

        // We continue with uploading the events
        UploadRoutePathLogs(CurrentRouteWalk);
    }

    private void OnRouteWalkPathLogCreateSuccess(RouteWalkPathAPIList res)
    {
        // The upload process was successful. We can delete all the local
        // data related to this route walk
        // If we want to keep a local history (for reminders or similar),
        // we should comment out the following line
        // RouteWalk.Delete(CurrentRouteWalk.Id);

        // Delete local data related to this route walk
        PathpointLog.DeleteFromRouteWalk(CurrentRouteWalk.Id);

        // Get the route walk path logs from the API response
        RouteWalkPathAPIResult[] logs = res.routewalk_path;

        // Get all walk events for the current route walk that are not from API
        var walkEvents = RouteWalkEventLog.GetAll(we => we.RouteWalkId == CurrentRouteWalk.Id && !we.FromAPI);

        foreach (var we in walkEvents)
        {
            // Find matching path log ID for start and end points in the API response
            var startPathLog = logs.FirstOrDefault(log => log.local_rpath_id == we.StartPathpointLogId);
            var endPathLog = logs.FirstOrDefault(log => log.local_rpath_id == we.EndPathpointLogId);

            if (startPathLog != null)
            {
                we.StartPathpointLogId = startPathLog.rpath_id;
                we.InsertDirty();
            }

            if (endPathLog != null)
            {
                we.EndPathpointLogId = endPathLog.rpath_id;
                we.InsertDirty();
            }
        }

        // We continue uploading the path log event
        UploadRouteWalkEvents(CurrentRouteWalk);
    }


    private void OnRouteWalkEventCreateSuccess(RouteWalkEventAPIList arg0)
    {
        // We delete the local copies, which have been uploaded
        // We do not need to keep them
        RouteWalkEventLog.DeleteFromRouteWalk(CurrentRouteWalk.Id);

        TerminateOrContinueProcess();        
    }



    private void OnCreateError(string message)
    {
        Debug.Log(message);
        OnDataUploadError?.Invoke(this, new EventArgs<string>(message));
    }

    private void TerminateOrContinueProcess()
    {
        
        if (CurrentRouteWalkList.Count > 0)
        {
            var routeWalk = CurrentRouteWalkList[0];
            CurrentRouteWalkList.RemoveAt(0);


            UploadRouteWalk(routeWalk);
            return;
        }

        OnDataUploaded?.Invoke(this, null);
    }


}




public class WalkEventManager
{
    private PathpointLog CurrentPathpointLog;
    private RouteWalk CurrentRouteWalk;
   

    private WalkStoppedEvent CurrentWalkStoppedEvent;
    private OffTrackEvent CurrentOfftrackEvent;
    private DecisionMadeEvent CurrentDecisionEvent;
    private SegmentCompletedEvent CurrentSegmentEvent;
    private PausedEvent CurrentPauseEvent;
    private InstructionSleepEvent CurrentInstructionSleepEvent;
    private AdaptationEvent CurrentAdaptationEvent;

    private List<RouteWalkEventLog.RecoveryInstructionType> RecoveryInstructionUsedList;
    private List<RouteWalkEventLog.NavInstructionType> NavInstructionUsedList;

    public bool DisableLogging { get; set; }

    public WalkEventManager() { }

    public void SetCurrentPathpointLog(PathpointLog log)
    {
        CurrentPathpointLog = log;
    }

    public void SetCurrentRouteWalk(RouteWalk routeWalk)
    {
        CurrentRouteWalk = routeWalk;
    }

    // Capture contextual ui events
    public void AddUIEvent_NavInstructionUsed(RouteWalkEventLog.NavInstructionType instructionType)
    {
        if (NavInstructionUsedList == null)
        {
            NavInstructionUsedList = new List<RouteWalkEventLog.NavInstructionType> { };
        }

        NavInstructionUsedList.Add(instructionType);
    }

    public void AddUIEvent_RecoveryInstructionUsed(RouteWalkEventLog.RecoveryInstructionType recoveryType)
    {
        if (RecoveryInstructionUsedList == null)
        {
            RecoveryInstructionUsedList = new List<RouteWalkEventLog.RecoveryInstructionType> { };
        }

        RecoveryInstructionUsedList.Add(recoveryType);
    }

    public void AddUIEvent_OfftrackShown()
    {
        if (CurrentAdaptationEvent != null)
        {
            // We close the Adaptation event, which interrupts the user 
            PopulateEndEventBase(CurrentAdaptationEvent);

            CurrentAdaptationEvent.AdaptationTaskCompleted = false;
            //CurrentAdaptationEvent.AdaptationDowngradedBySystem = true;
            //CurrentAdaptationEvent.AdaptationDowngradedByUser = false;

            if (CurrentAdaptationEvent.AdaptationSupportMode != SupportMode.Trivia)
            {
                CurrentAdaptationEvent.AdaptationTaskCorrect = false;
            }

            var e = new RouteWalkEventLog(CurrentAdaptationEvent);
            e.Id = GetLastRouteWalkEventLog();
            e.InsertDirty();

            LogEventEnd("CurrentAdaptationEvent", CurrentAdaptationEvent);

            // we free the event
            CurrentAdaptationEvent = null;
        }
    }

    private void LogEventStart(string eventName, RouteWalkEventLogBase walkEvent)
    {
        //Debug.Log($"[EVENT START] - {eventName}: " +
        //          $"StartPathpointLogId: {walkEvent.StartPathpointLogId}, " +
        //          $"StartTimestamp: {walkEvent.StartTimestamp}, " +
        //          $"RouteWalkId: {walkEvent.RouteWalkId}, " +
        //          $"TargetPOIId: {walkEvent.TargetPOIId}");
    }

    private void LogEventEnd(string eventName, RouteWalkEventLogBase walkEvent)
    {
        //Debug.Log($"[EVENT END] - {eventName}: " +
        //          $"EndPathpointLogId: {walkEvent.EndPathpointLogId}, " +
        //          $"EndTimestamp: {walkEvent.EndTimestamp}, " +
        //          $"DurationEvent: {walkEvent.DurationEvent}, " +
        //          $"RouteWalkId: {walkEvent.RouteWalkId}, " +
        //          $"TargetPOIId: {walkEvent.TargetPOIId}");
    }

    public void ProcessWalkStatusChange(WalkingStatusArgs args)
    {
        if (DisableLogging) return;

        // The user stopped
        if (CurrentWalkStoppedEvent == null && !args.IsWalking)
        {
            CurrentWalkStoppedEvent = new WalkStoppedEvent();
            PopulateStartEventBase(CurrentWalkStoppedEvent, args.TargetPOI);

            LogEventStart("CurrentWalkStoppedEvent", CurrentWalkStoppedEvent);
        }
        // We are resuming walking
        else if (CurrentWalkStoppedEvent != null && args.IsWalking)
        {
            PopulateEndEventBase(CurrentWalkStoppedEvent);

            var e = new RouteWalkEventLog(CurrentWalkStoppedEvent);
            e.Id = GetLastRouteWalkEventLog();
            e.InsertDirty();

            LogEventEnd("CurrentWalkStoppedEvent", CurrentWalkStoppedEvent);

            // we free the event
            CurrentWalkStoppedEvent = null;
        }
        else
        {
            Debug.Log("CurrentWalkStoppedEvent: We need to handle better this case:" + args);
        }

    }

    public void ProcessPauseStatusChange(PauseStatusArgs args)
    {
        if (DisableLogging) return;

        // The user paused
        if (CurrentPauseEvent == null && args.IsPaused)
        {
            CurrentPauseEvent = new PausedEvent();
            PopulateStartEventBase(CurrentPauseEvent, args.TargetPOI);

            // we overwrite the timestamp, since we don't get it from pathpoint log (we are not tracking)
            CurrentPauseEvent.StartTimestamp = args.EventTimestamp;

            LogEventStart("CurrentPauseEvent", CurrentPauseEvent);
        }
        // We are resuming 
        else if (CurrentPauseEvent != null && !args.IsPaused)
        {
            PopulateEndEventBase(CurrentPauseEvent);

            // we overwrite the timestamp, since we don't get it from pathpoint log (we are not tracking)
            CurrentPauseEvent.EndTimestamp = args.EventTimestamp;
            CurrentPauseEvent.DurationEvent = CurrentPauseEvent.EndTimestamp - CurrentPauseEvent.StartTimestamp;

            var e = new RouteWalkEventLog(CurrentPauseEvent);
            e.Id = GetLastRouteWalkEventLog();
            e.InsertDirty();

            LogEventEnd("CurrentPauseEvent", CurrentPauseEvent);

            // we free the event
            CurrentPauseEvent = null;
        }
        else
        {
            Debug.Log("CurrentPauseEvent: We need to handle better this case:" + args);
        }

    }

    public void ProcessInstructionSleepStatusChange(InstructionSleepStatusArgs args)
    {
        if (DisableLogging) return; 

        // The instruction screen is sleeping
        if (CurrentInstructionSleepEvent == null && args.IsSleeping)
        {
            CurrentInstructionSleepEvent = new InstructionSleepEvent();
            PopulateStartEventBase(CurrentInstructionSleepEvent, null);

            LogEventStart("InstructionSleepEvent", CurrentInstructionSleepEvent);
        }
        // We are resuming 
        else if (CurrentInstructionSleepEvent != null && !args.IsSleeping)
        {
            PopulateEndEventBase(CurrentInstructionSleepEvent);

            var e = new RouteWalkEventLog(CurrentInstructionSleepEvent);
            e.Id = GetLastRouteWalkEventLog();
            e.WasAwakenByUser = args.WasAwakenByUser;
            e.InsertDirty();

            LogEventEnd("InstructionSleepEvent", CurrentInstructionSleepEvent);

            // we free the event
            CurrentInstructionSleepEvent = null;
        }
        else
        {
            Debug.Log("CurrentInstructionSleepEvent: We need to handle better this case:" + args);
        }

    }

    public void ProcessOfftrackStatusChange(ValidationArgs args)
    {
        if (DisableLogging) return;

        // The user is off-track
        if (CurrentOfftrackEvent == null && !args.OnTrack)
        {
            CurrentOfftrackEvent = new OffTrackEvent();
            PopulateStartEventBase(CurrentOfftrackEvent, args.TargetPOI);

            LogEventStart("CurrentOfftrackEvent", CurrentOfftrackEvent);
        }
        // We are resuming walking
        else if (CurrentOfftrackEvent != null && args.OnTrack)
        {
            PopulateEndEventBase(CurrentOfftrackEvent);

            CurrentOfftrackEvent.DistanceWalked = args.DistanceWalked;
            CurrentOfftrackEvent.WalkingPace = args.WalkingPace;
            CurrentOfftrackEvent.MaxDistanceFromTrack = args.MaxDistanceFromTrack;

            var e = new RouteWalkEventLog(CurrentOfftrackEvent);
            e.Id = GetLastRouteWalkEventLog();
            e.RecoveryInstructionUsed = RecoveryInstructionUsedList;
            e.InsertDirty();

            LogEventEnd("CurrentOfftrackEvent", CurrentOfftrackEvent);

            // we free the event
            CurrentOfftrackEvent = null;
            RecoveryInstructionUsedList = null;
        }
        else
        {
            //Debug.Log("CurrentOfftrackEvent: We need to handle better this case:" + args);
        }
    }

    public void ProcessDecisionStatusChange(DecisionArgs args)
    {
        if (DisableLogging) return;

        // The user is off-track
        if (CurrentDecisionEvent == null && args.OnTargetPOI)
        {
            CurrentDecisionEvent = new DecisionMadeEvent();
            PopulateStartEventBase(CurrentDecisionEvent, args.TargetPOI);

            CurrentDecisionEvent.DecisionExpected = args.DecisionExpected;

            LogEventStart("CurrentDecisionEvent", CurrentDecisionEvent);

        }
        // We left the POI and made a decision
        else if (CurrentDecisionEvent != null && !args.OnTargetPOI)
        {
            PopulateEndEventBase(CurrentDecisionEvent);

            CurrentDecisionEvent.DistanceWalked = args.DistanceWalked;
            CurrentDecisionEvent.WalkingPace = args.WalkingPace;
            CurrentDecisionEvent.IsCorrectDecision = args.IsCorrectDecision;
            CurrentDecisionEvent.NavIssue = args.NavIssue;

            var e = new RouteWalkEventLog(CurrentDecisionEvent);
            e.Id = GetLastRouteWalkEventLog();
            e.NavInstructionUsed = NavInstructionUsedList;
            e.InsertDirty();

            LogEventEnd("CurrentDecisionEvent", CurrentDecisionEvent);

            // we free the event
            CurrentDecisionEvent = null;
            NavInstructionUsedList = null;
        }
        else
        {
           // Debug.Log("CurrentDecisionEvent: We need to handle better this case:" + args);
        }
    }

    public void ProcessSegmentCompleteStatusChange(SegmentCompletedArgs args)
    {
        if (DisableLogging) return;

        if (CurrentSegmentEvent == null && !args.HasArrived)
        {
            CurrentSegmentEvent = new SegmentCompletedEvent();
            PopulateStartEventBase(CurrentSegmentEvent, args.TargetPOI);
            CurrentSegmentEvent.SegPOIStartId = args.SegPOIStartId;
            CurrentSegmentEvent.SegExpectedPOIEndId = args.SegExpectedPOIEndId;

            LogEventStart("CurrentSegmentEvent", CurrentSegmentEvent);

        }
        // We left the POI and made a decision
        else if (CurrentSegmentEvent != null && args.HasArrived)
        {
            PopulateEndEventBase(CurrentSegmentEvent);

            CurrentSegmentEvent.DistanceWalked = args.DistanceWalked;
            CurrentSegmentEvent.WalkingPace = args.WalkingPace;
            CurrentSegmentEvent.SegReachedPOIEndId = args.SegReachedPOIEndId;
            CurrentSegmentEvent.DistanceCorrectlyWalked = args.DistanceCorrectlyWalked;

            var e = new RouteWalkEventLog(CurrentSegmentEvent);
            e.Id = GetLastRouteWalkEventLog();
            e.InsertDirty();

            LogEventEnd("CurrentSegmentEvent", CurrentSegmentEvent);

            // we free the event
            CurrentSegmentEvent = null;
        }
        else
        {
           // Debug.Log("CurrentSegmentEvent: We need to handle better this case:" + args);
        }
    }

    public void ProcessAdaptationTaskStatusChange(AdaptationTaskArgs args)
    {
        if (DisableLogging) return;

        if (CurrentAdaptationEvent == null && args.IsTaskStart)
        {
            CurrentAdaptationEvent = new AdaptationEvent();
            PopulateStartEventBase(CurrentAdaptationEvent, args.TargetPOI);
            CurrentAdaptationEvent.SegPOIStartId = args.SegPOIStartId;
            CurrentAdaptationEvent.SegExpectedPOIEndId = args.SegExpectedPOIEndId;

            CurrentAdaptationEvent.AdaptationSupportMode = args.AdaptationSupportMode;
            CurrentAdaptationEvent.AdaptationIntroShown = args.AdaptationIntroShown;
            CurrentAdaptationEvent.AdaptationTaskAccepted = args.AdaptationTaskAccepted;

            LogEventStart("CurrentSegmentEvent", CurrentAdaptationEvent);

        }
        // there is an update
        else if (CurrentAdaptationEvent != null && args.IsTaskStart)
        {
            CurrentAdaptationEvent.AdaptationSupportMode = args.AdaptationSupportMode;
            CurrentAdaptationEvent.AdaptationIntroShown = args.AdaptationIntroShown;
            CurrentAdaptationEvent.AdaptationTaskAccepted = args.AdaptationTaskAccepted;

            LogEventStart("CurrentSegmentEvent - Update", CurrentAdaptationEvent);
        }

        // We left the POI and made a decision
        else if (CurrentAdaptationEvent != null && !args.IsTaskStart)
        {
            PopulateEndEventBase(CurrentAdaptationEvent);
            CurrentAdaptationEvent.SegReachedPOIEndId = args.SegReachedPOIEndId;
            CurrentAdaptationEvent.AdaptationTaskCompleted = args.AdaptationTaskCompleted;
            CurrentAdaptationEvent.AdaptationTaskCorrect = args.AdaptationTaskCorrect;
            CurrentAdaptationEvent.AdaptationDowngradedByUser = args.AdaptationDowngradedByUser;
            CurrentAdaptationEvent.AdaptationDowngradedBySystem = args.AdaptationDowngradedBySystem;

            var e = new RouteWalkEventLog(CurrentAdaptationEvent);
            e.Id = GetLastRouteWalkEventLog();
            e.InsertDirty();

            LogEventEnd("CurrentAdaptationEvent", CurrentAdaptationEvent);

            // we free the event
            CurrentAdaptationEvent = null;
        }
        else
        {
            // Debug.Log("CurrentSegmentEvent: We need to handle better this case:" + args);
        }
    }

    public void InterruptSegmentComplete(SegmentCompletedArgs args)
    {
        if (CurrentSegmentEvent != null)
        {
            PopulateEndEventBase(CurrentSegmentEvent);

            CurrentSegmentEvent.DistanceWalked = args.DistanceWalked;
            CurrentSegmentEvent.WalkingPace = args.WalkingPace;
            CurrentSegmentEvent.SegReachedPOIEndId = args.SegReachedPOIEndId;
            CurrentSegmentEvent.DistanceCorrectlyWalked = args.DistanceCorrectlyWalked;

            CancelLog(new RouteWalkEventLog(CurrentSegmentEvent));
        }
    }

    public void InterruptDecision(DecisionArgs args)
    {
        if (CurrentDecisionEvent != null)
        {
            PopulateEndEventBase(CurrentDecisionEvent);

            CurrentDecisionEvent.DistanceWalked = args.DistanceWalked;
            CurrentDecisionEvent.WalkingPace = args.WalkingPace;
            CurrentDecisionEvent.IsCorrectDecision = args.IsCorrectDecision;
            CurrentDecisionEvent.NavIssue = args.NavIssue;

            CancelLog(new RouteWalkEventLog(CurrentDecisionEvent));
        }
    }

    public void InterruptOfftrack(ValidationArgs args)
    {
        if (CurrentOfftrackEvent != null)
        {
            PopulateEndEventBase(CurrentOfftrackEvent);

            CurrentOfftrackEvent.DistanceWalked = args.DistanceWalked;
            CurrentOfftrackEvent.WalkingPace = args.WalkingPace;
            CurrentOfftrackEvent.MaxDistanceFromTrack = args.MaxDistanceFromTrack;
            CancelLog(new RouteWalkEventLog(CurrentOfftrackEvent));
        }
    }

    public void InterruptWalkStatus()
    {
        if (CurrentWalkStoppedEvent != null)
        {
            PopulateEndEventBase(CurrentWalkStoppedEvent);
            CancelLog(new RouteWalkEventLog(CurrentWalkStoppedEvent));
        }
    }

    private void CancelLog(RouteWalkEventLog log)
    {
        if (DisableLogging) return;

        log.Id = GetLastRouteWalkEventLog();
        log.WasEventInterrupted = true;
        log.InsertDirty();
    }

    private void PopulateStartEventBase(RouteWalkEventLogBase walkEvent, Pathpoint targetPOI)
    {
        walkEvent.StartPathpointLogId = CurrentPathpointLog.Id;
        walkEvent.StartTimestamp = CurrentPathpointLog.Timestamp;
        walkEvent.RouteWalkId = CurrentRouteWalk.Id;
        walkEvent.TargetPOIId = targetPOI?.Id;
    }

    private void PopulateEndEventBase(RouteWalkEventLogBase walkEvent)
    {
        walkEvent.EndPathpointLogId = CurrentPathpointLog.Id;
        walkEvent.EndTimestamp = CurrentPathpointLog.Timestamp;

        walkEvent.DurationEvent = CurrentPathpointLog.Timestamp - walkEvent.StartTimestamp;
    }

    private int GetLastRouteWalkEventLog()
    {
        var lastLog = RouteWalkEventLog.GetWithMinId(x => x.Id);
        return (lastLog != null ? lastLog.Id : 0) - 1;
    }
    

}