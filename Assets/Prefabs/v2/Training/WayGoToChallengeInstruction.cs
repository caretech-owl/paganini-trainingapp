using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class WayGoToChallengeInstruction : MonoBehaviour
{
    [Header("Main states")]
    public GameObject IntroPanel;  
    public GameObject TaskPanel;

    [Header("UI Components")]
    public AudioInstruction AuralInstruction;

    [Header("Events")]
    public UnityEvent OnTaskCompleted;
    public event EventHandler<EventArgs<AdaptationTaskArgs>> OnAdaptationEventChange;


    private AdaptationTaskArgs CurrentAdaptationTask;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadInstruction(Pathpoint pathtpoint, bool skipIntro = false)
    {
        CurrentAdaptationTask = new ();
        CurrentAdaptationTask.IsTaskStart = true;
        CurrentAdaptationTask.IsAtPOIMode = false;

        // init view
        if (pathtpoint.CurrentInstructionMode.IsAtPOINewToUser && !skipIntro)
        {            
            AuralInstruction.PlayChallengeGoToIntro();
            ShowTask(false);

            CurrentAdaptationTask.AdaptationIntroShown = true;
            SendAdaptationTaskData(); // partially send that we have shown the intro

        }
        else
        {
            CurrentAdaptationTask.AdaptationIntroShown = false;
            SendAdaptationTaskData();

            ShowTask(true);            
        }

        
    }

    public void InformModeCancelled()
    {
        CurrentAdaptationTask.AdaptationTaskAccepted = false;  // update the intro      
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
        CurrentAdaptationTask.AdaptationTaskAccepted = true; // update the intro   
        SendAdaptationTaskData();

        ShowTask(true);       
    }

    // return whether the task was accepted
    public bool InformIfTaskNotAccepted()
    {
        if (CurrentAdaptationTask == null) return true;

        bool accepted = true;
        // are we still awaiting confirmation? that means we didn't send the start of the event
        if (CurrentAdaptationTask.AdaptationIntroShown == true &&
            CurrentAdaptationTask.AdaptationTaskAccepted == null)
        {
            SendAdaptationTaskData();

            CurrentAdaptationTask.IsTaskStart = false;
            CurrentAdaptationTask.AdaptationTaskCompleted = false;
            SendAdaptationTaskData();

            accepted = false;
        }

        return accepted;
    }

    public void ShowTask(bool show)
    {
        IntroPanel.SetActive(!show);

        if (show)
        {
            AuralInstruction.CancelCurrentPlayback();
            OnTaskCompleted?.Invoke();
        }
    }

    private void SendAdaptationTaskData()
    {
        CurrentAdaptationTask.AdaptationSupportMode = PathpointPIM.SupportMode.Challenge;
        OnAdaptationEventChange?.Invoke(this, new EventArgs<AdaptationTaskArgs>(CurrentAdaptationTask));
    }

}
