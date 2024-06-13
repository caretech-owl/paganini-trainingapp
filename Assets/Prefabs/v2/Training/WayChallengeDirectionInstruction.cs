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


    public UnityEvent OnTaskCompleted;

    private Pathpoint POI;

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
        
        if (pathtpoint.CurrentInstructionMode.IsNewToUser && !skipIntro)
        {
            ShowTask(false);
            AuralInstruction.PlayChallengeDirectionIntro();
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
