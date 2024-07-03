using System;
using Newtonsoft.Json;

//public int Id { set; get; }
//public System.DateTime ActiveSinceDateTime { set; get; }
//public bool IsNewToUser { set; get; }
//public int PathpointId { set; get; }
//public SupportMode Mode { set; get; }

public interface IPathpointPIMAPI
{
    public int pim_id { get; set; }
    public int ppoint_id { get; set; }  
    public string pim_timestamp { get; set; }
    public bool pim_atpoi_isnew { get; set; }
    public bool pim_topoi_isnew { get; set; }
    public int pim_atpoi_mode { get; set; }
    public int pim_topoi_mode { get; set; }
}

public class PathpointPIMBase : BaseAPI
{
    public string pim_timestamp { get; set; }
    public bool pim_atpoi_isnew { get; set; }
    public bool pim_topoi_isnew { get; set; }
    public int pim_atpoi_mode { get; set; }
    public int pim_topoi_mode { get; set; }
}

[System.Serializable]
public class PathpointPIMAPI : PathpointPIMBase, IPathpointPIMAPI
{
    [JsonIgnore]
    public int pim_id { get; set; }

    [JsonIgnore]
    public int ppoint_id { get; set; }
}

[System.Serializable]
public class PathpointPIMAPIResult : PathpointPIMBase, IPathpointPIMAPI
{
    [JsonRequired]
    public int pim_id { get; set; }

    [JsonProperty]
    public int ppoint_id { get; set; }
}

[System.Serializable]
public class PathpointPIMAPIUpdate : PathpointPIMBase, IPathpointPIMAPI
{
    [JsonRequired]
    public int pim_id { get; set; }

    [JsonIgnore]
    public int ppoint_id { get; set; }
}

public class InstructionModeAPIList
{
    public PathpointPIMAPIResult[] pims;
}
