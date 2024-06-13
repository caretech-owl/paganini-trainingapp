using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TrackingStatus : MonoBehaviour
{

    public GameObject GPSPanelColor;    
    public TMPro.TMP_Text GPSInfoLabel;

    private Color GoodStatus = Color.green;
    private Color MediumStatus = Color.yellow;
    private Color PoorStatus = Color.red;
    private Color InactiveStatus = Color.gray;


    Color pastGPSColor;
    string pastGPSLabel;

    public enum RunningStatus
    {
        Active,
        Inactive,
        Error
    }


    private void Start()
    {
        GPSInfoLabel.text = "-";       
    }


    void Update()
    {
     
    }


    public void UpdateGPSStatus(RunningStatus status, float accuracy)
    {
        Color color = InactiveStatus;
        string label = Math.Round(accuracy) + " m";

        if (status == RunningStatus.Inactive)
        {
            color= InactiveStatus;
            label = "-";
        }
        else if (status == RunningStatus.Error)
        {
            color = PoorStatus;
            label = "-";
        }
        else if (accuracy < 8)
        {
            color = GoodStatus;
        }
        else if (accuracy < 15)
        {
            color = MediumStatus;
        }
        else
        {
            color = PoorStatus;
        }

        ApplyColorToBackground(GPSPanelColor, color);
        GPSInfoLabel.text = label;

        pastGPSLabel = label;
        pastGPSColor = color;
    }

    public void UpdateGPSError()
    {

    }

    private void ApplyColorToBackground(GameObject panel, Color color)
    {
        Image img = panel.GetComponent<Image>();
        if (img != null) {
            img.color = color;
        }        
    }
}


