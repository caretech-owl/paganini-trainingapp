using System;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class AddressAPI : BaseAPI
{
    [JsonIgnore]
    public int adr_id;
    [JsonIgnore]
    public int user_id;

	public string adr_streetname;
	public string adr_housenumber;
	public int adr_zipcode;
	public string adr_city;
	public string adr_icon;
	public string adr_name;
}

[System.Serializable]
public class AddressAPIResult : AddressAPI
{
    [JsonIgnore]
    public int adr_id;
    [JsonIgnore]
    public int user_id;
}

[System.Serializable]
public class AddressAPIUpdate : BaseAPI
{
    public int adr_id;
}

public class AddressAPIList
{
	public AddressAPIResult[] addresses;
}