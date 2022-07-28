using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaganiniRestAPI
{
    //Base URL for the Rest APi
    public const string serverURL = "https://infinteg-main.fh-bielefeld.de/paganini/api/";

    //getUserProfile  param: apitoken: ""
    public const string getUserProfile = serverURL + "userManagement/userProfile";
    //getUserBegehungen  param: apitoken: "", wegid: ""
    public static string GetUserBegehungen(int wegid) { return serverURL + "weg/"+wegid.ToString()+"/begehung/"; }
    //getUserWege  param: apitoken: ""
    public const string getUserWege = serverURL + "weg/";

}
