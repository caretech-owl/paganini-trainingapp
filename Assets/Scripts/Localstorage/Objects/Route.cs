using System.Collections.Generic;
using System.Data;
using System.Linq;
using SQLite4Unity3d;
//using SQLiteNetExtensions.Attributes;


public class Route : BaseModel<Route>
{

    [PrimaryKey]
    public int Id { set; get; }
    public string Name { set; get; }
    public System.DateTime Date { set; get; }
    public int Pin { set; get; }
    public RouteStatus Status { set; get; }
    public string LocalVideoFilename { set; get; }
    public string LocalVideoResolution { set; get; }
    public long? StartTimestamp { set; get; }
    public long? EndTimestamp { set; get; }
    public int SocialWorkerId { set; get; }

    //[Indexed]
    public int WayId { get; set; }

    [Ignore]
    public List<Pathpoint> Pathpoints { get; set; }

    public override string ToString()
    {
        return string.Format("[exploratory_route_walk: erw_id={0}, way_id={1}, erw_name={2},  erw_datum={3}, erw_pin={4}, erw_status=(5)]", Id, WayId, Name, Date, Pin, Status);
    }

    public enum RouteStatus
    {
        New,
        DraftPrepared,
        DraftNegotiated,
        Training,
        Completed,
        Discarded
    }

    public static Dictionary<RouteStatus, string> RouteStatusDescriptions = new Dictionary<RouteStatus, string>()
    {
        { RouteStatus.New, "New route" },
        { RouteStatus.DraftPrepared, "Draft prepared" },
        { RouteStatus.DraftNegotiated, "Draft negotiated" },
        { RouteStatus.Training, "Training in progress" },
        { RouteStatus.Completed, "Route completed" },
        { RouteStatus.Discarded, "Route discarded" },
    };

    public Route() { }
    public Route(RouteAPIResult erw)
    {
        this.Id = erw.erw_id;
        this.WayId = erw.way_id;
        this.Name = erw.erw_name;
        this.Date = System.DateTime.Parse(erw.erw_date);
        this.Pin = erw.erw_pin;
        this.LocalVideoFilename = erw.erw_video_url;
        if (erw.status != null)
        {
            this.Status = (RouteStatus)erw.status.erw_status_id;
        }
        this.FromAPI = true;

        LocalVideoResolution = erw.erw_video_resolution;
        StartTimestamp = DateUtils.ConvertStringToTsMilliseconds(erw.erw_start_time);
        EndTimestamp = DateUtils.ConvertStringToTsMilliseconds(erw.erw_end_time);
        SocialWorkerId = erw.erw_socialworker_id ?? -1;
    }

    public RouteAPI ToAPI()
    {
        RouteAPI erw = new RouteAPI
        {
            way_id = this.WayId,
            erw_name = this.Name,
            erw_date = Date.ToString("yyyy-MM-dd HH:mm:ss"),
            erw_pin = this.Pin,
            erw_video_url = this.LocalVideoFilename,
            status = new RouteStatusAPI { erw_status_id = (int)this.Status }
        };

        erw.erw_video_resolution = LocalVideoResolution;
        erw.erw_start_time = DateUtils.ConvertMillisecondsToString(StartTimestamp);
        erw.erw_end_time = DateUtils.ConvertMillisecondsToString(EndTimestamp);

        erw.erw_socialworker_id = SocialWorkerId != -1 ? SocialWorkerId : null;

        // Tag whether needs to be updated
        if (!FromAPI)
        {
            erw.IsNew = true;
        }
        else
        {
            erw.erw_id = this.Id;
        }

        return erw;
    }

    public static List<Route> GetRouteListByWay(int wayId)
    {
        List<Route> routes;

        var conn = DBConnector.Instance.GetConnection();

        // Query all Ways and their related Routes using sqlite-net's built-in mapping functionality

        routes = conn.Table<Route>().Where(r => r.WayId == wayId).ToList();

        return routes;
    }

    public static void ChangeParent(int oldWayId, int newWayId)
    {
        // Open the SQLliteConnection
        var conn = DBConnector.Instance.GetConnection();

        // Create the SQL command with the update query and parameters
        string cmdText = "UPDATE Route SET WayId = ? WHERE WayId = ?";
        SQLiteCommand cmd = conn.CreateCommand(cmdText, newWayId, oldWayId);

        // Execute the command
        cmd.ExecuteNonQuery();

    }

}