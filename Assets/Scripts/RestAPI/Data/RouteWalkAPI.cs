using System;
using Newtonsoft.Json;

public interface IRouteWalkAPI
{
    public int rw_id { get; set; }
    public int erw_id { get; set; }
    public int rw_complete { get; set; }
    public double rw_complete_percentage { get; set; }
    public string rw_start_time { get; set; }
    public string rw_end_time { get; set; }

}

public class RouteWalkBase : BaseAPI
{
    public int rw_complete { get; set; }
    public double rw_complete_percentage { get; set; }
    public string rw_start_time { get; set; }
    public string rw_end_time { get; set; }
}

[System.Serializable]
public class RouteWalkAPI : RouteWalkBase, IRouteWalkAPI
{
    [JsonIgnore]
    public int rw_id { get; set; }

    [JsonIgnore]
    public int erw_id { get; set; }

}

[System.Serializable]
public class RouteWalkAPIResult : RouteWalkBase, IRouteWalkAPI
{
    [JsonRequired]
    public int rw_id { get; set; }

    [JsonProperty]
    public int erw_id { get; set; }


    public RouteWalkEventAPIResult[] routewalk_events;
    public RouteWalkPathAPIResult[] routewalk_paths;

}

[System.Serializable]
public class RouteWalkAPIUpdate : RouteWalkBase, IRouteWalkAPI
{
    [JsonRequired]
    public int rw_id { get; set; }

    [JsonIgnore]
    public int erw_id { get; set; }

    [JsonIgnore]
    public RouteWalkEventAPIResult[] routewalk_events { get; set; }

    [JsonIgnore]
    public RouteWalkPathAPIResult[] routewalk_paths { get; set; }

}

public class RouteWalkAPIBatch : BaseAPI
{
    public IRouteWalkAPI[] routeWalks;
}

public class RouteWalkAPIList
{
    public RouteWalkAPIResult[] routeWalks;
}
