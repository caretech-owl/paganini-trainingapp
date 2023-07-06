using System;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class WayAPI : BaseAPI
{
    [JsonIgnore]
    public int way_id;

	public AddressAPI way_start;
	public AddressAPI way_destination;
	public string way_name;
	public string way_description;

    [JsonIgnore]
    public RouteAPIResult [] routes;
}

[System.Serializable]
public class WayAPIResult : WayAPI
{
    [JsonRequired]
    public int way_id;

    [JsonProperty]
    public RouteAPIResult[] routes;
}

//TODO: We should move address to its own Table, and then
// serialise start and destination with the IDs
[System.Serializable]
public class WayAPIUpdate : WayAPI
{
    [JsonIgnore]
    public AddressAPI way_start;
    [JsonIgnore]
    public AddressAPI way_destination;
}

public class WayAPIList
{
	public WayAPIResult[] ways;
}