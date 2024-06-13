using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WayStartInstruction : MonoBehaviour
{  
    public PhotoSlideShow StartSlideShow;
    public InstructionCard Card;
   
    private Pathpoint POI;

    public UnityEvent OnRouteWalkStart;

    // Start is called before the first frame update
    void Start()
    {
        Card.OnCardTransition.AddListener(Card_OnCardTransition);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadStart(Way way, Pathpoint pathtpoint)
    {
        POI = pathtpoint;

        StartSlideShow.LoadSlideShow(pathtpoint.Photos);

        string subtitle = pathtpoint.Description;
        if (subtitle == null || subtitle.Trim() == "") {
            subtitle = way.Start;
            subtitle = char.ToUpper(subtitle[0]) + subtitle.Substring(1);
        }

        Card.FillInstruction(null, subtitle);
        Card.InstructionIconPOI.RenderIcon(pathtpoint, way);
        //Card.InstructionIconPOI.WayIconChoice.SetSelectedLandmark(Int32.Parse(way.StartType));
        Card.RenderInstruction();
    }

    public void EnableStartTraining()
    {
        Card.RenderConfirmation();
    }

    public void CleanUpView()
    {
        StartSlideShow.CleanUpView();
    }

    private void OnDestroy()
    {
        Card.OnCardTransition.RemoveListener(Card_OnCardTransition);
        CleanUpView();
    }

    // Handlers
    private void Card_OnCardTransition()
    {
        OnRouteWalkStart.Invoke();
    }


}
