using System;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class WayAPI : BaseAPI
{
    [JsonIgnore]
    public int way_id;

    public IAddressAPI way_start;
    public IAddressAPI way_destination;
    public string way_name;
    public string way_description;

    [JsonIgnore]
    public RouteAPIResult[] routes;
}

[System.Serializable]
public class WayAPIResult : WayAPI
{
    [JsonRequired]
    public int way_id;

    [JsonProperty]
    public AddressAPIResult way_start;
    [JsonProperty]
    public AddressAPIResult way_destination;

    [JsonProperty]
    public RouteAPIResult[] routes;
}

//TODO: We should move address to its own Table, and then
// serialise start and destination with the IDs
[System.Serializable]
public class WayAPIUpdate : WayAPI
{
    [JsonIgnore]
    public AddressAPIUpdate way_start;
    [JsonIgnore]
    public AddressAPIUpdate way_destination;
}

public class WayAPIList
{
    public WayAPIResult[] ways;
}