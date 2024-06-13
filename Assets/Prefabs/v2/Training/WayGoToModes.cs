using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static InstructionMode;

public class WayGoToModes : MonoBehaviour
{
    [Header("Instruction modes")]
    public WayGoToInstruction NormalMode;
    public WayGoToTriviaInstruction TriviaMode;
    //public WayChallengeDirectionInstruction ChallengeDirectionMode;

    [Header("Utils")]
    public AudioInstruction AuralInstruction;
    public bool EnableAdaptiveModes = true;

    public UnityEvent OnTaskCompleted;

    private Way CurrentWay;
    private Pathpoint CurrentPOI;
    private SupportMode CurrentMode;


    private List<SupportMode> supportedModes = new List<SupportMode>{ SupportMode.Instruction, SupportMode.TriviaGoto, SupportMode.ChallengeGoto };

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
            supportMode = pathpoint.CurrentInstructionMode.Mode;
        }

        LoadInstructionByMode(way, pathpoint, supportMode);
    }

    public void LoadInstructionConfirmation()
    {
        // we load the confirmation of the normal mode
        // if not active, we activate it
        // This can happen if the user did not complete the trivia task in time
        // before reaching the destination
        var wasTooLate = TriviaMode.gameObject.activeSelf && !TriviaMode.IsTaskDone();

        if (!NormalMode.gameObject.activeSelf)
        {
            LoadInstructionMode(CurrentWay, CurrentPOI);
            //NormalMode.HideOverlayOptions();
        }
        NormalMode.LoadInstructionConfirmation();

        // if we did not finish the trivia, then we provide a feedback to the user
        // Time is out, we already arrived. Next time.
        if (wasTooLate)
        {
            AuralInstruction.PlayTriviaGoToTooLate();
        }
    }

    public void CancelHideSupport()
    {
        NormalMode.CancelHideSupport();
    }

    private void LoadInstructionByMode(Way way, Pathpoint pathpoint, SupportMode? mode, bool skipIntro = false)
    {
        if (!EnableAdaptiveModes)
        {
            mode = null;
        }

        if (mode == SupportMode.TriviaGoto)
        {
            CurrentMode = SupportMode.TriviaGoto;
            LoadTriviaMode(way, pathpoint, skipIntro);
        }
        else if (mode == SupportMode.ChallengeGoto)
        {
            CurrentMode = SupportMode.ChallengeGoto;
            LoadChallengeMode(way, pathpoint, skipIntro);
        }
        else // Instruction
        {
            CurrentMode = SupportMode.Instruction;
            LoadInstructionMode(way, pathpoint);
            AuralInstruction.PlayGotoInstruction(pathpoint);
        }
    }

    public void UserCancelledMode()
    {        
        Debug.Log("User cancelled mode.");

        AuralInstruction.CancelCurrentPlayback();
        AuralInstruction.PlayRefuseTask();

        //downgrade one
        var index = supportedModes.IndexOf(CurrentMode);
        CurrentMode = supportedModes[index-1];

        LoadInstructionByMode(CurrentWay, CurrentPOI, CurrentMode, true);
        
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

        // select an element from POIList, between CurrentPOIIndex and Count-1 (including)
        int currentIndex = POIWatcher.Instance.CurrentPOIIndex;
        List<Pathpoint> poiList = RouteSharedData.Instance.POIList;
        var wrongOptionIndex = TriviaMode.SelectRandomAlternative(currentIndex, poiList);
        var wrongPathpoint =RouteSharedData.Instance.PreparePOIData(wrongOptionIndex);

        TriviaMode.LoadInstruction(way, pathpoint, wrongPathpoint, skipIntro);
        TriviaMode.OnTaskCompleted.AddListener(Instruction_OnTriviaCompleted);
    }

    public void LoadChallengeMode(Way way, Pathpoint pathpoint, bool skipIntro)
    {
        CleanUpView();

        //RenderOnly(ChallengeDirectionMode.gameObject);
        //ChallengeDirectionMode.LoadInstruction(way, pathpoint, skipIntro);
        //ChallengeDirectionMode.OnTaskCompleted.AddListener(Instruction_OnTaskCompleted);
    }

    public void CleanUpView()
    {
        NormalMode.OnTaskCompleted.RemoveListener(Instruction_OnTaskCompleted);
        TriviaMode.OnTaskCompleted.RemoveListener(Instruction_OnTriviaCompleted);
        //ChallengeDirectionMode.OnTaskCompleted.RemoveListener(Instruction_OnTaskCompleted);

        NormalMode.CleanUpView();
        TriviaMode.CleanupView();

    }

    private void RenderOnly(GameObject panel)
    {
        NormalMode.gameObject.SetActive(NormalMode.gameObject == panel);
        TriviaMode.gameObject.SetActive(TriviaMode.gameObject == panel);
        //ChallengeMode.gameObject.SetActive(ChallengMode.gameObject == panel);
    }

    private void OnDestroy()
    {
        CleanUpView();
    }

    // Handlers
    private void Instruction_OnTaskCompleted()
    {
        OnTaskCompleted?.Invoke();
    }

    // After the trivia, we should show the standard goto instruction
    private void Instruction_OnTriviaCompleted()
    {
        LoadInstructionMode(CurrentWay, CurrentPOI);
    }

}
