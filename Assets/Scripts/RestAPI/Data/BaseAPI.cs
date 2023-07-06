using System;
using Newtonsoft.Json;

public class BaseAPI 
{
    [JsonIgnore]
    public bool IsNew { get; set; }
}