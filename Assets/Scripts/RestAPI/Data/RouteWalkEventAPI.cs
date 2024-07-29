using System;
using Newtonsoft.Json;

public interface IRouteWalkEventAPI
{
    int revent_id { get; set; }
    int rw_id { get; set; }
    int revent_type { get; set; }
    int start_rpath_id { get; set; }
    int end_rpath_id { get; set; }
    string revent_start_time { get; set; }
    string revent_end_time { get; set; }
    bool? revent_was_interrupted { get; set; }
    int? target_ppoint_id { get; set; }
    double revent_duration { get; set; }
    double? revent_distance_walked { get; set; }
    double? revent_walking_pace { get; set; }
    double? revent_max_offtrack_distance { get; set; }
    bool? revent_decision_correct { get; set; }
    string revent_decision_issue { get; set; }
    string revent_decision_expected { get; set; }
    string revent_instruction_used { get; set; }
    string revent_recovery_used { get; set; }

    bool? revent_was_awaken_byuser { set; get; }
    string revent_adapt_support_mode { get; set; }
    bool? revent_adapt_intro_shown { set; get; }
    bool? revent_adapt_task_accepted { set; get; }
    bool? revent_adapt_task_completed { set; get; }
    bool? revent_adapt_task_correct { set; get; }
    bool? revent_adapt_downgraded_byuser { set; get; }
    bool? revent_adapt_downgraded_bysystem { set; get; }

    int? revent_seg_start_ppoint_id { get; set; }
    int? revent_seg_end_expected_ppoint_id { get; set; }
    int? revent_seg_end_actual_ppoint_id { get; set; }
    double? revent_seg_distance_walked_correctly { get; set; }
}

public class RouteWalkEventBase : BaseAPI
{
    public int revent_type { get; set; }
    public int start_rpath_id { get; set; }
    public int end_rpath_id { get; set; }
    public string revent_start_time { get; set; }
    public string revent_end_time { get; set; }
    public int? target_ppoint_id { get; set; }
    public double revent_duration { get; set; }
    public bool? revent_was_interrupted { get; set; }
    public double? revent_distance_walked { get; set; }
    public double? revent_walking_pace { get; set; }
    public double? revent_max_offtrack_distance { get; set; }
    public bool? revent_decision_correct { get; set; }
    public string revent_decision_issue { get; set; }
    public string revent_decision_expected { get; set; }
    public string revent_instruction_used { get; set; }
    public string revent_recovery_used { get; set; }
    public int? revent_seg_start_ppoint_id { get; set; }
    public int? revent_seg_end_expected_ppoint_id { get; set; }
    public int? revent_seg_end_actual_ppoint_id { get; set; }
    public double? revent_seg_distance_walked_correctly { get; set; }

    public bool? revent_was_awaken_byuser { set; get; }
    public string revent_adapt_support_mode { get; set; }
    public bool? revent_adapt_intro_shown { set; get; }
    public bool? revent_adapt_task_accepted { set; get; }
    public bool? revent_adapt_task_completed { set; get; }
    public bool? revent_adapt_task_correct { set; get; }
    public bool? revent_adapt_downgraded_byuser { set; get; }
    public bool? revent_adapt_downgraded_bysystem { set; get; }
}

[System.Serializable]
public class RouteWalkEventAPI : RouteWalkEventBase, IRouteWalkEventAPI
{
    [JsonIgnore]
    public int revent_id { get; set; }

    [JsonIgnore]
    public int rw_id { get; set; }
}

[System.Serializable]
public class RouteWalkEventAPIResult : RouteWalkEventBase, IRouteWalkEventAPI
{
    [JsonRequired]
    public int revent_id { get; set; }

    [JsonProperty]
    public int rw_id { get; set; }
}

[System.Serializable]
public class RouteWalkEventAPIUpdate : RouteWalkEventBase, IRouteWalkEventAPI
{
    [JsonRequired]
    public int revent_id { get; set; }

    [JsonIgnore]
    public int rw_id { get; set; }
}

public class RouteWalkEventAPIBatch : BaseAPI
{
    public IRouteWalkEventAPI[] routewalk_events;
}

public class RouteWalkEventAPIList
{
    public RouteWalkEventAPIResult[] routewalk_events;
}
