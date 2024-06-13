using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StudySettings : MonoBehaviour
{
    [Header("Advanced settings Overlay")]
    public GameObject HiddenOverlay;
    public Toggle BaselineDesignToggle;
    public Toggle ImprovedDesignToggle;
    public Toggle AdaptiveDesignToggle;
    public TMPro.TMP_Text DesignName;


    [Header("Improved Design Elements")]
    public Button ARIconButton;
    public Button VideoTriggerButton;
    public GameObject OfftrackImproved;
    public GameObject OfftrackBaseline;
    public WayGoToInstruction WayGoTo;
    public WayGoToModes GotoModes;
    public WayDecisionInstruction WayDecision;
    public WayDecisionModes DecisionModes;
    public WaySafetyInstruction WaySafety;


    private void Start()
    {
        ConfigureUX(AppState.Training.ActiveDesignMode);
        HiddenOverlay?.SetActive(false);
    }

    public void RenderHiddenOptions()
    {
        HiddenOverlay.SetActive(true);

        BaselineDesignToggle.SetIsOnWithoutNotify(AppState.Training.ActiveDesignMode == AppState.Training.DesignMode.Baseline);        
        ImprovedDesignToggle.SetIsOnWithoutNotify(AppState.Training.ActiveDesignMode == AppState.Training.DesignMode.Improved);
        AdaptiveDesignToggle.SetIsOnWithoutNotify(AppState.Training.ActiveDesignMode == AppState.Training.DesignMode.Adaptive);

        renderDesignName();

        Debug.Log("RenderHiddenOptions: "+ HiddenOverlay.activeSelf);

        if (BaselineDesignToggle.isOn)
        {            
            BaselineDesignToggle.Select();
        }
        else if (ImprovedDesignToggle.isOn)
        {            
            ImprovedDesignToggle.Select();
        }
        else
        {
            AdaptiveDesignToggle.Select();
        }



    }

    public void SwitchDesign(bool active)
    {
        // as it is associated to three toggles, only the active one triggers it
        if (!active) return;

        if (BaselineDesignToggle.isOn)
        {
            AppState.Training.ActiveDesignMode = AppState.Training.DesignMode.Baseline;
        }
        else if (ImprovedDesignToggle.isOn)
        {
            AppState.Training.ActiveDesignMode = AppState.Training.DesignMode.Improved;
        }
        else
        {
            AppState.Training.ActiveDesignMode = AppState.Training.DesignMode.Adaptive;
        }
        

        ConfigureUX(AppState.Training.ActiveDesignMode);
        renderDesignName();

        Debug.Log("ActiveDesignMode: " + AppState.Training.ActiveDesignMode);
    }

    private void ConfigureUX(AppState.Training.DesignMode designMode)
    {
        // We remove functionality from the baseline, which is available in improved and adaptive
        ARIconButton.gameObject.SetActive(designMode != AppState.Training.DesignMode.Baseline);
        VideoTriggerButton.gameObject.SetActive(designMode != AppState.Training.DesignMode.Baseline);

        OfftrackImproved.SetActive(designMode != AppState.Training.DesignMode.Baseline);
        OfftrackBaseline.SetActive(designMode == AppState.Training.DesignMode.Baseline);

        WayGoTo.EnableHideSupport = designMode != AppState.Training.DesignMode.Baseline;       
        WayDecision.EnablePicturePoVConfirmation = designMode != AppState.Training.DesignMode.Baseline;        
        
        WaySafety.ShowOverlayOptions(designMode != AppState.Training.DesignMode.Baseline);
        WayDecision.ShowOverlayOptions(designMode != AppState.Training.DesignMode.Baseline);

        DecisionModes.EnableImprovedDesign = designMode != AppState.Training.DesignMode.Baseline;

        // only for adaptive design:
        GotoModes.EnableAdaptiveModes = designMode == AppState.Training.DesignMode.Adaptive;
        DecisionModes.EnableAdaptiveModes = designMode == AppState.Training.DesignMode.Adaptive;
        

    }

    private void renderDesignName()
    {
        var name = AppState.Training.ActiveDesignMode.ToString();
        DesignName.text = name;
    }


}
