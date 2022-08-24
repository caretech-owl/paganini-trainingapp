using UnityEngine;
using UnityEngine.Android;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class BegehungsLocationService : MonoBehaviour
{
    /// <summary>
    /// Game Object to Display Geopositon Info in
    /// </summary>
    public GameObject LocationSection;

    private Boolean running = false;
    private Boolean rights = false;

  
    private  void Start()
    {
        DisplayLocation();
    //connect to sqlite
    DBConnector.Instance.Startup();

        if (AppState.SelectedBegehung != -1)
        {
            var list=DBConnector.Instance.GetConnection().Query<ExploratoryRouteWalk>("Select * from Begehung where beg_id=" + AppState.SelectedBegehung);
            foreach(ExploratoryRouteWalk b in list)
            {
                GameObject.Find("BegehungName").GetComponent<Text>().text = b.Name;
            }
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

    /// <summary>
    /// Prints theGeolocation to the GUI
    /// </summary>
    private void DisplayLocation()
    {
        if (LocationSection != null)
        {
            var list = LocationSection.GetComponentsInChildren<Text>();
            foreach (var df in list)
            {
                switch (df.name)
                {
                    case "DFTrackingStatus":
                        switch (UnityEngine.Input.location.status)
                        {
                            case LocationServiceStatus.Stopped:
                                df.text = "Stopped";
                                break;
                            case LocationServiceStatus.Running:
                                df.text = "Running";
                                break;
                            case LocationServiceStatus.Initializing:
                                df.text = "Initializing";
                                break;
                            default:
                                df.text = "Failed";
                                break;
                        }
                        break;
                    case "DFLatitude":
                        if (UnityEngine.Input.location.status == LocationServiceStatus.Running)
                        {
                            df.text = UnityEngine.Input.location.lastData.latitude.ToString();
                        }
                        else
                        {
                            df.text = "";
                        }
                        break;
                    case "DFLongitude":
                        if (UnityEngine.Input.location.status == LocationServiceStatus.Running)
                        {
                            df.text = UnityEngine.Input.location.lastData.longitude.ToString();
                        }
                        else
                        {
                            df.text = "";
                        }
                            break;
                    case "DFAltitude":
                        if (UnityEngine.Input.location.status == LocationServiceStatus.Running)
                        {
                            df.text = UnityEngine.Input.location.lastData.altitude.ToString();
                        }
                        else
                        {
                            df.text = "";
                        }
                        break;
                    case "DFAccuracy":
                        if (UnityEngine.Input.location.status == LocationServiceStatus.Running)
                        {
                            df.text = UnityEngine.Input.location.lastData.horizontalAccuracy.ToString();
                        }
                        else
                        {
                            df.text = "";
                        }
                        break;
                    case "DFLastUpdate":
                        if (UnityEngine.Input.location.status == LocationServiceStatus.Running)
                        {
                            df.text = (System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() - UnityEngine.Input.location.lastData.timestamp).ToString();
                        }
                        else
                        {
                            df.text = "";
                        }
                        break;
                    case "DFGetracktePunkte":
                        df.text=count.ToString();
                        break;
                }

            }
        }
    }

    private int count = 0;
    private double last = 0;
    /// <summary>
    /// Prints theGeolocation to the GUI
    /// </summary>
    private void Update()
    {
        DisplayLocation();
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
            DBConnector.Instance.GetConnection().Insert(GetCurrentWegpunkt());
            count = DBConnector.Instance.GetConnection().Query<Pathpoint>("SELECT * FROM Wegpunkt where beg_id=?", AppState.SelectedBegehung.ToString()).Count;
        }
    }


    /// <summary>
    /// neuen wegpunkt anlegen
    /// </summary>
    private Pathpoint GetCurrentWegpunkt()
    {
        Pathpoint punkt = new Pathpoint
        {
            Erw_id = AppState.SelectedBegehung,
            Longitude = UnityEngine.Input.location.lastData.longitude,
            Latitude = UnityEngine.Input.location.lastData.latitude,
            Altitude = UnityEngine.Input.location.lastData.altitude,
            Accuracy = UnityEngine.Input.location.lastData.horizontalAccuracy,
            POIType = -1,
            Timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Description = ""
        };
        return punkt;
    }
    /// <summary>
    /// neuen wegpunkt mit poi marker anlegen
    /// </summary>
    public void MarkPOI(int poiType)
    {
        Pathpoint currentLocation = GetCurrentWegpunkt();
        currentLocation.POIType = poiType;
        DBConnector.Instance.GetConnection().Insert(currentLocation);
    }

    /// <summary>
    /// Starts the Tracking Service Again
    /// </summary>
    public void RestartTracking()
    {

        
        DBConnector.Instance.GetConnection().Execute("DELETE FROM Pathpoint where Id=?", AppState.SelectedBegehung.ToString());
      
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