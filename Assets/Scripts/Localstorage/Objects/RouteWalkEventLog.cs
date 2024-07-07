using System.Collections.Generic;
using System.Linq;
using SQLite4Unity3d;
using static LocationUtils;
using static Pathpoint;
using static PathpointPIM;
using static RouteWalkEventLog;

/// <summary>
/// Base class for route walk event logs.
/// </summary>
public abstract class RouteWalkEventLogBase
{
    public int Id { set; get; }
    public int RouteWalkId { set; get; }
    // type of event
    public RouteEvenLogType EvenLogType { set; get; }
    // reference to the pathpoint logs when the event starts and end
    public int StartPathpointLogId { set; get; }
    public int EndPathpointLogId { set; get; }

    // Starting / end time (for quick lookup)
    public long StartTimestamp { set; get; }
    public long EndTimestamp { set; get; }

    // reference to the POI it is occuring around, if any
    public int? TargetPOIId { set; get; }

    // basic stats
    public double DurationEvent { set; get; }

    public bool? WasEventInterrupted { set; get; }

    /// <summary>
    /// Enumeration for event types.
    /// </summary>
    public enum RouteEvenLogType
    {
        POIReached = 0,
        DecisionMade = 1,
        Offtrack = 2,
        Stopped = 3,
        Paused = 4,
        Sleep = 5,
        Adaptation = 6
    }
}

/// <summary>
/// Represents a route walk event log.
/// </summary>
public class RouteWalkEventLog : BaseModel<RouteWalkEventLog>
{

    [PrimaryKey]
    public int Id { set; get; }
    public int RouteWalkId { set; get; }
    // type of event
    public RouteWalkEventLogBase.RouteEvenLogType EvenLogType { set; get; }
    // reference to the pathpoint logs when the event starts and end
    public int StartPathpointLogId { set; get; }
    public int EndPathpointLogId { set; get; }

    // Starting / end time (for quick lookup)
    public long StartTimestamp { set; get; }
    public long EndTimestamp { set; get; }

    // Was event not completed?
    public bool? WasEventInterrupted { set; get; }

    // reference to the POI it is occuring around, if any
    public int? TargetPOIId { set; get; }

    // basic stats
    public double DurationEvent { set; get; }

    // performance stats
    public double? DistanceWalked { set; get; }
    public double? WalkingPace { set; get; }

    // stat for off-track
    public double? MaxDistanceFromTrack { set; get; }
    //           RecoveryInstructionUsed

    // data for decisionMade
    public bool? IsCorrectDecision { set; get; }
    //           NavIssue
    //           NavInstructionUsed
    //           DecisionExpected

    // data for InstructionSleep
    public bool? WasAwakenByUser { set; get; }


    // data for Adaptation
    public bool? AdaptationIntroShown { set; get; }
    public bool? AdaptationTaskAccepted { set; get; }
    public bool? AdaptationTaskCompleted { set; get; }
    public bool? AdaptationTaskCorrect { set; get; }
    public bool? AdaptationDowngradedByUser { set; get; }
    public bool? AdaptationDowngradedBySystem { set; get; }

    // SQLit has problems handling nullable enums, so a workaround for this:    
    public string NavIssueString { set; get; }
    public string DecisionExpectedString { set; get; }
    public string AdaptationSupportModeString { set; get; }
    public string NavInstructionUsedString { set; get; }
    public string RecoveryInstructionUsedString { set; get; }

    // data for segment SegmentCompleted
    public int? SegPOIStartId { set; get; }
    public int? SegExpectedPOIEndId { set; get; }
    public int? SegReachedPOIEndId { set; get; }
    public double? DistanceCorrectlyWalked { set; get; }

    public long? LastUpdate { set; get; }

    //TODO: Log skipped POI?

    [Ignore]
    public NavDirection? DecisionExpected
    {
        get => ConvertStringToNullableEnum<NavDirection>(DecisionExpectedString);
        set => DecisionExpectedString = ConvertNullableEnumToString(value);
    }
    [Ignore]
    public NavigationIssue? NavIssue
    {
        get => ConvertStringToNullableEnum<NavigationIssue>(NavIssueString);
        set => NavIssueString = ConvertNullableEnumToString(value);
    }
    [Ignore]
    public SupportMode? AdaptationSupportMode
    {
        get => ConvertStringToNullableEnum<SupportMode>(AdaptationSupportModeString);
        set => AdaptationSupportModeString = ConvertNullableEnumToString(value);
    }
    [Ignore]
    public List<NavInstructionType> NavInstructionUsed
    {
        get => ConvertStringToEnumList<NavInstructionType>(NavInstructionUsedString);
        set => NavInstructionUsedString = ConvertEnumListToString(value);
    }

    [Ignore]
    public List<RecoveryInstructionType> RecoveryInstructionUsed
    {
        get => ConvertStringToEnumList<RecoveryInstructionType>(RecoveryInstructionUsedString);
        set => RecoveryInstructionUsedString = ConvertEnumListToString(value);
    }

    public enum NavInstructionType
    {
        None = 0,
        Picture = 1,
        Video = 2,
        Compass = 3
    }

    public enum RecoveryInstructionType
    {
        None = 0, // self-recovery
        Compass = 1,
        CallHelp = 2,
        NoIssue = 3
    }

    public RouteWalkEventLog()
    {

    }

    public RouteWalkEventLog(DecisionMadeEvent walkEvent)
    {
        populateEvent(walkEvent);
        EvenLogType = RouteWalkEventLogBase.RouteEvenLogType.DecisionMade;
        DistanceWalked = walkEvent.DistanceWalked;
        WalkingPace = walkEvent.WalkingPace;
        IsCorrectDecision = walkEvent.IsCorrectDecision;
        NavIssue = walkEvent.NavIssue;
        DecisionExpected = walkEvent.DecisionExpected;
        NavInstructionUsed = walkEvent.NavInstructionUsed;
    }

    public RouteWalkEventLog(OffTrackEvent walkEvent)
    {
        populateEvent(walkEvent);
        EvenLogType = RouteWalkEventLogBase.RouteEvenLogType.Offtrack;
        DistanceWalked = walkEvent.DistanceWalked;
        WalkingPace = walkEvent.WalkingPace;
        MaxDistanceFromTrack = walkEvent.MaxDistanceFromTrack;
        RecoveryInstructionUsed = walkEvent.RecoveryInstructionUsed;
    }

    public RouteWalkEventLog(WalkStoppedEvent walkEvent)
    {
        populateEvent(walkEvent);
        EvenLogType = RouteWalkEventLogBase.RouteEvenLogType.Stopped;
    }

    public RouteWalkEventLog(PausedEvent walkEvent)
    {
        populateEvent(walkEvent);
        EvenLogType = RouteWalkEventLogBase.RouteEvenLogType.Paused;
    }

    public RouteWalkEventLog(InstructionSleepEvent sleepEvent)
    {
        populateEvent(sleepEvent);
        WasAwakenByUser = sleepEvent.WasAwakenByUser;
        EvenLogType = RouteWalkEventLogBase.RouteEvenLogType.Sleep;
    }

    public RouteWalkEventLog(SegmentCompletedEvent walkEvent)
    {
        populateEvent(walkEvent);
        EvenLogType = RouteWalkEventLogBase.RouteEvenLogType.POIReached;
        DistanceWalked = walkEvent.DistanceWalked;
        WalkingPace = walkEvent.WalkingPace;

        SegPOIStartId = walkEvent.SegPOIStartId;
        SegExpectedPOIEndId = walkEvent.SegExpectedPOIEndId;
        SegReachedPOIEndId = walkEvent.SegReachedPOIEndId;
        DistanceCorrectlyWalked = walkEvent.DistanceCorrectlyWalked;
    }

    public RouteWalkEventLog(AdaptationEvent adaptationEvent)
    {
        populateEvent(adaptationEvent);
        EvenLogType = RouteWalkEventLogBase.RouteEvenLogType.Adaptation;

        AdaptationSupportMode = adaptationEvent.AdaptationSupportMode;
        AdaptationIntroShown = adaptationEvent.AdaptationIntroShown;
        AdaptationTaskAccepted = adaptationEvent.AdaptationTaskAccepted;
        AdaptationTaskCompleted = adaptationEvent.AdaptationTaskCompleted;
        AdaptationTaskCorrect = adaptationEvent.AdaptationTaskCorrect;
        AdaptationDowngradedByUser = adaptationEvent.AdaptationDowngradedByUser;
        AdaptationDowngradedBySystem = adaptationEvent.AdaptationDowngradedBySystem;

        SegPOIStartId = adaptationEvent.SegPOIStartId;
        SegExpectedPOIEndId = adaptationEvent.SegExpectedPOIEndId;
        SegReachedPOIEndId = adaptationEvent.SegReachedPOIEndId;
    }

    public RouteWalkEventLog(IRouteWalkEventAPI eventAPI)
    {
        Id = eventAPI.revent_id;
        RouteWalkId = eventAPI.rw_id;
        EvenLogType = (RouteWalkEventLogBase.RouteEvenLogType)eventAPI.revent_type;
        StartPathpointLogId = eventAPI.start_rpath_id;
        EndPathpointLogId = eventAPI.end_rpath_id;
        if (eventAPI.revent_start_time != null)
        {
            StartTimestamp = (long)DateUtils.ConvertUTCStringToTsMilliseconds(eventAPI.revent_start_time, "yyyy-MM-dd'T'HH:mm:ss");
        }
        if (eventAPI.revent_end_time != null)
        {
            EndTimestamp = (long)DateUtils.ConvertUTCStringToTsMilliseconds(eventAPI.revent_end_time, "yyyy-MM-dd'T'HH:mm:ss");
        }

        TargetPOIId = eventAPI.target_ppoint_id;
        DurationEvent = eventAPI.revent_duration;
        WasEventInterrupted = eventAPI.revent_was_interrupted;
        DistanceWalked = eventAPI.revent_distance_walked;
        WalkingPace = eventAPI.revent_walking_pace;
        MaxDistanceFromTrack = eventAPI.revent_max_offtrack_distance;
        IsCorrectDecision = eventAPI.revent_decision_correct;
        NavIssueString = eventAPI.revent_decision_issue?.ToString();
        DecisionExpectedString = eventAPI.revent_decision_expected?.ToString();
        NavInstructionUsedString = eventAPI.revent_instruction_used?.ToString();
        RecoveryInstructionUsedString = eventAPI.revent_recovery_used?.ToString();
        SegPOIStartId = eventAPI.revent_seg_start_ppoint_id;
        SegExpectedPOIEndId = eventAPI.revent_seg_end_expected_ppoint_id;
        SegReachedPOIEndId = eventAPI.revent_seg_end_actual_ppoint_id;
        DistanceCorrectlyWalked = eventAPI.revent_seg_distance_walked_correctly;
    }

    public IRouteWalkEventAPI ToAPI()
    {
        IRouteWalkEventAPI eventAPI = new RouteWalkEventAPI();

        eventAPI.revent_id = Id;
        eventAPI.rw_id = RouteWalkId;
        eventAPI.revent_type = (int)EvenLogType;
        eventAPI.start_rpath_id = StartPathpointLogId;
        eventAPI.end_rpath_id = EndPathpointLogId;
        eventAPI.revent_start_time = DateUtils.ConvertMillisecondsToUTCString(StartTimestamp, "yyyy-MM-dd'T'HH:mm:ss");
        eventAPI.revent_end_time = DateUtils.ConvertMillisecondsToUTCString(EndTimestamp, "yyyy-MM-dd'T'HH:mm:ss");
        eventAPI.target_ppoint_id = TargetPOIId;
        eventAPI.revent_duration = DurationEvent;
        eventAPI.revent_was_interrupted = WasEventInterrupted;
        eventAPI.revent_distance_walked = DistanceWalked;
        eventAPI.revent_walking_pace = WalkingPace;
        eventAPI.revent_max_offtrack_distance = MaxDistanceFromTrack;
        eventAPI.revent_decision_correct = IsCorrectDecision;
        eventAPI.revent_decision_issue = NavIssue != null ? ((int)NavIssue).ToString() : null;
        eventAPI.revent_decision_expected = DecisionExpected != null ? ((int)DecisionExpected).ToString() : null;
        eventAPI.revent_instruction_used = NavInstructionUsedString;
        eventAPI.revent_recovery_used = RecoveryInstructionUsedString;
        eventAPI.revent_seg_start_ppoint_id = SegPOIStartId;
        eventAPI.revent_seg_end_expected_ppoint_id = SegExpectedPOIEndId;
        eventAPI.revent_seg_end_actual_ppoint_id = SegReachedPOIEndId;
        eventAPI.revent_seg_distance_walked_correctly = DistanceCorrectlyWalked;

        return eventAPI;
    }



    private void populateEvent(RouteWalkEventLogBase walkEvent)
    {
        // Populate common properties from walkEventBase
        Id = walkEvent.Id;
        RouteWalkId = walkEvent.RouteWalkId;
        StartPathpointLogId = walkEvent.StartPathpointLogId;
        EndPathpointLogId = walkEvent.EndPathpointLogId;
        StartTimestamp = walkEvent.StartTimestamp;
        EndTimestamp = walkEvent.EndTimestamp;
        TargetPOIId = walkEvent.TargetPOIId;
        DurationEvent = walkEvent.DurationEvent;
    }


    public static void ChangeParent(int oldRwId, int newRwId)
    {
        // Open the SQLliteConnection
        var conn = DBConnector.Instance.GetConnection();

        // Create the SQL command with the update query and parameters
        string cmdText = "UPDATE RouteWalkEventLog SET RouteWalkId = ? WHERE RouteWalkId = ?";
        SQLiteCommand cmd = conn.CreateCommand(cmdText, newRwId, oldRwId);

        // Execute the command
        cmd.ExecuteNonQuery();

    }


    public static void DeleteFromRouteWalk(int routeWalkId)
    {
        //// Open the SQLliteConnection
        var conn = DBConnector.Instance.GetConnection();

        // Prepare the base DELETE command text
        string cmdText = @"DELETE FROM RouteWalkEventLog 
                         WHERE RouteWalkId = ?";


        List<object> parameters = new List<object> { routeWalkId };

        //// Prepare the SQLiteCommand with the command text and parameters
        SQLiteCommand cmd = conn.CreateCommand(cmdText, parameters.ToArray());

        // Execute the command
        cmd.ExecuteNonQuery();
    }


    public static long? GetLastUpdateByRoute(int routeId)
    {

        var conn = DBConnector.Instance.GetConnection();

        // Query all PathpointPhotos and their related Pathpoints using sqlite-net's raw query
        //string cmdText = @"SELECT max(pd.LastUpdate) maxlastUpdate FROM PhotoData pd
        //                    JOIN PathpointPhoto pp ON pd.Id = pp.PhotoId
        //                    JOIN Pathpoint p ON pp.PathpointId = p.Id
        //                    WHERE p.RouteId = ?";

        string cmdText = @"SELECT re.*
            FROM RouteWalkEventLog re
    JOIN RouteWalk rw ON rw.Id = re.RouteWalkId
    WHERE rw.RouteId = ?
    ORDER BY re.StartTimestamp DESC
    LIMIT 1";

        List<RouteWalkEventLog> maxLastUpdate = conn.Query<RouteWalkEventLog>(cmdText, routeId).ToList();

        if (maxLastUpdate.Count > 0)
        {
            return maxLastUpdate[0].LastUpdate;
        }

        return null;
    }


}

/// <summary>
/// Represents a decision made event, extending <see cref="RouteWalkEventLogBase"/>.
/// </summary>
public class DecisionMadeEvent : RouteWalkEventLogBase
{
    // performance stats
    public double DistanceWalked { set; get; }
    public double WalkingPace { set; get; }

    // data for decisionMade
    public bool? IsCorrectDecision { set; get; }
    public NavigationIssue? NavIssue { set; get; }
    public Pathpoint.NavDirection? DecisionExpected { set; get; }
    public List<NavInstructionType> NavInstructionUsed;


}

/// <summary>
/// Represents an event when the route goes off track, extending <see cref="RouteWalkEventLogBase"/>.
/// </summary>
public class OffTrackEvent : RouteWalkEventLogBase
{
    // performance stats
    public double DistanceWalked { set; get; }
    public double WalkingPace { set; get; }

    // Stat for off-track
    public double MaxDistanceFromTrack { set; get; }
    public List<RecoveryInstructionType> RecoveryInstructionUsed;

}

/// <summary>
/// Represents an event when the walking is stopped, extending <see cref="RouteWalkEventLogBase"/>.
/// </summary>
public class WalkStoppedEvent : RouteWalkEventLogBase
{
    // No additional properties specific to stopped events
}

/// <summary>
/// Represents an event when the user paused the tracking, extending <see cref="RouteWalkEventLogBase"/>.
/// </summary>
public class PausedEvent : RouteWalkEventLogBase
{
    // No additional properties specific to stopped events
}

/// <summary>
/// Represents an event when the instructions are turned off to manage user attention, extending <see cref="RouteWalkEventLogBase"/>.
/// </summary>
public class InstructionSleepEvent : RouteWalkEventLogBase
{
    
    public bool WasAwakenByUser; // add to main 

}

/// <summary>
/// Represents an event when the user paused the tracking, extending <see cref="RouteWalkEventLogBase"/>.
/// </summary>
public class AdaptationEvent : RouteWalkEventLogBase
{
    public SupportMode? AdaptationSupportMode { set; get; }
    public bool? AdaptationIntroShown { set; get; }
    public bool? AdaptationTaskAccepted { set; get; }
    public bool? AdaptationTaskCompleted { set; get; }
    public bool? AdaptationTaskCorrect { set; get; }
    public bool? AdaptationDowngradedByUser { set; get; }
    public bool? AdaptationDowngradedBySystem { set; get; }
    //public Pathpoint.NavDirection? DecisionExpected { set; get; }

    public int? SegPOIStartId { set; get; }
    public int? SegExpectedPOIEndId { set; get; }
    public int? SegReachedPOIEndId { set; get; }
}

/// <summary>
/// Represents an event when a Point of Interest (POI) is reached, extending <see cref="RouteWalkEventLogBase"/>.
/// </summary>
public class SegmentCompletedEvent : RouteWalkEventLogBase
{
    // Performance stats
    public double DistanceWalked { set; get; }
    public double WalkingPace { set; get; }

    // data for segment SegmentCompleted
    public int? SegPOIStartId { set; get; }
    public int? SegExpectedPOIEndId { set; get; }
    public int? SegReachedPOIEndId { set; get; }
    public double DistanceCorrectlyWalked { set; get; }

}

