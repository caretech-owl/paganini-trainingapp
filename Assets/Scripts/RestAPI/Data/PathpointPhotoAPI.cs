using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public interface IPathpointPhotoAPI
{
    public int pphoto_id { get; set; }
    public int ppoint_id { get; set; }
    public string pphoto_description { get; set; }
    public IPhotoDataAPI data { get; set; }
    public int photo_id { get; set; }
    public string photo_reference { get; set; }
    public string pphoto_timestamp { get; set; }
    public int? pphoto_cleaning_feedback { get; set; }
    public int? pphoto_discussion_feedback { get; set; }
    public bool IsNew { get; set; }
}

public class PathpointPhotoAPIBase : BaseAPI
{
    public string pphoto_description { get; set; }
    public IPhotoDataAPI data { get; set; }
    public string pphoto_timestamp { get; set; }
    public int? pphoto_cleaning_feedback { get; set; }
    public int? pphoto_discussion_feedback { get; set; }
}


[System.Serializable]
public class PathpointPhotoAPI : PathpointPhotoAPIBase, IPathpointPhotoAPI
{
    [JsonIgnore]
    public int pphoto_id { get; set; }

    [JsonIgnore]
    public int ppoint_id { get; set; }

    [JsonProperty]
    public string photo_reference { get; set; }

    [JsonProperty]
    public PhotoDataAPI data { get; set; }

    [JsonIgnore]
    public int photo_id { get; set; }
}

[System.Serializable]
public class PathpointPhotoAPIResult : PathpointPhotoAPIBase, IPathpointPhotoAPI
{
    [JsonProperty]
    public int pphoto_id { get; set; }

    [JsonProperty]
    public int ppoint_id { get; set; }

    [JsonIgnore]
    public string photo_reference { get; set; }

    [JsonIgnore]
    public PhotoDataAPIResult data { get; set; }

    [JsonProperty]
    public int photo_id { get; set; }
}

[System.Serializable]
public class PathpointPhotoAPIUpdate : PathpointPhotoAPIBase, IPathpointPhotoAPI
{
    [JsonProperty]
    public int pphoto_id { get; set; }

    [JsonIgnore]
    public int ppoint_id { get; set; }

    [JsonIgnore]
    public string photo_reference { get; set; }

    [JsonProperty]
    public PhotoDataAPIUpdate data { get; set; }

    [JsonIgnore]
    public int photo_id { get; set; }
}

[System.Serializable]
public class PathpointPhotoAPIBatchCreateElement : PathpointPhotoAPIBase, IPathpointPhotoAPI
{
    [JsonIgnore]
    public int pphoto_id { get; set; }

    [JsonProperty]
    public int ppoint_id { get; set; }

    [JsonProperty]
    public string photo_reference { get; set; }

    [JsonProperty]
    public PhotoDataAPI data { get; set; }

    [JsonIgnore]
    public int photo_id { get; set; }
}

[System.Serializable]
public class PathpointPhotoAPIBatchUpdateElement : PathpointPhotoAPIBase, IPathpointPhotoAPI
{
    [JsonProperty]
    public int pphoto_id { get; set; }

    [JsonProperty]
    public int ppoint_id { get; set; }

    [JsonProperty]
    public string photo_reference { get; set; }

    [JsonProperty]
    public PhotoDataAPIUpdate data { get; set; }

    [JsonIgnore]
    public int photo_id { get; set; }
}


public class PathpointPhotoAPIList
{
    public PathpointPhotoAPIResult[] photos;
}


public class PathpointPhotoAPIBatch : BaseAPI
{
    public IPathpointPhotoAPI[] photos;

    [JsonIgnore]
    public Dictionary<string, byte[]> files;
}