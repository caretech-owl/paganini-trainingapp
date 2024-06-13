using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaySafetyInstruction : MonoBehaviour
{  
    public PhotoSlideShow SlideShow;
    public InstructionCard Card;
    public GameObject OverlayOptions;
    public bool EnableOverlayOptions { get; set; }

    private Pathpoint POI;

    public UnityEvent OnTaskCompleted;

    // Start is called before the first frame update
    void Start()
    {
        Card.SetCardTransitionTimer(AppState.Training.SafetyPointConfirmationDelay);
        Card.OnCardTransition.AddListener(Card_OnCardTransition);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadInstruction(Pathpoint pathtpoint)
    {
        POI = pathtpoint;
        string name = pathtpoint.Description;
        string title = "Siehst du das";

        SlideShow.LoadSlideShow(pathtpoint.Photos);
        OverlayOptions?.SetActive(EnableOverlayOptions);

        // Render Card
        if (name == null || name.Trim() == "")
        {
            title = title + "?";
        }
        else
        {
            title += $": <color=blue>{name}</color>.";
        }

        Card.FillInstruction(title, "Geh weiter.");
        Card.InstructionIconPOI.RenderIcon(pathtpoint, null);
        Card.RenderInstruction();

    }

    public void ShowOverlayOptions(bool doShow)
    {
        EnableOverlayOptions = doShow;
        OverlayOptions?.SetActive(doShow);
    }

    public void LoadInstructionConfirmation()
    {
        Card.RenderConfirmation();
    }

    public void CleanUpView()
    {
        SlideShow.CleanUpView();
    }

    private void OnDestroy()
    {
        Card.OnCardTransition.RemoveListener(Card_OnCardTransition);
        CleanUpView();
    }

    // Handlers
    private void Card_OnCardTransition()
    {
        OnTaskCompleted.Invoke();
    }


}
