using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static PaganiniRestAPI;
using static PathpointPIM;

public class WayGoToModes : MonoBehaviour
{
    [Header("Instruction modes")]
    public WayGoToInstruction NormalMode;
    public WayGoToTriviaInstruction TriviaMode;
    public WayGoToChallengeInstruction ChallengeMode;

    [Header("Utils")]
    public AudioInstruction AuralInstruction;
    public bool EnableAdaptiveModes = true;

    public UnityEvent OnTaskCompleted;

    private Way CurrentWay;
    private Pathpoint CurrentPOI;
    private SupportMode CurrentMode;

    private POIWatcher POIWatch;
    private RouteSharedData SharedData;


    private List<SupportMode> supportedModes = new List<SupportMode>{ SupportMode.Instruction, SupportMode.Trivia, SupportMode.Challenge };

    // Start is called before the first frame update
    void Start()
    {
        POIWatch = POIWatcher.Instance;
        SharedData = RouteSharedData.Instance;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadInstruction(Way way, Pathpoint targetPathpoint)
    {
        CurrentWay = way;
        CurrentPOI = targetPathpoint;
        SupportMode? supportMode = null;


        if (targetPathpoint.CurrentInstructionMode != null)
        {
            supportMode = targetPathpoint.CurrentInstructionMode.ToPOIMode;
        }

        if (EnableAdaptiveModes && POIWatch.GetPreviousState() == POIWatcher.POIState.OffTrack)
        {
            HandleBackOnTrackSupport(supportMode);
        }
        else
        {
            LoadInstructionByMode(way, targetPathpoint, supportMode);
        }

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

        if (CurrentMode == SupportMode.Challenge)
        {
            AuralInstruction.PlayChallengeGoToFeedbackOk();
        }
    }

    public void CancelHideSupport()
    {
        NormalMode.CancelHideSupport();
    }

    private void HandleBackOnTrackSupport(SupportMode? supportMode)
    {
        if (!EnableAdaptiveModes || supportMode == null || supportMode == SupportMode.Instruction ||
            supportedModes.IndexOf((SupportMode)supportMode) < 0)
        {
            LoadInstructionByMode(CurrentWay, CurrentPOI, supportMode);
        }
        else if (EnableAdaptiveModes && supportMode == SupportMode.Challenge)
        {
            AuralInstruction.PlayBackOnTrackDowngradeMode((SupportMode)supportMode);
            DownGradeInstructionMode();
        }

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
        }
        else if (mode == SupportMode.Challenge)
        {
            CurrentMode = SupportMode.Challenge;
            LoadChallengeMode(pathpoint, skipIntro);
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

        DownGradeInstructionMode();

    }

    private void DownGradeInstructionMode()
    {
        //var index = supportedModes.IndexOf(CurrentMode);
        //CurrentMode = supportedModes[index-1];

        // update: we downgrade to simple instruction, to avoid confusion

        CurrentMode = SupportMode.Instruction;

        LoadInstructionByMode(CurrentWay, CurrentPOI, CurrentMode, true);

        // let's temporarily save the downgraded mode, so we keep track of the user decision
        // in case we need to show the instruction again
        RememberDowngradedMode(CurrentPOI, CurrentMode);
    }

    private void RememberDowngradedMode(Pathpoint poi, SupportMode supportMode)
    {
        PathpointPIM mode = poi.CurrentInstructionMode;

        if (mode != null)
        {
            mode.ToPOIMode = supportMode;

            // if muted, we downgrade the atPOIMode as well
            if (mode.AtPOIMode == SupportMode.Mute)
            {
                mode.AtPOIMode = SupportMode.Challenge;
            }            
        }

        poi.CurrentInstructionMode = mode;


    }

    public void LoadInstructionMode(Way way, Pathpoint pathpoint, bool renderAsChallenge = false)
    {
        CleanUpView();

        RenderOnly(NormalMode.gameObject);
        NormalMode.LoadInstruction(way, pathpoint);
        NormalMode.OnTaskCompleted.AddListener(Instruction_OnTaskCompleted);


        NormalMode.RenderChallengeCard(renderAsChallenge);

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

    public void LoadChallengeMode(Pathpoint pathpoint, bool skipIntro)
    {        
        if (!skipIntro && pathpoint.CurrentInstructionMode != null && pathpoint.CurrentInstructionMode.IsToPOINewToUser)
        {
            CleanUpView();

            RenderOnly(ChallengeMode.gameObject);
            ChallengeMode.LoadInstruction(pathpoint);
            ChallengeMode.OnTaskCompleted.AddListener(LoadUpcomingNotMutedPOI);
        }
        else
        {
            LoadUpcomingNotMutedPOI();
        }
                
    }


    public void LoadUpcomingNotMutedPOI()
    {
        // let's get the upcoming POI that is not muted as out target for the Goto instruction
        var targetPOI = POIWatch.GetUpcomingNonMutedTarget();
        // let's prepare the data needed for the instruction
        targetPOI = SharedData.PreparePOIData(targetPOI);

        // play the instruction mode
        LoadInstructionMode(CurrentWay, targetPOI, renderAsChallenge: true);
        AuralInstruction.PlayGotoInstruction(targetPOI);
    }

    public void CleanUpView()
    {
        NormalMode.OnTaskCompleted.RemoveListener(Instruction_OnTaskCompleted);
        TriviaMode.OnTaskCompleted.RemoveListener(Instruction_OnTriviaCompleted);
        ChallengeMode.OnTaskCompleted.RemoveListener(LoadUpcomingNotMutedPOI);

        NormalMode.CleanUpView();
        TriviaMode.CleanupView();

    }

    private void RenderOnly(GameObject panel)
    {
        NormalMode.gameObject.SetActive(NormalMode.gameObject == panel);
        TriviaMode.gameObject.SetActive(TriviaMode.gameObject == panel);
        ChallengeMode.gameObject.SetActive(ChallengeMode.gameObject == panel);
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
