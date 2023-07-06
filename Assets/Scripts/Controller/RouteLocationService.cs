using UnityEngine;
using UnityEngine.Android;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class RouteLocationService : MonoBehaviour
{
    /// <summary>
    /// Game Object to Display Geopositon Info in
    /// </summary>
    public GameObject LocationSection;

    public PhoneCam PhoneCam;

    private Boolean running = false;
    private Boolean rights = false;


    private void Awake()
    {
        DBConnector.Instance.Startup();
    }

    private void Start()
    {

    }

    public void StartTracking()
    {
        if (AppState.SelectedBegehung != -1)
        {
            var route = Route.Get(AppState.SelectedBegehung);
            GameObject obj = GameObject.Find("BegehungName");
            if (obj != null)
            {
                obj.GetComponent<Text>().text = route.Name;
            }
            else
            {
                Debug.Log("Component BegehungName not present!");
            }

            // var list = DBConnector.Instance.GetConnection().Query<Route>("Select * from ExploratoryRouteWalk where Id=" + AppState.SelectedBegehung);
            //foreach(Route b in list)
            //{
            //    GameObject obj = GameObject.Find("BegehungName");
            //    if (obj != null)
            //    {
            //        obj.GetComponent<Text>().text = b.Name;
            //    } else {
            //        Debug.Log("Component BegehungName not present!");
            //    }
            //}
        }
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        this.rights = true;
#elif UNITY_IOS
        PlayerSettings.iOS.locationUsageDescription = "Details to use location";
#endif
        StartCoroutine(StartLocationService());
    }

    private IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("User has not enabled location");
            yield break;
        }
        Input.location.Start(5, 1);
        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            yield return new WaitForSeconds(1);
        }
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }
        Debug.Log("Latitude : " + Input.location.lastData.latitude);
        Debug.Log("Longitude : " + Input.location.lastData.longitude);
        Debug.Log("Altitude : " + Input.location.lastData.altitude);
    }


    private double last = 0;
    /// <summary>
    /// Prints theGeolocation to the GUI
    /// </summary>
    private void Update()
    {
        //check if tracking on
        if (!this.running) return;
        //check if tracking mode is running
        if (UnityEngine.Input.location.status != LocationServiceStatus.Running) return;
        //check if last position newer than last check
        if (UnityEngine.Input.location.lastData.timestamp == last)return;        
        last = UnityEngine.Input.location.lastData.timestamp;

        //punkt in db schreiben
        if (AppState.recording&&!AppState.pausedRec)
        {
            var pathpoint = GetCurrentPathpoint();
            pathpoint.Insert();

            //DBConnector.Instance.GetConnection().Insert();
            //count = DBConnector.Instance.GetConnection().Query<Pathpoint>("SELECT * FROM Pathpoint where RouteId=?", AppState.SelectedBegehung.ToString()).Count;
        }
    }


    /// <summary>
    /// neuen wegpunkt anlegen
    /// </summary>
    private Pathpoint GetCurrentPathpoint()
    {
        Pathpoint punkt = new Pathpoint
        {
            RouteId = AppState.SelectedBegehung,            
            Longitude = UnityEngine.Input.location.lastData.longitude,
            Latitude = UnityEngine.Input.location.lastData.latitude,
            Altitude = UnityEngine.Input.location.lastData.altitude,
            Accuracy = UnityEngine.Input.location.lastData.horizontalAccuracy,
            POIType = Pathpoint.POIsType.Point,
            Timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            FromAPI = false,
            IsDirty = true,
            Description = ""
        };
        punkt.Id = -(punkt.RouteId * 10000 + System.DateTime.Now.Minute * 1000 + System.DateTime.Now.Millisecond);
        return punkt;
    }

    /// <summary>
    /// neuen wegpunkt mit poi marker anlegen
    /// </summary>
    public void MarkPOI(int poiType)
    {
        Pathpoint poi = GetCurrentPathpoint();
        poi.POIType = (Pathpoint.POIsType)poiType;

        poi.Insert();

        //DBConnector.Instance.GetConnection().Insert(poi);
    }

    /// <summary>
    /// Saves a POI with a picture
    /// </summary>
    public void MarkPOIPhoto()
    {        
        Pathpoint currentLocation = GetCurrentPathpoint();
        currentLocation.POIType = Pathpoint.POIsType.Reassurance;

        DateTime currentTime = DateTimeOffset.FromUnixTimeMilliseconds(currentLocation.Timestamp).LocalDateTime;
        currentLocation.PhotoFilename = "recording_" + currentTime.ToString("yyyy_MM_dd_H_mm_ss_FF") + ".jpg";

        currentLocation.Insert();

        //DBConnector.Instance.GetConnection().Insert(currentLocation);

        PhoneCam.TakePicture(currentLocation.PhotoFilename);

    }

    /// <summary>
    /// Starts the Tracking Service Again
    /// </summary>
    public void RestartTracking()
    {
        Pathpoint.DeleteFromRoute(AppState.SelectedBegehung, null, null);
        
        //DBConnector.Instance.GetConnection().Execute("DELETE FROM Pathpoint where RouteId=?", AppState.SelectedBegehung.ToString());
      
        this.running = true;
      
    }

    /// <summary>
    /// Stop service if there is no need to query location updates continuously
    /// </summary>
    public void Stop()
    {
        if (this.running == true) { 
            UnityEngine.Input.location.Stop();
            this.running = false;
        }
    }

    /// <summary>
    /// Trunkates Local Wegepunk table
    /// </summary>
    public void EndBegehung()
    {
        Stop();
        AppState.wrapBegehung = true;
    }


}