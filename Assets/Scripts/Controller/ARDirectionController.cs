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
using static LocationUtils;

public class ARDirectionController : MonoBehaviour
{

    RouteSharedData SharedData;
    LocationUtils RouteValidation;

    private POIWatcher POIWatch;

    private Pathpoint CurrentPathpoint;
    private double? CurrentAheadBearing;
    private int CurrentClosestPathpointIndex;

    public PlaceAtLocations LocationPlacement;
    public PlaceAtLocation BeaconPlacement;

    public GameObject CloseViewPanel;
    public ARCompass OrientationCompass;

    public TMPro.TMP_Text LogText;

    public GameObject SceneUI;

    public enum ARSupportMode
    {
        Confusion,
        Deviation,
        OffTrack
    }

    private ARSupportMode SupportMode;
    private AudioInstruction AuralInstruction;

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

        if (!Input.compass.enabled)
        {
            Debug.LogError("Compass not available");
        }

        // We borrow the existing Audio component from the background scene
        var obj = GameObject.Find("AudioInstruction");
        if (obj != null)
        {
            AuralInstruction = obj.GetComponent<AudioInstruction>();
        }
        
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

        OrientationCompass.RenderCompassError(true);
        AuralInstruction?.PlayCompassGPSIssues();

    }

    private void POIWatch_OnArrived(object sender, POIArgs e)
    {

    }

    private void POIWatch_OnAlongTrack(object sender, ValidationArgs e)
    {
        // Should switch to normal view
        //CloseViewPanel.SetActive(true);
        ClearARMarkers();

        // The confirmation is currently the same, this might change
        // with the refinement of the menu. This is a placeholder
        if (SupportMode == ARSupportMode.Confusion)
        {
            RenderConfirmation();
        }
        else if (SupportMode == ARSupportMode.OffTrack)
        {
            RenderConfirmation();
        }
        else
        {
            RenderConfirmation();
        }
        
    }

    private void POIWatch_OnOffTrack(object sender, ValidationArgs e)
    {
        //ClearARMarkers();
        //RenderPOIBeacon();

        // In this case, we close, since the main nav app takes care of off-track messages
        CloseAR();
    }

    private void POIWatch_OnLeftPOI(object sender, POIArgs e)
    {
        // Should switch to normal view
        //CloseViewPanel.SetActive(true);
    }

    private void POIWatch_OnEnteredPOI(object sender, POIArgs e)
    {
        // Should switch to showing directions
        ClearARMarkers();
        //RenderPOIDirections();

        //Log("RenderPOIDirections");

        // This happens if we are off-track and get back on track, and directly on a POI
        // in this case, we close the support and go back to the main nav

        RenderConfirmation();
    }

    private void POIWatch_OnUserLocationChanged(object sender, LocationChangedArgs e)
    {
        if (SupportMode == ARSupportMode.Confusion)
        {
            CalulateAheadOrientation(e.Point, e.SegmentInfo);
        }
        else if (SupportMode == ARSupportMode.OffTrack)
        {
            CalulateBringToPOIOrientation(e.Point, e.SegmentInfo);
        }
        else // We just use the same stuff as in nav, but we could tailor it
        {
            CalulateBringToClosestPointOrientation(e.Point, e.SegmentInfo);
        }

        Debug.Log("AR: POIWatch_OnUserLocationChanged");

    }

    /* Public functions */

    public void StartTraining()
    {
        //hack, remove video from view, due to some overlaying problem
        //TODO: Find a more elegant solution
        var obj = GameObject.Find("VideoInstruction");
        obj?.SetActive(false);

        CurrentClosestPathpointIndex = -1;

        Debug.Log($"AR StartTraining: POIWatcher State '{POIWatch.GetCurrentState()}'");

        ClearARMarkers();

        if (POIWatch.GetCurrentState() == POIWatcher.POIState.OffTrack)
        {
            // wrong decision or deviation?
            //RenderPOIBeacon();
            SupportMode = ARSupportMode.OffTrack;

            // it's a deviation, then it should ahve different treatment
            if (POIWatch.GetNavigationIssue() == NavigationIssue.Deviation)
            {
                SupportMode = ARSupportMode.Deviation;
            }


        }
        else if (POIWatch.GetCurrentState() == POIWatcher.POIState.OnPOI)
        {
            //RenderPOIDirections();
            SupportMode = ARSupportMode.Confusion;
            CurrentPathpoint = POIWatch.GetCurrentTargetPathpoint();
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

    public void CalulateAheadOrientation(Pathpoint pathpoint, SegmentDistanceAndBearing segmentInfo)
    {
        //var segmentInfo = RouteValidation.CalculateMinDistanceAndBearing(pathpoint);

        // We target the a pathpoint in the road ahead
        int orientationTargetIndex = Math.Min(SharedData.PathpointList.Count, segmentInfo.ClosestSegmentIndex + 3);

        // calculate bearing to target pathpoint
        CurrentAheadBearing = RouteValidation.CalculateBearing(pathpoint, SharedData.PathpointList[orientationTargetIndex]);      
        
    }

    // We try to bring the user to the POI, where they made the decision
    public void CalulateBringToPOIOrientation(Pathpoint pathpoint, SegmentDistanceAndBearing segmentInfo)
    {
        CurrentAheadBearing = RouteValidation.CalculateBearing(pathpoint, POIWatch.GetCurrentTargetPathpoint());
    }

    public void CalulateBringToClosestPointOrientation(Pathpoint pathpoint, SegmentDistanceAndBearing segmentInfo)
    {
        var closestIndex = segmentInfo.ClosestSegmentIndex;
        if (CurrentClosestPathpointIndex < 0 || Math.Abs(closestIndex - CurrentClosestPathpointIndex) >= 2)
        {
            CurrentClosestPathpointIndex = closestIndex;
        }
        else
        {
            closestIndex = CurrentClosestPathpointIndex;
        }

        CurrentAheadBearing = RouteValidation.CalculateBearing(pathpoint, SharedData.PathpointList[closestIndex]);
    }



    private double simulatedHeading = 0;

    public void RenderOrientation()
    {
        double magneticHeading = 0;

        if (Input.compass.enabled)
        {
            magneticHeading = Input.compass.magneticHeading;
        }
        else
        {
            //Debug.LogError("Compass is not available");
            // Simulate movement by incrementing the heading
            // simulatedHeading += 5.0; // Increment by 5 degrees each time (adjust as needed)
            // magneticHeading = simulatedHeading;

            if (UnityEngine.Random.Range(0, 50) < 3)
            {
                magneticHeading = (double)CurrentAheadBearing;
            }
            else
            {
                magneticHeading += (double)CurrentAheadBearing + UnityEngine.Random.Range(-5, 5);
            }

            
        }

        // Adjust orientation based on magnetic heading
        double adjustedOrientationBearing = ((double)CurrentAheadBearing - magneticHeading) % 360;

        // Convert the bearing to Unity's coordinate system
        double unityOrientationBearing = -adjustedOrientationBearing;

        // Rotate the arrow based on the adjusted orientation bearing

        //var targetRotation = Quaternion.Euler(0,  0, (float)unityOrientationBearing);

        // Smoothly rotate the arrow towards the target rotation using lerp
        //OrientationCompass.transform.rotation = targetRotation; //Quaternion.Lerp(OrientationCompass.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        OrientationCompass.RenderOrientation((float)unityOrientationBearing);

        Log($"Rotation: {unityOrientationBearing.ToString("F3")} CurrentAheadBearing: {((double)CurrentAheadBearing).ToString("F3")}");
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

        // hack, there is a problem with video layer
        GameObject[] objs = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "VideoInstruction").ToArray();

        // Activate the first found object
        if (objs.Length > 0)
        {
            objs[0].SetActive(true);
        }

        SceneManager.UnloadSceneAsync(SceneSwitcher.ARDirectionScene);
    }

    public void TriggerPause()
    {
        GameObject.Find("RouteTrainingController").SendMessage("PasueAndBack");
    }

    public void TriggerHelp()
    {
        GameObject.Find("RouteTrainingController").SendMessage("HelpAndBack");
    }


    public void SendToBackground()
    {
        // Set the camera's depth to render behind the background scene
        //arCamera.depth = 0;

        // Send a message to BackgroundScene to notify it that ARDirectionScene is in the background

        // Hide canvas of AROrigin and the other ones as well
        SceneUI.SetActive(false);

    }


    public void BringToFroground()
    {
        // Show canvas of AROrigin and the other ones as well
        SceneUI.SetActive(true);
    }

    // private

    private void RenderConfirmation()
    {
        OrientationCompass.RenderConfirmation();
        float delaySec = 5f; // default delay after confirmation

        // we time it with the confirmation of the POI, in confusion mode
        if (CurrentPathpoint != null)
        {
            delaySec = CurrentPathpoint.POIType == Pathpoint.POIsType.Reassurance ?
                AppState.Training.SafetyPointConfirmationDelay :
                AppState.Training.DecisionPointConfirmationDelay;
        }

        StartCoroutine(CloseSceneAfterDelay(delaySec)); // Close the scene after 3 seconds
    }

    private IEnumerator CloseSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseAR();
    }

    private void Log(string msg)
    {
        LogText.text = msg;
    }



}
