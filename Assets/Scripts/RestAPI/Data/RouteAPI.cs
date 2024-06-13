using System;
using Newtonsoft.Json;
using UnityEngine;

public interface IRouteAPI 
{
    public int erw_id { get; set; }
    public int way_id { get; set; }
    public string erw_name { get; set; }
    public string erw_date { get; set; }
    public int erw_pin { get; set; }
    public string erw_video_url { get; set; }
    public string erw_video_resolution { get; set; }
    public string erw_start_time { get; set; }
    public string erw_end_time { get; set; }
    public int? erw_socialworker_id { get; set; }
    public RouteStatusAPI status { get; set; }
    public string photo_start { set; get; }
    public string photo_destination { set; get; }
    public bool IsNew { get; set; }
}

public class RouteAPIBase : BaseAPI
{
    public string erw_name { get; set; }
    public string erw_date { get; set; }
    public int erw_pin { get; set; }
    public string erw_video_url { get; set; }
    public string erw_video_resolution { get; set; }
    public string erw_start_time { get; set; }
    public string erw_end_time { get; set; }
    public int? erw_socialworker_id { get; set; }
    public RouteStatusAPI status { get; set; }
}

[System.Serializable]
public class RouteAPI : RouteAPIBase, IRouteAPI
{
    [JsonIgnore]
    public int erw_id { get; set; }

    [JsonIgnore]
    public int way_id { get; set; }

    [JsonIgnore]
    public string photo_start { set; get; }

    [JsonIgnore]
    public string photo_destination { set; get; }
}

[System.Serializable]
public class RouteAPIResult : RouteAPIBase, IRouteAPI
{
    [JsonProperty]
    public int erw_id { get; set; }

    [JsonProperty]
    public int way_id { get; set; }

    [JsonProperty]
    public string photo_start { set; get; }

    [JsonProperty]
    public string photo_destination { set; get; }
}

public class RouteAPIList
{
    public RouteAPIResult[] erw;
}


[System.Serializable]
public class RouteStatusAPI
{
    public int erw_status_id;
    public string erw_status_name;
}