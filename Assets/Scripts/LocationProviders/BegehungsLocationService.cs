using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class BegehungsLocationService : MonoBehaviour
{
    /// <summary>
    /// Game Object to Display Geopositon Info in
    /// </summary>
    public GameObject LocationSection;

    public GameObject content;
    public GameObject GeoPosPrefab;

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
                }

            }
        }
    }


    private int id = 0;
    private double last = 0;
    private int interval = 5;
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
        addentry(id++, UnityEngine.Input.location.lastData.longitude, UnityEngine.Input.location.lastData.latitude, System.DateTimeOffset.UtcNow.ToUnixTimeSeconds());
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

   
    public void addentry(int id, Double lon, Double lat, long timestamp)
    {
        var neu = Instantiate(GeoPosPrefab);
        if (content != null)
        {
            var list = neu.GetComponentsInChildren<Text>();
            foreach (var df in list)
            {
                switch (df.name)
                {
                    case "id":
                        df.text =id.ToString();
                        break;
                    case "lon":
                        df.text =lon.ToString();
                        break;
                    case "lat":
                        df.text =lat.ToString();
                        break;
                    case "timestamp":
                        df.text =timestamp.ToString();
                        break;

                }
            }
            neu.transform.SetParent(content.transform);
        }
    }
}