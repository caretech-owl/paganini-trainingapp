using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPSDebug : MonoBehaviour
{

    public TMPro.TMP_Text LonText;
    public TMPro.TMP_Text LatText;
    public TMPro.TMP_Text AccText;
    public TMPro.TMP_Text TsText;
    public TMPro.TMP_Text LogText;

    public TMPro.TMP_Text POIDistance;
    public TMPro.TMP_Text SegDistance;
    public TMPro.TMP_Text SegBearing;
    public TMPro.TMP_Text UserHeading;
    public TMPro.TMP_Text SegmentHeading;

    public TMPro.TMP_Text PingText;

    private int pingNum = 0;

    // Start is called before the first frame update
    void Start()
    {
        pingNum = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayGPS(Pathpoint pathpoint)
    {
        LonText.text = pathpoint.Longitude.ToString();
        LatText.text = pathpoint.Latitude.ToString();
        AccText.text = pathpoint.Accuracy.ToString();
        TsText.text = pathpoint.Timestamp.ToString();
    }


    public void DisplayMetrics(double poiDistance, double segDistance, double segBearing, double userHeading, double segHeading)
    {
        POIDistance.text = poiDistance.ToString();
        SegDistance.text = segDistance.ToString();
        SegBearing.text = segBearing.ToString();

        UserHeading.text = userHeading.ToString();
        SegmentHeading.text = segHeading.ToString();
    }

    public void Log(string message)
    {
        LogText.text = message;
    }

    public void Ping()
    {
        pingNum++;
        PingText.text = pingNum.ToString();
    }

}
