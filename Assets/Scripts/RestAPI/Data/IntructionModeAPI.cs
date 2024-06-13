using System;
using Newtonsoft.Json;

//public int Id { set; get; }
//public System.DateTime ActiveSinceDateTime { set; get; }
//public bool IsNewToUser { set; get; }
//public int PathpointId { set; get; }
//public SupportMode Mode { set; get; }

public interface IInstructionModeAPI
{
    public int im_id { get; set; }
    public int ppoint_id { get; set; }  
    public string im_date { get; set; }
    public bool im_new { get; set; }
    public int im_mode { get; set; }
}

public class InstructionModeBase : BaseAPI
{
    public string im_date { get; set; }
    public bool im_new { get; set; }
    public int im_mode { get; set; }
}

[System.Serializable]
public class InstructionModeAPI : InstructionModeBase, IInstructionModeAPI
{
    [JsonIgnore]
    public int im_id { get; set; }

    [JsonIgnore]
    public int ppoint_id { get; set; }
}

[System.Serializable]
public class InstructionModeAPIResult : InstructionModeBase, IInstructionModeAPI
{
    [JsonRequired]
    public int im_id { get; set; }

    [JsonProperty]
    public int ppoint_id { get; set; }
}

[System.Serializable]
public class InstructionModeAPIUpdate : InstructionModeBase, IInstructionModeAPI
{
    [JsonRequired]
    public int im_id { get; set; }

    [JsonIgnore]
    public int ppoint_id { get; set; }
}

public class InstructionModeAPIList
{
    public InstructionModeAPIResult[] instructionModes;
}
