using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaganiniRestAPI
{
    //Base URL for the Rest APi
    public const string serverURL = "https://infinteg-main.fh-bielefeld.de/paganini/api/";
    //getAuthToken  param: apitoken: ""
    public const string getAuthToken = serverURL + "userManagement/authentification";
    //getUserProfile  param: apitoken: ""
    public const string getUserProfile = serverURL + "userManagement/userProfile";
    //getUserBegehungen  param: apitoken: "", wegid: ""
    public static string GetUserBegehungen(int wegid) { return serverURL + "way/"+wegid.ToString()+"/erw/"; }
    //getUserWege  param: apitoken: ""
    public const string getUserWege = serverURL + "way/";

}
