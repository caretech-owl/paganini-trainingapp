using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WayEndInstruction : MonoBehaviour
{
    [Header("Main panels")]
    public GameObject CompletedPanel;
    public GameObject CancelledPanel;

    public PhotoSlideShow SlideShow;
    public InstructionCard Card;
   
    private Pathpoint POI;

    public UnityEvent OnEndSuccessful;

    // Start is called before the first frame update
    void Start()
    {
        Card.OnCardTransition.AddListener(Card_OnCardTransition);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadCompleted(Way way, Pathpoint pathtpoint)
    {
        ShowPanel(CompletedPanel);

        POI = pathtpoint;

        SlideShow.LoadSlideShow(pathtpoint.Photos);

        Card.FillInstruction(null, null);
        Card.InstructionIconPOI.RenderIcon(pathtpoint, way);
        Card.RenderConfirmation();
    }

    public void LoadCancelled()
    {
        ShowPanel(CancelledPanel);
    }

    private void OnDestroy()
    {
        Card.OnCardTransition.RemoveListener(Card_OnCardTransition);
    }

    // Handlers
    private void Card_OnCardTransition()
    {
        OnEndSuccessful.Invoke();
    }


    private void ShowPanel(GameObject obj)
    {
        CompletedPanel.SetActive(CompletedPanel == obj);
        CancelledPanel.SetActive(CancelledPanel == obj);

    }


}
