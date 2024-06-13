using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class WayGoToInstruction : MonoBehaviour
{  
    public PhotoSlideShow SlideShow;
    public InstructionCard Card;
    public GameObject HideSupportPanel;    

    private Pathpoint POI;

    public UnityEvent OnTaskCompleted;

    public float HideSupportDelaySeconds = 10;
    public bool EnableHideSupport { get; set; }

    private bool cancelHideSupport { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Card.SetCardTransitionTimer(AppState.Training.GotoConfirmationDelay);
        Card.OnCardTransition.AddListener(Card_OnCardTransition);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadInstruction(Way way, Pathpoint pathtpoint)
    {
        POI = pathtpoint;


        SlideShow.LoadSlideShow(pathtpoint.Photos);

        string title = "Geh weiter, bis du das siehst";
        string subtitle = pathtpoint.Description;
        
        if (subtitle == null || subtitle.Trim() == "") {
            subtitle = "";            
        }

        Card.FillInstruction(title, subtitle, subtitle.Trim() != "");
        Card.InstructionIconPOI.RenderIcon(pathtpoint, way);
        Card.RenderInstruction();

        cancelHideSupport = false;
        DelayHideInstruction();
        
    }

    public void DelayHideInstruction()
    {
        if (EnableHideSupport) {
            StartCoroutine(HideInstructionSupport());
        }        
    }

    public void LoadInstructionConfirmation()
    {
        HideSupportPanel.SetActive(false);
        Card.RenderConfirmation();        
    }

    public void CancelHideSupport()
    {
        cancelHideSupport = true;
        HideSupportPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        Card.OnCardTransition.RemoveListener(Card_OnCardTransition);
    }

    // Handlers
    private void Card_OnCardTransition()
    {
        OnTaskCompleted.Invoke();
    }

    private IEnumerator HideInstructionSupport()
    {
        // let's wait a for a moment before checking.
        yield return new WaitForSeconds(HideSupportDelaySeconds);
        
        HideSupportPanel.SetActive(!cancelHideSupport);

    }

    public void CleanUpView()
    {
        SlideShow.CleanUpView();
    }

    //private IEnumerator HideInstructionSupport()
    //{
    //    // Let's wait for a moment before fading in.
    //    yield return new WaitForSeconds(5f);

    //    // Set the alpha to zero to make it transparent
    //    CanvasGroup canvasGroup = HideSupportPanel.GetComponent<CanvasGroup>();
    //    canvasGroup.alpha = 0f;
    //    HideSupportPanel.SetActive(true);

    //    // Use LeanTween to fade in
    //    LeanTween.alphaCanvas(canvasGroup, 1f, fadeDuration)
    //        .setEase(LeanTweenType.easeOutQuad); // Use easeOutQuad for a smooth fade-in effect
    //}

}
