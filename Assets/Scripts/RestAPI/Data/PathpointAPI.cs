using System;
using Newtonsoft.Json;
using UnityEngine;

public interface IPathpointAPI
{
    public int ppoint_id { get; set; }
    public int erw_id { get; set; }
    public double ppoint_lon { get; set; }
    public double ppoint_lat { get; set; }
    public double ppoint_altitude { get; set; }
    public double ppoint_accuracy { get; set; }
    public int ppoint_poitype { get; set; }
    public string ppoint_timestamp { get; set; }
    public string ppoint_description { get; set; }
    public string ppoint_instruction { get; set; }
    public string ppoint_video_filename { get; set; }
    public string ppoint_notes { get; set; }
    public string ppoint_time_in_video { get; set; }
    public int? ppoint_relevance_feedback { get; set; }
    public int? ppoint_familiarity_feedback { get; set; }
    public int? ppoint_cleaning_feedback { get; set; }
    public bool IsNew { get; set; }

    public int? current_pim_id { get; set; }
    public PathpointPIMAPI current_pim { get; set; }

}

public class PathpointAPIBase : BaseAPI
{
    public double ppoint_lon { get; set; }
    public double ppoint_lat { get; set; }
    public double ppoint_altitude { get; set; }
    public double ppoint_accuracy { get; set; }
    public int ppoint_poitype { get; set; }
    public string ppoint_timestamp { get; set; }
    public string ppoint_description { get; set; }
    public string ppoint_instruction { get; set; }
    public string ppoint_video_filename { get; set; }
    public string ppoint_notes { get; set; }
    public string ppoint_time_in_video { get; set; }
    public int? ppoint_relevance_feedback { get; set; }
    public int? ppoint_familiarity_feedback { get; set; }
    public int? ppoint_cleaning_feedback { get; set; }
    
    public PathpointPIMAPI current_pim { get; set; }
}

[System.Serializable]
public class PathpointAPI : PathpointAPIBase, IPathpointAPI
{
    [JsonIgnore]
    public int ppoint_id { get; set; }

    [JsonIgnore]
    public int erw_id { get; set; }

    [JsonIgnore]
    public int? current_pim_id { get; set; }

}

[System.Serializable]
public class PathpointAPIResult : PathpointAPIBase, IPathpointAPI
{
    [JsonRequired]
    public int ppoint_id { get; set; }

    [JsonProperty]
    public int erw_id { get; set; }

    [JsonProperty]
    public int? current_pim_id { get; set; }

}

[System.Serializable]
public class PathpointAPIUpdate : PathpointAPIBase, IPathpointAPI
{
    [JsonRequired]
    public int ppoint_id { get; set; }

    [JsonIgnore]
    public int erw_id { get; set; }

    [JsonIgnore]
    public int? current_pim_id { get; set; }
}


public class PathpointAPIBatch : BaseAPI
{
    public IPathpointAPI[] pathpoints;
}


public class PathpointAPIList
{
    public PathpointAPIResult[] pathpoints;
}