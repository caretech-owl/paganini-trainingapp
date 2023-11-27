using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingProgressBar : MonoBehaviour
{
    [SerializeField] private Slider RouteSlider;
    [SerializeField] private LandmarkIcon RouteStartIcon;
    [SerializeField] private LandmarkIcon RouteEndIcon;


    private RouteSharedData SharedData;
    private POIWatcher POIWatch;
    private LocationUtils RouteValidation;

    // Start is called before the first frame update
    void Start()
    {
        SharedData = RouteSharedData.Instance;
        POIWatch = POIWatcher.Instance;

        POIWatch.OnEnteredPOI += POIWatch_OnEnteredPOI;
        POIWatch.OnUserLocationChanged += POIWatch_OnUserLocationChanged;

    }



    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        POIWatch.OnEnteredPOI -= POIWatch_OnEnteredPOI;
        POIWatch.OnUserLocationChanged -= POIWatch_OnUserLocationChanged;
    }

    public void Initialise() {
        RouteSlider.minValue = 1;
        RouteSlider.maxValue = 100;

        RouteStartIcon.SetSelectedLandmark(SharedData.CurrentWay.StartType);
        RouteEndIcon.SetSelectedLandmark(SharedData.CurrentWay.DestinationType);
    }

    // TODO: Detect walking status change to reflect in the walking figure


    /* Events */

    private void POIWatch_OnEnteredPOI(object sender, POIArgs e)
    {
        
    }

    private void POIWatch_OnUserLocationChanged(object sender, LocationChangedArgs e)
    {

        if (POIWatch.GetCurrentState() != POIWatcher.POIState.OffTrack) {
            RouteSlider.value = Mathf.Round(((float)e.SegmentInfo.ClosestSegmentIndex / (float)SharedData.PathpointList.Count) * 100);
        }
        
    }

}
