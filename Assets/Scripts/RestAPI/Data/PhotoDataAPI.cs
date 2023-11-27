using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public interface IPhotoDataAPI
{
    public int photo_id { get; set; }
    public string photo { get; set; }
    public string photo_reference { get; set; }
    public string last_update { get; set; }

    public bool IsNew { get; set; }
}

public class PhotoDataAPIBase : BaseAPI
{
    public string photo { get; set; }
}


[System.Serializable]
public class PhotoDataAPI : PhotoDataAPIBase, IPhotoDataAPI
{
    [JsonIgnore]
    public int photo_id { get; set; }

    [JsonIgnore]
    public string last_update { get; set; }

    [JsonProperty]
    public string photo_reference { get; set; }
}

[System.Serializable]
public class PhotoDataAPIResult : PhotoDataAPIBase, IPhotoDataAPI
{
    [JsonProperty]
    public int photo_id { get; set; }

    [JsonProperty]
    public string last_update { get; set; }

    [JsonIgnore]
    public string photo_reference { get; set; }
}

[System.Serializable]
public class PhotoDataAPIUpdate : PhotoDataAPIBase, IPhotoDataAPI
{
    [JsonProperty]
    public int photo_id { get; set; }

    [JsonIgnore]
    public string last_update { get; set; }

    [JsonProperty]
    public string photo_reference { get; set; }
}

public class PhotoDataAPIBatch : BaseAPI
{
    public IPhotoDataAPI[] data;

    [JsonIgnore]
    public Dictionary<string, byte[]> files;
}

public class PhotoDataAPIList
{
    public PhotoDataAPIResult[] data;
}
