using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using static DirectionIcon;
using static PaganiniRestAPI;

public class WayChallengeDirectionInstruction : MonoBehaviour
{
    [Header("Main states")]
    public GameObject IntroPanel;  
    public GameObject TaskPanel;

    [Header("UI Components")]
    public PhotoSlideShow SlideShow;
    public InstructionCard Card;
    public AudioInstruction AuralInstruction;

    [Header("Events")]
    public UnityEvent OnTaskCompleted;
    public event EventHandler<EventArgs<AdaptationTaskArgs>> OnAdaptationEventChange;

    private Pathpoint POI;
    private AdaptationTaskArgs CurrentAdaptationTask;

    // Start is called before the first frame update
    void Start()
    {
        Card.SetCardTransitionTimer(AppState.Training.DecisionPointConfirmationDelay);
        Card.OnCardTransition.AddListener(Card_OnCardTransition);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadInstruction(Way way, Pathpoint pathtpoint, bool skipIntro = false)
    {
        CurrentAdaptationTask = new();
        CurrentAdaptationTask.IsTaskStart = true;
        

        if (pathtpoint.CurrentInstructionMode.IsAtPOINewToUser && !skipIntro)
        {
            ShowTask(false);
            AuralInstruction.PlayChallengeDirectionIntro();

            CurrentAdaptationTask.AdaptationIntroShown = true;
            SendAdaptationTaskData(); // partially send that we have shown the intro
        }
        else
        {
            CurrentAdaptationTask.AdaptationIntroShown = false;
            SendAdaptationTaskData();
        }

        LoadTask(way, pathtpoint);
    }

    public void LoadTask(Way way, Pathpoint pathtpoint)
    {
        POI = pathtpoint;

        // Load the picture 
        SlideShow.LoadSlideShow(pathtpoint.Photos);       

        // Prepare card
        Card.FillInstruction(null, null);
        Card.InstructionIconPOI.RenderChallenge();
        Card.RenderInstruction();

    }

    public void ShowTask(bool showTask)
    {
        IntroPanel.SetActive(!showTask);
        TaskPanel.SetActive(showTask);

        if (showTask)
        {
            AuralInstruction.CancelCurrentPlayback();
            AuralInstruction.PlayChallengeDirectionTask();
        }
    }


    public void InformModeCancelled()
    {
        CurrentAdaptationTask.AdaptationTaskAccepted = false; // update intro
        SendAdaptationTaskData();

        CurrentAdaptationTask.IsTaskStart = false;
        CurrentAdaptationTask.AdaptationTaskCompleted = false;
        SendAdaptationTaskData();

        CurrentAdaptationTask = null;
    }

    public void InformTaskSuccessful()
    {
        CurrentAdaptationTask.IsTaskStart = false;
        CurrentAdaptationTask.AdaptationTaskCompleted = true;
        CurrentAdaptationTask.AdaptationTaskCorrect = true;
        SendAdaptationTaskData();
    }

    public void TaskAcceptedByUser()
    {
        CurrentAdaptationTask.AdaptationTaskAccepted = true; // update intro
        SendAdaptationTaskData();

        ShowTask(true);
    }



    private void SendAdaptationTaskData()
    {
        CurrentAdaptationTask.AdaptationSupportMode = PathpointPIM.SupportMode.Challenge;
        OnAdaptationEventChange?.Invoke(this, new EventArgs<AdaptationTaskArgs>(CurrentAdaptationTask));
    }

    private void OnDestroy()
    {
        Card.OnCardTransition.RemoveListener(Card_OnCardTransition);
        //PhotoSlideShow. cleanup view
    }


    // Handlers
    private void Card_OnCardTransition()
    {
        OnTaskCompleted.Invoke();
    }


}
