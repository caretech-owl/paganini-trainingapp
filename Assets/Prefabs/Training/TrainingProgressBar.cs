using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingProgressBar : MonoBehaviour
{
    [SerializeField] private Slider RouteSlider;
    [SerializeField] private IconPOI RouteStartIcon;
    [SerializeField] private IconPOI RouteEndIcon;

    private RouteSharedData SharedData;
    private POIWatcher POIWatch;
    private LocationUtils RouteValidation;

    private int StartIndex;
    private int EndIndex;

    private double previousDistance;
    private int prevClosestIndex = 0;


    // Start is called before the first frame update
    void Start()
    {
        SharedData = RouteSharedData.Instance;
        POIWatch = POIWatcher.Instance;

        POIWatch.OnEnteredPOI += POIWatch_OnEnteredPOI;
        POIWatch.OnUserLocationChanged += POIWatch_OnUserLocationChanged;
        POIWatch.OnPOITargetChange += POIWatch_OnPOITargetChange;
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        POIWatch.OnEnteredPOI -= POIWatch_OnEnteredPOI;
        POIWatch.OnUserLocationChanged -= POIWatch_OnUserLocationChanged;
        POIWatch.OnPOITargetChange -= POIWatch_OnPOITargetChange;
    }

    public void Initialise(int poiIndexStart, int poiIndexEnd) {
        RouteSlider.minValue = 1;
        RouteSlider.maxValue = 100;

        StartIndex = poiIndexStart;
        EndIndex = poiIndexEnd;

        var pStart = RouteSharedData.Instance.PathpointList[StartIndex];
        var pEnd = RouteSharedData.Instance.PathpointList[EndIndex];

        RouteStartIcon.RenderIcon(pStart, RouteSharedData.Instance.CurrentWay);
        RouteEndIcon.RenderIcon(pEnd, RouteSharedData.Instance.CurrentWay);

        previousDistance = int.MaxValue;

        RouteSlider.value = CalculateProgress(prevClosestIndex);
    }

    public void MarkAsComplete()
    {
        RouteSlider.value = 100;
    }

    // TODO: Detect walking status change to reflect in the walking figure


    /* Events */

    private void POIWatch_OnEnteredPOI(object sender, POIArgs e)
    {
           
    }

    private void POIWatch_OnPOITargetChange(object sender, EventArgs<Pathpoint> e)
    {
        
    }

    private void POIWatch_OnUserLocationChanged(object sender, LocationChangedArgs e)
    {
        var trainingStatus = POIWatch.GetCurrentState();

        // When we just left, depending on the precision of the tracking, the bar might not be completed.
        // Thus, we just mark as complete the progress bar, not to confuse the user
        if (trainingStatus == POIWatcher.POIState.OnPOI && !POIWatch.IsCurrentPOIMuted())
        {
            MarkAsComplete();
        }
        // When we are off-track, we pause the progress bar        
        else if (trainingStatus != POIWatcher.POIState.OffTrack ) {

            //float progress = ((float)EndIndex - (float)StartIndex);
            //progress = ((float)e.SegmentInfo.ClosestSegmentIndex - (float)StartIndex) / progress;

            // We prevent the progress bar from going backwards, if we are
            // cutting the distance to the destination

            float currentProgress = CalculateProgress(e.SegmentInfo.ClosestSegmentIndex);

            if (RouteSlider.value < currentProgress ||    // we are making progress OR
                (e.SegmentStats !=null && e.SegmentStats.DistanceFromPOI > previousDistance)) // we are regressing and getting further away 
            {
                RouteSlider.value = currentProgress;

                previousDistance = e.SegmentStats != null ? e.SegmentStats.DistanceFromPOI : previousDistance;
            }

                
        }
        prevClosestIndex = e.SegmentInfo.ClosestSegmentIndex;

       // Debug.Log($"POIWatch_OnUserLocationChanged: Index [{e.SegmentInfo.ClosestSegmentIndex}]");

    }

    private float CalculateProgress(int index)
    {
        float progress = ((float)EndIndex - (float)StartIndex);
        progress = ((float)index - (float)StartIndex) / progress;        

        return Mathf.Round(progress * 100); ;
    }

}
