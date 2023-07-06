using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


[System.Serializable]
public class PathpointPOIAPI : BaseAPI
{
    public PathpointAPI pathpoint;

    public IPathpointPhotoAPI [] photos;
}

[System.Serializable]
public class PathpointPOIAPIResult : BaseAPI
{
    public PathpointAPIResult pathpoint;

    public PathpointPhotoAPIResult [] photos;
}

public class PathpointPOIAPIList
{
    public PathpointPOIAPIResult [] pois;
}

public class PathpointPOIAPIBatch : BaseAPI
{
    public PathpointPOIAPI[] pois;

    [JsonIgnore]
    public Dictionary<string, byte[]> files;
}