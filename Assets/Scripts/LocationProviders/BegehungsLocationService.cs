using UnityEngine;
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

    private void startLocationService()
    {
        this.running = true;
        UnityEngine.Input.location.Start(5, 1);
    }
    private 
    IEnumerator Start()
    {
        DisplayLocation();
    //connect to sqlite
    DBConnector.Instance.Startup();

        if (AppState.SelectedBegehung != -1)
        {
            var list=DBConnector.Instance.GetConnection().Query<Begehung>("Select * from Begehung where beg_id=" + AppState.SelectedBegehung);
            foreach(Begehung b in list)
            {
                GameObject.Find("BegehungName").GetComponent<Text>().text = b.beg_name;
            }
        }

#if UNITY_EDITOR
        yield return new WaitWhile(() => !UnityEditor.EditorApplication.isRemoteConnected);
                yield return new WaitForSecondsRealtime(5f);
        this.rights = true;
#elif UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.CoarseLocation)) {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.CoarseLocation);
        }

        // First, check if user has location service enabled
        if (!UnityEngine.Input.location.isEnabledByUser) {
            // TODO Failure
            Debug.LogFormat("Android and Location not enabled");
            yield break;
        }else{
        this.rights = true;
        }

#elif UNITY_IOS
        if (!UnityEngine.Input.location.isEnabledByUser) {
            // TODO Failure
            Debug.LogFormat("IOS and Location not enabled");
            yield break;
        }
        else{
        this.rights = true;
        }
#endif
        // Start service before querying location
        startLocationService();

        

        // Wait until service initializes
        int maxWait = 15;
#if UNITY_EDITOR
       while (UnityEngine.Input.location.status == LocationServiceStatus.Stopped && maxWait > 0)
#elif UNITY_ANDROID
       while (UnityEngine.Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
#endif

        {
            yield return new WaitForSecondsRealtime(1);
            maxWait--;
        }

        // Service didn't initialize in 15 seconds
        if (maxWait < 1)
        {
            // TODO Failure
            this.running = false;
            Debug.LogFormat("Timed out");
            yield break;
        }

        // Connection has failed
        if (UnityEngine.Input.location.status != LocationServiceStatus.Running)
        {
            // TODO Failure
            Debug.LogFormat("Unable to determine device location. Failed with status {0}", UnityEngine.Input.location.status);
            yield break;
        }
        else
        {
            Debug.LogFormat("Location service live. status {0}", UnityEngine.Input.location.status);
            // Access granted and location value could be retrieved
            Debug.LogFormat("Location: "
                + UnityEngine.Input.location.lastData.latitude + " "
                + UnityEngine.Input.location.lastData.longitude + " "
                + UnityEngine.Input.location.lastData.altitude + " "
                + UnityEngine.Input.location.lastData.horizontalAccuracy + " "
                + UnityEngine.Input.location.lastData.timestamp);
            // TODO success do something with location
        }


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
        DBConnector.Instance.GetConnection().Insert(getCurrentWegpunkt());
        count=DBConnector.Instance.GetConnection().Query<Wegpunkt>("SELECT * FROM Wegpunkt where beg_id=?", AppState.SelectedBegehung.ToString()).Count;
    }


    /// <summary>
    /// neuen wegpunkt anlegen
    /// </summary>
    private Wegpunkt getCurrentWegpunkt()
    {
        Wegpunkt punkt = new Wegpunkt();
        punkt.beg_id = AppState.SelectedBegehung;
        punkt.wegp_longitude = UnityEngine.Input.location.lastData.longitude;
        punkt.wegp_latitude = UnityEngine.Input.location.lastData.latitude;
        punkt.wegp_altitude = UnityEngine.Input.location.lastData.altitude;
        punkt.wegp_accuracy = UnityEngine.Input.location.lastData.horizontalAccuracy;
        punkt.wegp_POIType = -1;
        punkt.wegp_timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        punkt.wegp_lernstand = -1;
        return punkt;
    }


    /// <summary>
    /// Starts the Tracking Service Again
    /// </summary>
    public void restartTracking()
    {
#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.CoarseLocation))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.CoarseLocation);
        }

        // First, check if user has location service enabled
        if (!UnityEngine.Input.location.isEnabledByUser)
        {
            // TODO Failure
            Debug.LogFormat("Android and Location not enabled");
            this.rights = false;
        }
        else
        {
            this.rights = true;
        }
#endif
        if(this.running==false&&this.rights==true)
        startLocationService();
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
    public void resetBegehung()
    {
        DBConnector.Instance.GetConnection().Execute("Delete FROM Wegpunkt where beg_id=?",AppState.SelectedBegehung);
        count = 0;
        last = 0;
    }
}