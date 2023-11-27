using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PaganiniRestAPI;

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
    private WalkStoppedEvent CurrentWalkStoppedEvent;
    private OffTrackEvent CurrentOfftrackEvent;
    private DecisionMadeEvent CurrentDecisionEvent;
    private SegmentCompletedEvent CurrentSegmentEvent;


    public WalkEventManager() { }

    public void SetCurrentPathpointLog(PathpointLog log)
    {
        CurrentPathpointLog = log;
    }

    public void ProcessWalkStatusChange(WalkingStatusArgs args)
    {
        // The user stopped
        if (CurrentWalkStoppedEvent == null && !args.IsWalking)
        {
            CurrentWalkStoppedEvent = new WalkStoppedEvent();
            PopulateStartEventBase(CurrentWalkStoppedEvent, args.TargetPOI);
        }
        // We are resuming walking
        else if (CurrentWalkStoppedEvent != null && args.IsWalking)
        {
            PopulateEndEventBase(CurrentWalkStoppedEvent);

            var e = new RouteWalkEventLog(CurrentWalkStoppedEvent);
            e.Id = GetLastRouteWalkEventLog();
            e.InsertDirty();

            // we free the event
            CurrentWalkStoppedEvent = null;
        }

    }

    public void ProcessOfftrackStatusChange(ValidationArgs args)
    {
        // The user is off-track
        if (CurrentOfftrackEvent == null && !args.OnTrack)
        {
            CurrentOfftrackEvent = new OffTrackEvent();
            PopulateStartEventBase(CurrentOfftrackEvent, args.TargetPOI);
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
            e.InsertDirty();

            // we free the event
            CurrentOfftrackEvent = null;
        }
    }

    public void ProcessDecisionStatusChange(DecisionArgs args)
    {
        // The user is off-track
        if (CurrentDecisionEvent == null && args.OnTargetPOI)
        {
            CurrentDecisionEvent = new DecisionMadeEvent();
            PopulateStartEventBase(CurrentDecisionEvent, args.TargetPOI);

            CurrentDecisionEvent.DecisionExpected = args.DecisionExpected;

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
            e.InsertDirty();

            // we free the event
            CurrentDecisionEvent = null;
        }
    }

    public void ProcessSegmentCompleteStatusChange(SegmentCompletedArgs args)
    {
        // The user is off-track
        if (CurrentSegmentEvent == null && !args.HasArrived)
        {
            CurrentSegmentEvent = new SegmentCompletedEvent();
            PopulateStartEventBase(CurrentSegmentEvent, args.TargetPOI);
            CurrentSegmentEvent.SegPOIStartId = args.SegPOIStartId;
            CurrentSegmentEvent.SegExpectedPOIEndId = args.SegExpectedPOIEndId;

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

            // we free the event
            CurrentSegmentEvent = null;
        }
    }

    private void PopulateStartEventBase(RouteWalkEventLogBase walkEvent, Pathpoint targetPOI)
    {
        walkEvent.StartPathpointLogId = CurrentPathpointLog.Id;
        walkEvent.StartTimestamp = CurrentPathpointLog.Timestamp;
        walkEvent.RouteWalkId = CurrentPathpointLog.RouteWalkId;
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