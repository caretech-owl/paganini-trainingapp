using System;
using Newtonsoft.Json;

public interface IRouteWalkPathAPI
{
    int rpath_id { get; set; }
    int rw_id { get; set; }
    double rpath_lat { get; set; }
    double rpath_lon { get; set; }
    double rpath_altitude { get; set; }
    double rpath_accuracy { get; set; }
    string rpath_timestamp { get; set; }
    int? seg_start_ppoint_id { get; set; }
    int? seg_end_ppoint_id { get; set; }
    int? rpath_target_ppoint_id { get; set; }
    bool? stat_is_walking { get; set; }
    double stat_walking_pace { get; set; }
    double stat_curr_walking_distance { get; set; }
    double stat_curr_walking_time { get; set; }
    int? local_rpath_id { get; set; } // for local references 
}

public class RouteWalkPathBase : BaseAPI
{
    public double rpath_lat { get; set; }
    public double rpath_lon { get; set; }
    public double rpath_altitude { get; set; }
    public double rpath_accuracy { get; set; }
    public string rpath_timestamp { get; set; }
    public int? seg_start_ppoint_id { get; set; }
    public int? seg_end_ppoint_id { get; set; }
    public int? rpath_target_ppoint_id { get; set; }
    public bool? stat_is_walking { get; set; }
    public double stat_walking_pace { get; set; }
    public double stat_curr_walking_distance { get; set; }
    public double stat_curr_walking_time { get; set; }
    public int? local_rpath_id { get; set; }
}

[System.Serializable]
public class RouteWalkPathAPI : RouteWalkPathBase, IRouteWalkPathAPI
{
    [JsonIgnore]
    public int rpath_id { get; set; }

    [JsonIgnore]
    public int rw_id { get; set; }
}

[System.Serializable]
public class RouteWalkPathAPIResult : RouteWalkPathBase, IRouteWalkPathAPI
{
    [JsonRequired]
    public int rpath_id { get; set; }

    [JsonProperty]
    public int rw_id { get; set; }
}

[System.Serializable]
public class RouteWalkPathAPIUpdate : RouteWalkPathBase, IRouteWalkPathAPI
{
    [JsonRequired]
    public int rpath_id { get; set; }

    [JsonIgnore]
    public int rw_id { get; set; }
}

public class RouteWalkPathAPIBatch : BaseAPI
{
    public IRouteWalkPathAPI[] routewalk_path;
}

public class RouteWalkPathAPIList
{
    public RouteWalkPathAPIResult[] routewalk_path;
}
