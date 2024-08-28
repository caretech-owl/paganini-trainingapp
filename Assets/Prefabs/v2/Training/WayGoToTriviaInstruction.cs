using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class WayGoToTriviaInstruction : MonoBehaviour
{
    [Header("Main states")]
    public GameObject IntroPanel;  
    public GameObject TaskPanel;

    [Header("UI Components")]
    //public PhotoSlideShow SlideShow;
    public InstructionCard Card;
    public AudioInstruction AuralInstruction;
    public Button EvaluateButton;


    [Header("Trivia Options")]
    public ToggleGroup OptionGroup;
    public ScrollRect OptionScrollView;
    public PictureToggle Option1;
    public PictureToggle Option2;

    //public GameObject VideoClose;

    [Header("Events")]
    public UnityEvent OnTaskCompleted;
    public event EventHandler<EventArgs<AdaptationTaskArgs>> OnAdaptationEventChange;

    private Pathpoint POI;

    private int correctDecisionIndex;
    private bool isTaskDone;
    private AdaptationTaskArgs CurrentAdaptationTask;

    // Start is called before the first frame update
    void Start()
    {
        Card.SetCardTransitionTimer(AppState.Training.GotoConfirmationDelay);
        Card.OnCardTransition.AddListener(Card_OnCardTransition);
        Option1.ToggleIndex = 1;
        Option2.ToggleIndex = 2;

        //DirectionOption1.GetComponent<Button>().onClick.AddListener(() => OnButtonOptionClicked(DirectionOption1));
        //DirectionOption2.GetComponent<Button>().onClick.AddListener(() => OnButtonOptionClicked(DirectionOption2));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadInstruction(Way way, Pathpoint okPathtpoint, Pathpoint wrongPathpoint, bool skipIntro = false)
    {
        CurrentAdaptationTask = new();
        CurrentAdaptationTask.IsTaskStart = true;
        CurrentAdaptationTask.IsAtPOIMode = false;

        // init view
        ShowTask(false);        

        if (okPathtpoint.CurrentInstructionMode.IsAtPOINewToUser && !skipIntro)
        {            
            AuralInstruction.PlayTriviaGoToIntro();
            LoadTask(way, okPathtpoint, wrongPathpoint);

            CurrentAdaptationTask.AdaptationIntroShown = true;            
        }
        else
        {
            LoadTask(way, okPathtpoint, wrongPathpoint);
            ShowTask(true);

            CurrentAdaptationTask.AdaptationIntroShown = false;
            SendAdaptationTaskData();
        }

    }

    public void InformModeCancelled()
    {
        CurrentAdaptationTask.AdaptationTaskAccepted = false;
        SendAdaptationTaskData();

        CurrentAdaptationTask.IsTaskStart = false;
        CurrentAdaptationTask.AdaptationTaskCompleted = false;
        SendAdaptationTaskData();

        CurrentAdaptationTask = null;
    }

    public void InformTaskNotDone()
    {
        // are we still awaiting confirmation? that means we didn't send the start of the event
        if (CurrentAdaptationTask.AdaptationIntroShown == true &&
            CurrentAdaptationTask.AdaptationTaskAccepted == null)
        {
            SendAdaptationTaskData();
        }

        CurrentAdaptationTask.IsTaskStart = false;
        CurrentAdaptationTask.AdaptationTaskCompleted = false;
        SendAdaptationTaskData();
    }

    public void TaskAcceptedByUser()
    {
        CurrentAdaptationTask.AdaptationTaskAccepted = true;
        SendAdaptationTaskData();

        ShowTask(true);

    }


    public void LoadTask(Way way, Pathpoint okPathpoint, Pathpoint wrongPathpoint)
    {
        POI = okPathpoint;
        isTaskDone = false;

        EnableComponentsForTask(true);


        // Select the pictures to show

        //Assign the okPathpoint randomly to either Option1 or 2, to avoid positional bias         
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            Option1.RenderPicture(okPathpoint.Photos[0].Data.Photo);
            Option2.RenderPicture(wrongPathpoint.Photos[0].Data.Photo);
            correctDecisionIndex = 1;
        }
        else
        {
            Option1.RenderPicture(wrongPathpoint.Photos[0].Data.Photo);
            Option2.RenderPicture(okPathpoint.Photos[0].Data.Photo);
            correctDecisionIndex = 2;
        }

        // Prepare card
        Card.FillInstruction(null, null);
        Card.InstructionIconPOI.RenderQuiz();
        Card.RenderInstruction();

    }

    public void ShowTask(bool showTask)
    {        
        IntroPanel.SetActive(!showTask);
        TaskPanel.SetActive(showTask);

        if (showTask)
        {
            AuralInstruction.CancelCurrentPlayback();
            AuralInstruction.PlayTriviaGoToTask();
        }
    }

    // returns the index of a random element ahead, unless there are no ahead alternatives,
    // case it looks backwards
    // TODO: it should not considered 'hidden' POIs (skipped)
    public int SelectRandomAlternative(int currentIndex, List<Pathpoint> poiList)
    {
        int randomIndex = 0;

        // Select an element from POIList between CurrentPOIIndex and Count-1 (including)
        if (currentIndex >= 0 && currentIndex < poiList.Count - 1)
        {
            randomIndex = UnityEngine.Random.Range(currentIndex + 1, poiList.Count);
        }
        else if (currentIndex > 0 && poiList.Count > 2)
        {
            randomIndex = UnityEngine.Random.Range(0, currentIndex);
        }

        return randomIndex;
    }

    public bool IsTaskDone()
    {
        return isTaskDone;
    }

    public void OnOptionSelected(bool active)
    {
        // to trigger it only once, since we have the toggles
        // associated to it
        if (!active && OptionGroup.AnyTogglesOn())
            return;

        if (OptionGroup.AnyTogglesOn())
        {
            Card.RenderConfirmation();
        } else
        {
            Card.RenderInstruction();
        }

        Debug.Log("Option selected: "+ active);
        isTaskDone = true;
    }


    public void EvaluateSelection()
    {
        bool success = false;
        PictureToggle picToggle = null;

        foreach (var toggle in OptionGroup.ActiveToggles())
        {
            picToggle = toggle.gameObject.GetComponent<PictureToggle>();
            success = picToggle.ToggleIndex == correctDecisionIndex;
            picToggle.RenderFeedback(success);
        }

        // Disable the toggles
        EnableComponentsForTask(false);

        AuralInstruction.CancelCurrentPlayback();

        if (success)
        {
            AuralInstruction.PlayTriviaGoToFeedbackOk();
        }
        else
        {
            AuralInstruction.PlayTriviaGoToFeedbackWrong();
            AuralInstruction.PlayGotoInstruction(POI);

            // Mark the other as correct
            if (Option1 != picToggle)
            {
                Option1.RenderFeedback(true);
                // Scroll to the left after a delay
                Invoke("ScrollToLeft", 1f);
            }
            else
            {
                Option2.RenderFeedback(true);
                // Scroll to the right after a delay
                Invoke("ScrollToRight", 1f);
            }
        }


        // send information about performance
        CurrentAdaptationTask.IsTaskStart = false;
        CurrentAdaptationTask.AdaptationTaskCompleted = true;
        CurrentAdaptationTask.AdaptationTaskCorrect = success;
        SendAdaptationTaskData();
    }

    public void ScrollToLeft()
    {
        // Scroll all the way to the left
        OptionScrollView.horizontalNormalizedPosition = 0f;
    }

    public void ScrollToRight()
    {
        // Scroll all the way to the right
        OptionScrollView.horizontalNormalizedPosition = 1f;
    }

    private void EnableComponentsForTask(bool activate)
    {

        Option1.enabled = activate;
        Option2.enabled = activate;
        EvaluateButton.gameObject.SetActive(activate);
        Card.ButtonConfirmationTimed.gameObject.SetActive(!activate);
    }



    public void CleanupView()
    {
        //Card.OnCardTransition.RemoveListener(Card_OnCardTransition);
        Option1.CleanupView();
        Option1.CleanupView();        
    }

    private void OnDestroy()
    {
        CleanupView();
        Card.OnCardTransition.RemoveListener(Card_OnCardTransition);
    }

    private void SendAdaptationTaskData()
    {
        CurrentAdaptationTask.AdaptationSupportMode = PathpointPIM.SupportMode.Trivia;
        OnAdaptationEventChange?.Invoke(this, new EventArgs<AdaptationTaskArgs>(CurrentAdaptationTask));
    }

    // Handlers
    private void Card_OnCardTransition()
    {
        OnTaskCompleted.Invoke();
    }


}
