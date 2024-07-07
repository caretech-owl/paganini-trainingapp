using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using static DirectionIcon;
using static PaganiniRestAPI;

public class WayTriviaInstruction : MonoBehaviour
{
    [Header("Main states")]
    public GameObject IntroPanel;  
    public GameObject TaskPanel;

    [Header("UI Components")]
    public PhotoSlideShow SlideShow;
    public InstructionCard Card;
    public AudioInstruction AuralInstruction;


    [Header("Trivia Options")]
    public GameButton DirectionOption1;
    public GameButton DirectionOption2;

    //public GameObject VideoClose;

    [Header("Events")]
    public UnityEvent OnTaskCompleted;
    public event EventHandler<EventArgs<AdaptationTaskArgs>> OnAdaptationEventChange;

    private Pathpoint POI;
    private DirectionType option1;
    private DirectionType option2;
    private DirectionType correctDecision;
    private bool isTaskDone;
    private AdaptationTaskArgs CurrentAdaptationTask;

    // Start is called before the first frame update
    void Start()
    {
        Card.SetCardTransitionTimer(AppState.Training.DecisionPointConfirmationDelay);
        Card.OnCardTransition.AddListener(Card_OnCardTransition);

        DirectionOption1.GetComponent<Button>().onClick.AddListener(() => OnButtonOptionClicked(DirectionOption1));
        DirectionOption2.GetComponent<Button>().onClick.AddListener(() => OnButtonOptionClicked(DirectionOption2));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadInstruction(Way way, Pathpoint pathtpoint, bool skipIntro = false)
    {
        CurrentAdaptationTask = new();
        CurrentAdaptationTask.IsTaskStart = true;

        // init view
        ShowTask(false);

        if (pathtpoint.CurrentInstructionMode.IsAtPOINewToUser && !skipIntro)
        {            
            AuralInstruction.PlayTriviaDirectionIntro();
            LoadTask(way, pathtpoint);

            CurrentAdaptationTask.AdaptationIntroShown = true;
        }
        else
        {
            LoadTask(way, pathtpoint);
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

    public bool IsTaskDone()
    {
        return isTaskDone;
    }

    public void LoadTask(Way way, Pathpoint pathtpoint)
    {
        POI = pathtpoint;
        isTaskDone = false;

        // Load the picture 
        SlideShow.LoadSlideShow(pathtpoint.Photos);
        
        // Generate alternatives for quiz
        GenerateAlternatives();

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
            AuralInstruction.PlayTriviaDirectionTask();
        }
    }

    private void GenerateAlternatives()
    {
        // Generate alternatives
        DirectionType correctDirection;

        DirectionOption1.ResetButton();
        DirectionOption2.ResetButton();

        if (Enum.TryParse(POI.Instruction.ToString(), out correctDirection))
        {
            DirectionOption1.gameObject.SetActive(true);
            DirectionOption2.gameObject.SetActive(true);

            correctDecision = correctDirection;
        }
        else
        {
            DirectionOption1.gameObject.SetActive(false);
            DirectionOption2.gameObject.SetActive(false);
            return;
        }

        List<DirectionType> options = new List<DirectionType> { DirectionType.LeftTurn, DirectionType.Straight, DirectionType.RightTurn };

        // Remove correctDirection from options
        options.Remove(correctDirection);

        // Select the other options randomly from the remaining ones
        DirectionType incorrectDirection = options[UnityEngine.Random.Range(0, options.Count)];

        DirectionType arrowLeft;
        DirectionType arrowRight;

        // Determine the positions of the arrows based on the correct and incorrect directions
        if (correctDirection == DirectionType.LeftTurn)
        {
            arrowLeft = correctDirection;
            arrowRight = incorrectDirection;
        }
        else if (correctDirection == DirectionType.RightTurn)
        {
            arrowRight = correctDirection;
            arrowLeft = incorrectDirection;
        }
        else // Straight
        {
            // Randomly assign the correct direction to either option 1 or 2
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                arrowLeft = correctDirection;
                arrowRight = incorrectDirection;
            }
            else
            {
                arrowRight = correctDirection;
                arrowLeft = incorrectDirection;
            }
        }

        // Set the directions of the arrows for the options
        SetArrowDirection(DirectionOption1.IconNormal, arrowLeft);
        SetArrowDirection(DirectionOption1.IconPressed, arrowLeft);
        SetArrowDirection(DirectionOption2.IconNormal, arrowRight);
        SetArrowDirection(DirectionOption2.IconPressed, arrowRight);
        option1 = arrowLeft;
        option2 = arrowRight;
    }

    private void SetArrowDirection(GameObject arrow, DirectionType directionType)
    {

        // Set the rotation of the arrow based on the direction type
        switch (directionType)
        {
            case DirectionType.LeftTurn:
                arrow.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); // Rotate 90 degrees to the left
                break;
            case DirectionType.RightTurn:
                arrow.transform.localRotation = Quaternion.Euler(0f, 0f, -90f); // Rotate 90 degrees to the right
                break;
            case DirectionType.Straight:
                arrow.transform.localRotation = Quaternion.identity; // No rotation for straight direction
                break;
            default:
                Debug.LogError("Unknown direction type: " + directionType);
                break;
        }
    }

    private void OnButtonOptionClicked(GameButton button)
    {
        bool success = false;
        isTaskDone = true;

        if (button == DirectionOption1)
        {
            Debug.Log("Option 1 was clicked");
            success = option1 == correctDecision;

            RenderButtonFeedback(DirectionOption1, success);            
            RenderButtonFeedback(DirectionOption2, success? null : !success);

        }
        else if (button == DirectionOption2 )
        {
            Debug.Log("Option 2 was clicked");
            success = option2 == correctDecision;

            RenderButtonFeedback(DirectionOption2, success);
            RenderButtonFeedback(DirectionOption1, success ? null : !success);
        }

        AuralInstruction.CancelCurrentPlayback();

        if (success)
        {
            AuralInstruction.PlayTriviaDirectionFeedbackOk();
        }
        else
        {
            AuralInstruction.PlayTriviaDirectionFeedbackWrong();
        }


        // send information about performance
        CurrentAdaptationTask.IsTaskStart = false;
        CurrentAdaptationTask.AdaptationTaskCompleted = true;
        CurrentAdaptationTask.AdaptationTaskCorrect = success;
        SendAdaptationTaskData();

    }

    private void SendAdaptationTaskData()
    {
        CurrentAdaptationTask.AdaptationSupportMode = PathpointPIM.SupportMode.Trivia;
        OnAdaptationEventChange?.Invoke(this, new EventArgs<AdaptationTaskArgs>(CurrentAdaptationTask));
    }

    // status: correct, incorrect, neutral (null)
    private void RenderButtonFeedback(GameButton button, bool? correct)
    {
        button.RenderConfirmationState(correct);
    }

    public void CleanUpView()
    {
        SlideShow.CleanUpView();
    }


    private void OnDestroy()
    {
        Card.OnCardTransition.RemoveListener(Card_OnCardTransition);
        DirectionOption1.GetComponent<Button>().onClick.RemoveAllListeners(); // Remove all listeners for Option 1
        DirectionOption2.GetComponent<Button>().onClick.RemoveAllListeners(); // Remove all listeners for Option 2

        CleanUpView();
    }


    // Handlers
    private void Card_OnCardTransition()
    {
        OnTaskCompleted.Invoke();
    }


}
