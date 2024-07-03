using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static PathpointPIM;
using static PaganiniRestAPI;

public class WayDecisionModes : MonoBehaviour
{
    [Header("Instruction modes")]
    public WayDecisionInstruction NormalMode;
    public WayTriviaInstruction TriviaMode;
    public WayChallengeDirectionInstruction ChallengeDirectionMode;

    [Header("Utils")]
    public AudioInstruction AuralInstruction;
    public bool EnableAdaptiveModes = true;
    public bool EnableImprovedDesign = true;

    public UnityEvent OnTaskCompleted;

    private Way CurrentWay;
    private Pathpoint CurrentPOI;
    private SupportMode CurrentMode;

    private List<SupportMode> supportedModes = new List<SupportMode> { SupportMode.Instruction, SupportMode.Trivia, SupportMode.Challenge, SupportMode.Mute };

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadInstruction(Way way, Pathpoint pathpoint)
    {
        CurrentWay = way;
        CurrentPOI = pathpoint;
        SupportMode? supportMode = null;

        if (pathpoint.CurrentInstructionMode != null)
        {
            supportMode = pathpoint.CurrentInstructionMode.AtPOIMode;
        }

        if (EnableImprovedDesign && POIWatcher.Instance.GetPreviousState() == POIWatcher.POIState.OffTrack)
        {
            HandleBackOnTrackSupport(supportMode);
        }
        else
        {
            LoadInstructionByMode(way, pathpoint, supportMode);
        }
    }

    private void HandleBackOnTrackSupport(SupportMode? supportMode)
    {
        if (!EnableAdaptiveModes || supportMode == null || supportMode == SupportMode.Instruction ||
            supportedModes.IndexOf((SupportMode)supportMode) < 0)
        {
            AuralInstruction.PlayBackOnTrackConfusion();
            LoadInstructionByMode(CurrentWay, CurrentPOI, supportMode);
        }
        else 
        {
            AuralInstruction.PlayBackOnTrackDowngradeMode((SupportMode)supportMode);
            DownGradeInstructionMode(supportMode);
        }        
        
    }

    public void LoadInstructionConfirmation()
    {
        // we load the confirmation of the normal mode
        // if not active, we activate it
        //if (!NormalMode.gameObject.activeSelf)
        //{
        //    LoadInstructionMode(CurrentWay, CurrentPOI);
        //    NormalMode.HideOverlayOptions();
        //}

        // we activate only the normal mode running in the background
        if (CurrentMode != SupportMode.Instruction)
        {
            RenderOnly(NormalMode.gameObject);
        }

        NormalMode.LoadInstructionConfirmation();
        AuralInstruction.PlayNavInstructionCorrect();
    }

    private void LoadInstructionByMode(Way way, Pathpoint pathpoint, SupportMode? mode, bool skipIntro = false)
    {
        if (!EnableAdaptiveModes)
        {
            mode = null;
        }

        if (mode == SupportMode.Trivia)
        {
            CurrentMode = SupportMode.Trivia;
            LoadTriviaMode(way, pathpoint, skipIntro);
            // we run normal mode in the background, to pre-load video we
            // use for confirmation            
            LoadBackgrondNormalMode(way, pathpoint);
        }
        else if (mode == SupportMode.Challenge)
        {
            CurrentMode = SupportMode.Challenge;
            LoadChallengeDirectionMode(way, pathpoint, skipIntro);
            // we run normal mode in the background, to pre-load video we
            // use for confirmation            
            LoadBackgrondNormalMode(way, pathpoint);
        }
        else // Instruction
        {
            CurrentMode = SupportMode.Instruction;
            LoadInstructionMode(way, pathpoint);
            AuralInstruction.PlayNavInstruction(pathpoint);
        }
    }

    public void UserCancelledMode()
    {        
        Debug.Log("User cancelled mode.");

        AuralInstruction.CancelCurrentPlayback();
        AuralInstruction.PlayRefuseTask();

        //downgrade one
        DownGradeInstructionMode(CurrentMode);


    }

    private void DownGradeInstructionMode(SupportMode? supportMode)
    {
        if (supportMode != null && supportMode != SupportMode.Instruction)
        {
            var index = supportedModes.IndexOf((SupportMode)supportMode);
            supportMode = supportedModes[index - 1];
        }


        LoadInstructionByMode(CurrentWay, CurrentPOI, supportMode, true);
    }

    public void LoadBackgrondNormalMode(Way way, Pathpoint pathpoint)
    {
        NormalMode.gameObject.SetActive(true);
        NormalMode.LoadInstruction(way, pathpoint);
        NormalMode.OnTaskCompleted.AddListener(Instruction_OnTaskCompleted);
        NormalMode.HideOverlayOptions();
    }

    public void LoadInstructionMode(Way way, Pathpoint pathpoint)
    {
        CleanUpView();

        RenderOnly(NormalMode.gameObject);
        NormalMode.LoadInstruction(way, pathpoint);
        NormalMode.OnTaskCompleted.AddListener(Instruction_OnTaskCompleted);
    }

    public void LoadTriviaMode(Way way, Pathpoint pathpoint, bool skipIntro)
    {
        CleanUpView();

        RenderOnly(TriviaMode.gameObject);
        TriviaMode.LoadInstruction(way, pathpoint, skipIntro);
        // there is callback from a trivia yet
        //TriviaMode.OnTaskCompleted.AddListener(Instruction_OnTaskCompleted);
    }

    public void LoadChallengeDirectionMode(Way way, Pathpoint pathpoint, bool skipIntro)
    {
        CleanUpView();

        RenderOnly(ChallengeDirectionMode.gameObject);
        ChallengeDirectionMode.LoadInstruction(way, pathpoint, skipIntro);
        // there is no callback from a challenge
        //ChallengeDirectionMode.OnTaskCompleted.AddListener(Instruction_OnTaskCompleted);
    }

    public void CleanUpView()
    {
        NormalMode.OnTaskCompleted.RemoveListener(Instruction_OnTaskCompleted);
        //TriviaMode.OnTaskCompleted.RemoveListener(Instruction_OnTaskCompleted);
        //ChallengeDirectionMode.OnTaskCompleted.RemoveListener(Instruction_OnTaskCompleted);

        NormalMode.CleanUpView();
        TriviaMode.CleanUpView();

    }

    private void RenderOnly(GameObject panel)
    {
        NormalMode.gameObject.SetActive(NormalMode.gameObject == panel);
        TriviaMode.gameObject.SetActive(TriviaMode.gameObject == panel);
        ChallengeDirectionMode.gameObject.SetActive(ChallengeDirectionMode.gameObject == panel);
    }

    private void OnDestroy()
    {
        CleanUpView();
    }

    // Handlers
    private void Instruction_OnTaskCompleted()
    {
        OnTaskCompleted?.Invoke();
        Debug.Log("DecisionModes - Task Completed");
    }

}
