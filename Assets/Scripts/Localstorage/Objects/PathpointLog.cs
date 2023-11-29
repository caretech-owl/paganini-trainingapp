using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using SQLite4Unity3d;

public class PathpointLog : BaseModel<PathpointLog>
{
    [PrimaryKey]
    public int Id { set; get; }
    public int RouteWalkId { set; get; }
    public double Longitude { set; get; }
    public double Latitude { set; get; }
    public double Altitude { set; get; }
    public double Accuracy { set; get; }
    public long Timestamp { set; get; }

    public int? TargetPOIId { set; get; }
    public int? SegPOIStartId { set; get; }
    public int? SegPOIEndId { set; get; }

    // basic walking stats
    public bool? IsWalking { set; get; }
    public double WalkingPace { set; get; }
    public double TotalWalkedDistance { set; get; }
    public double TotalWalkingTime { set; get; }

    public PathpointLog() { }

    public PathpointLog(Pathpoint pathpoint) {
        Longitude = pathpoint.Longitude;
        Latitude = pathpoint.Latitude;
        Altitude = pathpoint.Altitude;
        Accuracy = pathpoint.Accuracy;
        Timestamp = pathpoint.Timestamp;
    }

    public override string ToString()
    {
        return $"Id: {Id}, RouteWalkId: {RouteWalkId}, Longitude: {Longitude}, Latitude: {Latitude}, Altitude: {Altitude}, Accuracy: {Accuracy}, Timestamp: {Timestamp}, " +
               $"TargetPOIId: {TargetPOIId}, SegPOIStartId: {SegPOIStartId}, SegPOIEndId: {SegPOIEndId}, " +
               $"IsWalking: {IsWalking}, WalkingPace: {WalkingPace}, TotalWalkedDistance: {TotalWalkedDistance}, TotalWalkingTime: {TotalWalkingTime}";
    }


    public PathpointLog(IRouteWalkPathAPI pathAPI)
    {
        Id = pathAPI.rpath_id;
        RouteWalkId = pathAPI.rw_id;
        Longitude = pathAPI.rpath_lon;
        Latitude = pathAPI.rpath_lat;
        Altitude = pathAPI.rpath_altitude;
        Accuracy = pathAPI.rpath_accuracy;
        Timestamp = (long)DateUtils.ConvertUTCStringToTsMilliseconds(pathAPI.rpath_timestamp, "yyyy-MM-dd'T'HH:mm:ss");
        TargetPOIId = pathAPI.rpath_target_ppoint_id;
        SegPOIStartId = pathAPI.seg_start_ppoint_id;
        SegPOIEndId = pathAPI.seg_end_ppoint_id;
        IsWalking = pathAPI.stat_is_walking;
        WalkingPace = pathAPI.stat_walking_pace;
        TotalWalkedDistance = pathAPI.stat_curr_walking_distance;
        TotalWalkingTime = pathAPI.stat_curr_walking_time;
    }

    public IRouteWalkPathAPI ToAPI()
    {
        IRouteWalkPathAPI pathAPI = new RouteWalkPathAPI();

        pathAPI.rpath_id = Id;
        pathAPI.rw_id = RouteWalkId;
        pathAPI.rpath_lon = Longitude;
        pathAPI.rpath_lat = Latitude;
        pathAPI.rpath_altitude = Altitude;
        pathAPI.rpath_accuracy = Accuracy;
        pathAPI.rpath_timestamp = DateUtils.ConvertMillisecondsToUTCString(Timestamp, "yyyy-MM-dd'T'HH:mm:ss");
        pathAPI.rpath_target_ppoint_id = TargetPOIId;
        pathAPI.seg_start_ppoint_id = SegPOIStartId;
        pathAPI.seg_end_ppoint_id = SegPOIEndId;
        pathAPI.stat_is_walking = IsWalking;
        pathAPI.stat_walking_pace = WalkingPace;
        pathAPI.stat_curr_walking_distance = TotalWalkedDistance;
        pathAPI.stat_curr_walking_time = TotalWalkingTime;

        return pathAPI;
    }


    public static void ChangeParent(int oldRwId, int newRwId)
    {
        // Open the SQLliteConnection
        var conn = DBConnector.Instance.GetConnection();

        // Create the SQL command with the update query and parameters
        string cmdText = "UPDATE PathpointLog SET RouteWalkId = ? WHERE RouteWalkId = ?";
        SQLiteCommand cmd = conn.CreateCommand(cmdText, newRwId, oldRwId);

        // Execute the command
        cmd.ExecuteNonQuery();

    }


    public static void DeleteFromRouteWalk(int routeWalkId)
    {
        //// Open the SQLliteConnection
        var conn = DBConnector.Instance.GetConnection();

        // Prepare the base DELETE command text
        string cmdText = @"DELETE FROM PathpointLog 
                         WHERE RouteWalkId = ?";


        List<object> parameters = new List<object> { routeWalkId };

        //// Prepare the SQLiteCommand with the command text and parameters
        SQLiteCommand cmd = conn.CreateCommand(cmdText, parameters.ToArray());

        // Execute the command
        cmd.ExecuteNonQuery();
    }

}
