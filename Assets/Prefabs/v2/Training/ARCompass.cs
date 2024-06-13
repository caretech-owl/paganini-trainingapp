using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Events;

public class ARCompass : MonoBehaviour
{
    // Main compass object
    [SerializeField] private SVGImage OrientationCompass;
    [SerializeField] private SVGImage OrientationConfirmation;
    [SerializeField] private SVGImage LoadingCompass;


    // Internal compass elements for visualising status
    [SerializeField] private GameObject CurveWarning;
    [SerializeField] private GameObject CurveError;
    [SerializeField] private GameObject CurveNormal;

    [SerializeField] private GameObject ArrowWarning;
    [SerializeField] private GameObject ArrowError;
    [SerializeField] private GameObject ArrowNormal;

    [SerializeField] private Color ErrorColor;
    private Color originalColor;
    private bool currentlyError = false;

    // Threshoulds for when the user is going not in the correct direction, 180+-threshould
    [Tooltip("Threshold for warning status when the user deviates from the correct direction by 180 degrees")]
    [SerializeField] private float Direction180WarningThreshold = 60; // Example value, adjust as needed
    [Tooltip("Threshold for error status when the user deviates from the correct direction by 180 degrees")]
    [SerializeField] private float Direction180ErrorThreshold = 30; // Example value, adjust as needed

    public float rotationSpeed = 4.0f;
    private Quaternion targetRotation;
    private bool isConfirmed = false; 

    private void Start()
    {
        isConfirmed = false;
        LoadingCompass?.gameObject.SetActive(true);
        OrientationCompass?.gameObject.SetActive(false);
        OrientationConfirmation?.gameObject.SetActive(false);

        originalColor = OrientationCompass.color;
    }

    private void Update()
    {
        // Smoothly rotate towards the target rotation
        OrientationCompass.transform.localRotation = Quaternion.Slerp(OrientationCompass.transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
    }



    public void SwitchModePanel(GameObject obj)
    {
        LoadingCompass?.gameObject.SetActive(false);
        OrientationCompass?.gameObject.SetActive(true);

        CurveWarning.SetActive(obj == CurveWarning);
        ArrowWarning.SetActive(obj == CurveWarning);

        CurveError.SetActive(obj == CurveError);
        ArrowError.SetActive(obj == CurveError);

        CurveNormal.SetActive(obj == CurveNormal);
        ArrowNormal.SetActive(obj == CurveNormal);
    }


    public void RenderOrientation(float orientationBearing)
    {
        if (!isConfirmed)
        {
            if (currentlyError)
                RenderCompassError(false);

            IdentifyStatus(orientationBearing);
            targetRotation = Quaternion.Euler(0, 0, orientationBearing);;
        }        
    }


    private void IdentifyStatus(float orientationBearing)
    {
        float deviation = Mathf.Abs(Mathf.DeltaAngle(orientationBearing, 180)); // Calculate deviation from 180 degrees

        if (deviation <= Direction180ErrorThreshold)
        {
            SwitchModePanel(CurveError);
        }
        else if (deviation <= Direction180WarningThreshold)
        {
            SwitchModePanel(CurveWarning);
        }
        else
        {
            SwitchModePanel(CurveNormal);
        }
    }


    public void RenderConfirmation()
    {
        OrientationCompass?.gameObject.SetActive(false);
        OrientationConfirmation?.gameObject.SetActive(true);

        isConfirmed = true;
    }

    public void RenderCompassError(bool error)
    {
        var compass = OrientationCompass;
        if (LoadingCompass.gameObject.activeSelf)
        {
            compass = LoadingCompass;
        }

        compass.color = error ? ErrorColor : originalColor;

        currentlyError = error;
    }

}
