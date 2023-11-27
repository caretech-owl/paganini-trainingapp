using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using SQLite4Unity3d;

public class Pathpoint : BaseModel<Pathpoint>
{
    [PrimaryKey]
    public int Id { set; get; }
    public int RouteId { set; get; }
    public double Longitude { set; get; }
    public double Latitude { set; get; }
    public double Altitude { set; get; }
    public double Accuracy { set; get; }
    public POIsType POIType { set; get; }
    public long Timestamp { set; get; }
    public string Description { set; get; }
    public string Notes { set; get; }
    public string PhotoFilename { set; get; }
    public NavDirection Instruction { set; get; }
    public double? TimeInVideo { set; get; }
    public POIFeedback CleaningFeedback { set; get; }
    public POIFeedback RelevanceFeedback { set; get; }
    public POIFeedback FamiliarityFeedback { set; get; }

    [Ignore]
    public List<PathpointPhoto> Photos { get; set; }
    [Ignore]
    public List<string> PhotoFilenames { get; set; }

    public enum POIFeedback
    {
        None = 0,
        Yes = 1,
        No = 2,
    }

    public enum NavDirection
    {
        None = 0,
        Straight = 1,
        RightTurn = 2,
        LeftTurn = 3        
    }

    public enum POIsType
    {
        [XmlEnum("-1")]
        Point = -1,
        [XmlEnum("1")]
        Reassurance = 1,
        [XmlEnum("2")]
        Landmark = 2,
        [XmlEnum("3")]
        WayStart = 3,
        [XmlEnum("4")]
        WayDestination = 4
    }


    public Pathpoint() { }

    public Pathpoint(PathpointAPIResult pathpoint)
    {

        Id = pathpoint.ppoint_id;
        RouteId = pathpoint.erw_id;
        Longitude = pathpoint.ppoint_lon;
        Latitude = pathpoint.ppoint_lat;
        Altitude = pathpoint.ppoint_altitude;
        Accuracy = pathpoint.ppoint_accuracy;
        POIType = (POIsType)pathpoint.ppoint_poitype;
        Description = pathpoint.ppoint_description;
        Notes = pathpoint.ppoint_notes;

        string direction = pathpoint.ppoint_instruction == "" || pathpoint.ppoint_instruction == null ? "None" : pathpoint.ppoint_instruction;
        Instruction = Enum.Parse<NavDirection>(direction);

        RelevanceFeedback = ((POIFeedback?)pathpoint.ppoint_relevance_feedback) ?? POIFeedback.None;
        FamiliarityFeedback = ((POIFeedback?)pathpoint.ppoint_familiarity_feedback) ?? POIFeedback.None;
        CleaningFeedback = ((POIFeedback?)pathpoint.ppoint_cleaning_feedback) ?? POIFeedback.None;

        FromAPI = true;

        Timestamp = (long)DateUtils.ConvertUTCStringToTsMilliseconds(pathpoint.ppoint_timestamp, "yyyy-MM-dd'T'HH:mm:ss");


        TimeInVideo = pathpoint.ppoint_time_in_video == null ? null : Double.Parse(pathpoint.ppoint_time_in_video);
    }

    public static List<Pathpoint> GetPathpointListByRoute(int routeId, Func<Pathpoint, bool> whereCondition = null)
    {
        List<Pathpoint> pathpoints;

        var conn = DBConnector.Instance.GetConnection();

        // Query all Ways and their related Routes using sqlite-net's built-in mapping functionality

        var pathpointQuery = conn.Table<Pathpoint>().Where(p => p.RouteId == routeId).AsEnumerable();

        if (whereCondition != null)
        {
            pathpointQuery = pathpointQuery.Where(whereCondition);
        }

        pathpoints = pathpointQuery.OrderBy(p => p.Timestamp).ToList();

        return pathpoints;
    }


    /// <summary>
    /// Deletes Pathpoint records from the database based on the specified route ID, 'FromAPI' flags, and POI types.
    /// </summary>
    /// <param name="routeId">The ID of the route to which the Pathpoint records are associated.</param>
    /// <param name="fromAPI">An array of boolean values representing the 'FromAPI' flag of the Pathpoint records to be deleted. If multiple values are provided, records matching any of the values will be deleted.</param>
    /// <param name="types">An array of Pathpoint.POIsType enum values representing the POI types of the Pathpoint records to be deleted. If multiple values are provided, records matching any of the types will be deleted.</param>
    /// <remarks>
    /// The method constructs an SQL DELETE command with the specified conditions and executes it using an SQLiteCommand object.
    /// </remarks>
    public static void DeleteFromRoute(int routeId, bool[] fromAPI, Pathpoint.POIsType[] types)
    {
        // Open the SQLliteConnection
        var conn = DBConnector.Instance.GetConnection();

        // Prepare the base DELETE command text
        string cmdText = "DELETE FROM Pathpoint WHERE RouteId = ?";

        List<object> parameters = new List<object> { routeId };

        // Add conditions for FromAPI
        if (fromAPI != null && fromAPI.Length > 0)
        {
            var fromAPIConditions = string.Join(" OR ", fromAPI.Select((val, idx) => $"FromAPI = ?"));
            cmdText += $" AND ({fromAPIConditions})";
            parameters.AddRange(fromAPI.OfType<object>());
        }

        // Add conditions for POIType
        if (types != null && types.Length > 0)
        {
            var typeConditions = string.Join(" OR ", types.Select((val, idx) => $"POIType = ?"));
            cmdText += $" AND ({typeConditions})";
            parameters.AddRange(types.OfType<object>());
        }

        // Prepare the SQLiteCommand with the command text and parameters
        SQLiteCommand cmd = conn.CreateCommand(cmdText, parameters.ToArray());

        // Execute the command
        cmd.ExecuteNonQuery();
    }



    public IPathpointAPI ToAPI()
    {
        IPathpointAPI pp;
        // For an update statement
        if (FromAPI)
        {
            pp = new PathpointAPIUpdate();
            pp.ppoint_id = Id;
            pp.IsNew = false;
        }
        // For a post statement
        else
        {
            pp = new PathpointAPI();
            pp.IsNew = true;
        }


        pp.ppoint_lon = Longitude;
        pp.ppoint_lat = Latitude;
        pp.ppoint_altitude = Altitude;
        pp.ppoint_accuracy = Accuracy;
        pp.ppoint_poitype = (int)POIType;
        pp.ppoint_timestamp = DateUtils.ConvertMillisecondsToUTCString(Timestamp, "yyyy-MM-dd'T'HH:mm:ss");
        pp.ppoint_description = Description;
        pp.ppoint_notes = Notes;
        pp.ppoint_instruction = Instruction.ToString();
        pp.ppoint_time_in_video = TimeInVideo != null ? TimeInVideo.Value.ToString("0.00", CultureInfo.InvariantCulture) : null;

        pp.ppoint_relevance_feedback = (int)RelevanceFeedback;
        pp.ppoint_familiarity_feedback = (int)FamiliarityFeedback;
        pp.ppoint_cleaning_feedback = (int)CleaningFeedback;

        return pp;
    }



}
