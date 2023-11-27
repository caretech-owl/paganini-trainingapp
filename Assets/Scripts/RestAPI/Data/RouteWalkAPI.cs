using System;
using Newtonsoft.Json;

public interface IRouteWalkAPI
{
    int rw_id { get; set; }
    int erw_id { get; set; }
    int rw_complete { get; set; }
    double rw_complete_percentage { get; set; }
    string rw_start_time { get; set; }
    string rw_end_time { get; set; }
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
}

[System.Serializable]
public class RouteWalkAPIUpdate : RouteWalkBase, IRouteWalkAPI
{
    [JsonRequired]
    public int rw_id { get; set; }

    [JsonIgnore]
    public int erw_id { get; set; }
}

public class RouteWalkAPIBatch : BaseAPI
{
    public IRouteWalkAPI[] routeWalks;
}

public class RouteWalkAPIList
{
    public RouteWalkAPIResult[] routeWalks;
}
