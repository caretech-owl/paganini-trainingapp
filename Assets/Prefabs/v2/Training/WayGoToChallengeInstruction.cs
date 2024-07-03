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

    public UnityEvent OnTaskCompleted;



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
        // init view
        if (pathtpoint.CurrentInstructionMode.IsAtPOINewToUser && !skipIntro)
        {            
            AuralInstruction.PlayChallengeGoToIntro();
            ShowTask(false);
        }
        else
        {            
            ShowTask(true);
        }

    }

    public void ShowTask(bool show)
    {
        IntroPanel.SetActive(!show);

        if (show)
        {
            AuralInstruction.CancelCurrentPlayback();
            OnTaskCompleted.Invoke();
        }
    }

}
