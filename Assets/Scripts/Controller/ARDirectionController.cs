using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using UnityEngine.SceneManagement;
using ARLocation;
using UnityEditor;

public class ARDirectionController : MonoBehaviour
{

    RouteSharedData SharedData;
    LocationUtils RouteValidation;

    private POIWatcher POIWatch;

    private Pathpoint CurrentPathpoint;
    private double? CurrentAheadBearing;

    public PlaceAtLocations LocationPlacement;
    public PlaceAtLocation BeaconPlacement;

    public GameObject CloseViewPanel;
    public GameObject OrientationCompass;

    public TMPro.TMP_Text LogText;

    // Start is called before the first frame update
    void Start()
    {        
        POIWatch = POIWatcher.Instance;
        SharedData = RouteSharedData.Instance;
        RouteValidation = LocationUtils.Instance;

        POIWatch.OnEnteredPOI -= POIWatch_OnEnteredPOI;
        POIWatch.OnLeftPOI -= POIWatch_OnLeftPOI;
        POIWatch.OnOffTrack -= POIWatch_OnOffTrack;
        POIWatch.OnAlongTrack -= POIWatch_OnAlongTrack;
        POIWatch.OnArrived -= POIWatch_OnArrived;
        POIWatch.OnInvalidPathpoint -= POIWatch_OnInvalidPathpoint;
        POIWatch.OnUserLocationChanged -= POIWatch_OnUserLocationChanged;

        POIWatch.OnEnteredPOI += POIWatch_OnEnteredPOI;
        POIWatch.OnLeftPOI += POIWatch_OnLeftPOI;
        POIWatch.OnOffTrack += POIWatch_OnOffTrack;
        POIWatch.OnAlongTrack += POIWatch_OnAlongTrack;
        POIWatch.OnArrived += POIWatch_OnArrived;
        POIWatch.OnInvalidPathpoint += POIWatch_OnInvalidPathpoint;
        POIWatch.OnUserLocationChanged += POIWatch_OnUserLocationChanged;

        StartTraining();        
    }

    
    private void Update()
    {
        if (CurrentAheadBearing != null)
        {
            RenderOrientation();
        }
    }


    void OnDestroy()
    {
        POIWatch.OnEnteredPOI -= POIWatch_OnEnteredPOI;
        POIWatch.OnLeftPOI -= POIWatch_OnLeftPOI;
        POIWatch.OnOffTrack -= POIWatch_OnOffTrack;
        POIWatch.OnAlongTrack -= POIWatch_OnAlongTrack;
        POIWatch.OnArrived -= POIWatch_OnArrived;
        POIWatch.OnInvalidPathpoint -= POIWatch_OnInvalidPathpoint;
        POIWatch.OnUserLocationChanged -= POIWatch_OnUserLocationChanged;
    }

    /* Events */

    private void POIWatch_OnInvalidPathpoint(object sender, PathpointInvalidArgs e)
    {
        Debug.Log($"{POIWatch.CurrentPOIIndex} POIWatch_OnInvalidPathpoint Accuracy: {e.Accuracy} Speed: {e.Speed}");
    }

    private void POIWatch_OnArrived(object sender, POIArgs e)
    {

    }

    private void POIWatch_OnAlongTrack(object sender, ValidationArgs e)
    {
        // Should switch to normal view
        CloseViewPanel.SetActive(true);
        ClearARMarkers();
    }

    private void POIWatch_OnOffTrack(object sender, ValidationArgs e)
    {
        ClearARMarkers();
        RenderPOIBeacon();

        Log("RenderPOIBeacon");
    }

    private void POIWatch_OnLeftPOI(object sender, POIArgs e)
    {
        // Should switch to normal view
        CloseViewPanel.SetActive(true);
    }

    private void POIWatch_OnEnteredPOI(object sender, POIArgs e)
    {
        // Should switch to showing directions
        ClearARMarkers();
        RenderPOIDirections();

        Log("RenderPOIDirections");
    }

    private void POIWatch_OnUserLocationChanged(object sender, LocationChangedArgs e)
    {
        CalulateAheadOrientation(e.Point);
    }

    /* Public functions */

    public void StartTraining()
    {
        Debug.Log($"AR StartTraining: POIWatcher State '{POIWatch.GetCurrentState()}'");

        if (POIWatch.GetCurrentState() == POIWatcher.POIState.OffTrack)
        {
            // wrong decision or deviation?
            RenderPOIBeacon();

        }
        else if (POIWatch.GetCurrentState() == POIWatcher.POIState.OnPOI)
        {
            RenderPOIDirections();
        }
        else
        {
            Debug.Log($"AR StartTraining: We do not show instructions for state '{POIWatch.GetCurrentState()}'");
        }
    }

    public void RenderPOIDirections()
    {
        // Get the Index of the Current POI
        int POIIndex = POIWatch.GetPathpointIndexById(POIWatch.GetCurrentTargetPathpoint().Id);

        // Extract a sub list containing a window of the previous and next elements around SharedData.PathpointList [POIIndex]
        List<Pathpoint> subpath = GetSubPathAroundPOI(SharedData.PathpointList, POIIndex, 10);

        Log($"RenderPOIDirections POIIndex: {POIIndex} Count: {subpath.Count}");

        // Render the subpath
        RenderBreadcrumbs(subpath);
    }

    public void RenderPOIBeacon()
    {
        BeaconPlacement.gameObject.SetActive(true);
        var point = POIWatch.GetCurrentTargetPathpoint();
        var loc = ConvertPathpointToLocation(point);
        BeaconPlacement.Location = loc;


        var rawDistance = 0; //BeaconPlacement.RawGpsDistance;
        Debug.Log($"AR Distance to Beacon: '{rawDistance}' | Lon: {loc.Longitude} Lat: {loc.Latitude} Acc: {loc.Altitude}");
        Log($"RenderPOIBeacon Lon: {loc.Longitude} Lat: {loc.Latitude} Altitute: {loc.Altitude}");
    }

    public void CalulateAheadOrientation(Pathpoint pathpoint)
    {
        var segmentInfo = RouteValidation.CalculateMinDistanceAndBearing(pathpoint);

        // We target the a pathpoint in the road ahead
        int orientationTargetIndex = Math.Min(SharedData.PathpointList.Count, segmentInfo.ClosestSegmentIndex + 3);

        // calculate bearing to target pathpoint
        CurrentAheadBearing = RouteValidation.CalculateBearing(pathpoint, SharedData.PathpointList[orientationTargetIndex]);      
        
    }

    const float rotationSpeed = 5f;
    public void RenderOrientation() {

        double magneticHeading = ARLocationProvider.Instance.CurrentHeading.magneticHeading;

        // Adjust orientation based on magnetic heading
        double adjustedOrientationBearing = (double)CurrentAheadBearing  - magneticHeading;

        // Convert the bearing to Unity's coordinate system
        double unityOrientationBearing = -adjustedOrientationBearing;

        // Rotate the arrow based on the adjusted orientation bearing

        var targetRotation = Quaternion.Euler(0, 0, (float)unityOrientationBearing);

        // Smoothly rotate the arrow towards the target rotation using lerp
        OrientationCompass.transform.rotation = targetRotation; //Quaternion.Lerp(OrientationCompass.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }



    public void RenderPath()
    {
        RenderBreadcrumbs(SharedData.POIList);
    }

    private void RenderBreadcrumbs(List<Pathpoint> list)
    {
        foreach (var point in list)
        {
            var loc = ConvertPathpointToLocation(point);
            LocationPlacement.AddLocation(loc);

            Debug.Log($"AR Breadcrum Lon: {loc.Longitude} Lat: {loc.Latitude} Acc: {loc.Altitude}");
        }      
    }

    private void ClearARMarkers()
    {
        BeaconPlacement.gameObject.SetActive(false);

        foreach (GameObject obj in LocationPlacement.Instances)
        {
            // Remove the game object from the scene
            Destroy(obj); 
        }
    }

    private List<Pathpoint> GetSubPathAroundPOI(List<Pathpoint> list, int centerIndex, int count)
    {
        int start = Math.Max(0, centerIndex - count / 2);
        int end = Math.Min(list.Count, start + count);
        start = Math.Max(0, end - count);

        return list.GetRange(start, end - start);
    }

    public Location ConvertPathpointToLocation(Pathpoint point)
    {
        var loc = new Location()
        {
            Latitude = point.Latitude,
            Longitude = point.Longitude,
            //Altitude = point.Altitude,
            Altitude = -10,
            AltitudeMode = AltitudeMode.GroundRelative
        };

        return loc;
    }



    public void CloseAR()
    {        
        SceneManager.UnloadSceneAsync(SceneSwitcher.ARDirectionScene);
    }


    private void Log(string msg)
    {
        LogText.text = msg;
    }


}
