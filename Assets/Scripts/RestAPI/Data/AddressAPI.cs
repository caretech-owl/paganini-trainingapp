using System;
using Newtonsoft.Json;
using UnityEngine;


public interface IAddressAPI
{
    public int? adr_id { get; set; }
    public int user_id { get; set; }
    public string adr_name { get; set; }
    public string adr_streetname { get; set; }
    public string adr_housenumber { get; set; }
    public int adr_zipcode { get; set; }
    public string adr_city { get; set; }
    public string adr_icon { get; set; }

}

public class AddressAPIBase : BaseAPI
{
    public string adr_streetname { get; set; }
    public string adr_housenumber { get; set; }
    public int adr_zipcode { get; set; }
    public string adr_city { get; set; }
    public string adr_icon { get; set; }
    public string adr_name { get; set; }
}


[System.Serializable]
public class AddressAPI : AddressAPIBase, IAddressAPI
{
    [JsonIgnore]
    public int? adr_id { get; set; }
    [JsonIgnore]
    public int user_id { get; set; }
}


[System.Serializable]
public class AddressAPIResult : AddressAPIBase, IAddressAPI
{
    [JsonIgnore]
    public int? adr_id { get; set; }
    [JsonIgnore]
    public int user_id { get; set; }
}

[System.Serializable]
public class AddressAPIUpdate : BaseAPI, IAddressAPI
{
    [JsonProperty]
    public int? adr_id { get; set; }

    [JsonIgnore]
    public int user_id { get; set; }

    [JsonProperty]
    public string adr_name { get; set; }

    [JsonProperty]
    public string adr_icon { get; set; }

    [JsonIgnore]
    public string adr_streetname { get; set; }
    [JsonIgnore]
    public string adr_housenumber { get; set; }
    [JsonIgnore]
    public int adr_zipcode { get; set; }
    [JsonIgnore]
    public string adr_city { get; set; }


}

public class AddressAPIList
{
    public AddressAPIResult[] addresses;
}