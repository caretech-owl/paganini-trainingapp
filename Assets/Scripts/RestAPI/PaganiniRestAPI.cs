using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ServerCommunication;
using UnityEngine.Events;
using System;
using static PaganiniRestAPI;

public class PaganiniRestAPI
{
    ////Base URL for the Rest APi
    //public const string serverURL = "https://infinteg-main.fh-bielefeld.de/paganini/api/usr/";
    ////public const string serverURL = "http://localhost:3000/usr/";
    ////getAuthToken  param: apitoken: ""
    //public const string getAuthToken = serverURL + "me/authentification";
    ////getUserProfile  param: apitoken: ""
    //public const string getUserProfile = serverURL + "me/profile";
    ////getUserBegehungen  param: apitoken: "", wegid: ""
    //public static string GetUserBegehungen(int wegid) { return serverURL + "way/"+wegid.ToString()+"/erw/"; }
    ////getUserWege  param: apitoken: ""
    //public const string getUserWege = serverURL + "me/ways/";

    public class Path
    {
        //Base URL for the Rest APi
        public const string BaseUrl = "https://infinteg-main.fh-bielefeld.de/paganini/api/usr/";
        public const string BaseSWUrl = "https://infinteg-main.fh-bielefeld.de/paganini/api/sw/";
        //public const string BaseUrl = "http://192.168.178.22:3000/usr/";

        public const string SWAuthenticate = BaseSWUrl + "me/authentification";
        public const string SwProfile = BaseSWUrl + "me/profile";

        public const string Authenticate = BaseUrl + "me/authentification";

        public const string UsrProfile = BaseUrl + "me/profile";

        public const string UsrWaysList = BaseUrl + "me/ways/";
        public const string UsrWays = BaseUrl + "me/ways/{0}";

        public const string UsrRoutesList = UsrWays + "/routes";
        public const string UsrRoutes = BaseUrl + "me/routes/{0}";

        public const string UsrPOIList = UsrRoutes + "/pois";

        public const string UsrPathpointList = UsrRoutes + "/pathpoints";

    }

    public class SocialWorker
    {

        public static void Authenticate(string username, string password, UnityAction<AuthTokenAPI> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password }
            };

            RESTAPI.Instance.Get<AuthTokenAPI>(Path.SWAuthenticate, successCallback, errorCallback, headers);
        }

        public static void GetProfile(UnityAction<SocialWorkerAPI> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.SWAPIToken }
            };

            string url = Path.SwProfile;

            RESTAPI.Instance.Get<SocialWorkerAPI>(url, successCallback, errorCallback, headers);
        }

    }

    public class User
    {

        public static void Authenticate(int pin, UnityAction<AuthTokenAPI> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "pin", pin.ToString() }
            };

            RESTAPI.Instance.Get<AuthTokenAPI>(Path.Authenticate, successCallback, errorCallback, headers);
        }

        public static void GetProfile(UnityAction<UserAPI> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.APIToken }
            };

            string url = Path.UsrProfile;

            RESTAPI.Instance.Get<UserAPI>(url, successCallback, errorCallback, headers);
        }

    }

    public class Way
    {

        public static void GetAll(UnityAction<WayAPIList> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.APIToken }
            };

            string url = Path.UsrWaysList;

            RESTAPI.Instance.Get<WayAPIList>(url, successCallback, errorCallback, headers);
        }

    }

    public class Route
    {

        public static void GetAll(Int32 wayId, UnityAction<RouteAPIList> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.APIToken }
            };

            string url = string.Format(Path.UsrRoutesList, wayId);

            RESTAPI.Instance.Get<RouteAPIList>(url, successCallback, errorCallback, headers);
        }

    }



    public class Pathpoint
    {
        public static void GetAll(Int32 routeId, UnityAction<PathpointAPIList> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.APIToken }
            };

            string url = string.Format(Path.UsrPathpointList, routeId);

            RESTAPI.Instance.Get<PathpointAPIList>(url, successCallback, errorCallback, headers);
        }

    }



    public class PathpointPOI
    {
        public static void GetAll(Int32 routeId, UnityAction<PathpointPOIAPIList> successCallback, UnityAction<string> errorCallback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "apitoken", AppState.APIToken }
            };

            string url = string.Format(Path.UsrPOIList, routeId);
            RESTAPI.Instance.Get<PathpointPOIAPIList>(url, successCallback, errorCallback, headers);
        }

    }

}
