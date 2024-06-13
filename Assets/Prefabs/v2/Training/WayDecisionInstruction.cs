using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;

public class WayDecisionInstruction : MonoBehaviour
{
    [Header("Main instructions")]
    public GameObject TextPanel;
    public GameObject PhotoPanel;
    public GameObject VideoPanel;    

    [Header("Photo Instruction")]
    public PhotoSlideShow SlideShow;
    public DirectionIcon DirectionNavIcon;
    public Image PictureConfirmation;

    [Header("Video Instruction")]
    public VideoPlayerPrefab VideoInstruction;
    public GameObject VideoConfirmation;

    [Header("Text Instruction")]
    public InstructionCard Card;

    [Header("Options")]
    public GameObject OverlayOptions;
    public GameObject VideoOpen;
    public GameObject VideoClose;
    public bool EnablePicturePoVConfirmation = true;
    public bool EnableOverlayOptions = true;

    private Pathpoint POI;
    private bool isARCompassEnabled = false;

    public UnityEvent OnTaskCompleted;

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

    public void LoadInstruction(Way way, Pathpoint pathtpoint)
    {
        POI = pathtpoint;
        isARCompassEnabled = false;
        string title = "";
        string subtitle = pathtpoint.Description;

        PhotoPanel?.SetActive(true);
        TextPanel?.SetActive(true);
        PictureConfirmation?.gameObject.SetActive(false);

        OverlayOptions?.SetActive(EnableOverlayOptions);
        VideoClose?.SetActive(false);
        VideoOpen?.SetActive(true);

        SlideShow.LoadSlideShow(pathtpoint.Photos);
        PrepareVideoInstruction(way, pathtpoint);

        // Render direction
        DirectionIcon.DirectionType directionType;
        if (POI == null)
        {
            Debug.Log("POI is null!!");
            return;
        }

        if (Enum.TryParse(POI.Instruction.ToString(), out directionType))
        {
            DirectionNavIcon.gameObject.SetActive(true);
            DirectionNavIcon.RenderDirection(directionType);
            title = directionType.GetDescription();
        }
        else
        {
            DirectionNavIcon.gameObject.SetActive(false);
            title = "Keine Anleitung gefunden";
        }

        // Render Card
        if (subtitle == null || subtitle.Trim() == "")
        {
            subtitle = "";
        }

        Card.FillInstruction(title, subtitle);
        Card.InstructionIconPOI.RenderIcon(pathtpoint, way);
        Card.RenderInstruction();

    }

    public void RenderVideo()
    {
        VideoPanel?.SetActive(true);
        //VideoInstruction.ResumePlayback();
        VideoInstruction.Replay();

        TextPanel?.SetActive(false);
        PhotoPanel?.SetActive(false);


        VideoConfirmation.SetActive(false);
        PictureConfirmation?.gameObject.SetActive(false);

        OverlayOptions?.SetActive(EnableOverlayOptions);
        VideoClose?.SetActive(true);
        VideoOpen?.SetActive(false);

        VideoInstruction.OnPlayBackFinished.AddListener(PlaybackFinishedHandler);
    }

    public void RenderARModeBackgroundSupport()
    {
        isARCompassEnabled = true;
    }

    public bool IsARCompassEnabled()
    {
        return isARCompassEnabled;
    }

    private void PlaybackFinishedHandler()
    {
        CloseVideo();
        //VideoInstruction.RenderFirstFrame();
        VideoInstruction.OnPlayBackFinished.RemoveListener(PlaybackFinishedHandler);
    }

    public void CloseVideo()
    {
        //VideoPanel?.SetActive(false);

        TextPanel?.SetActive(true);
        PhotoPanel?.SetActive(true);

        VideoClose?.SetActive(false);
        VideoOpen?.SetActive(true);
    }

    public void LoadInstructionConfirmation()
    {
        TextPanel?.SetActive(true);
        VideoInstruction.HideControls();
        VideoInstruction.RenderLastFrame();
        Card.RenderConfirmation();

        // photo mode
        if (PhotoPanel.activeSelf) {
            DirectionNavIcon.SetConfirmationMode(true);
            VideoConfirmation.SetActive(false);
            RenderPhotoConfirmation();
        }
        else
        {
            DirectionNavIcon.gameObject.SetActive(false);
            VideoConfirmation.SetActive(true);
        }        

    }

    public void HideOverlayOptions()
    {
        OverlayOptions?.SetActive(false);
    }

    public void ShowOverlayOptions(bool doShow)
    {
        EnableOverlayOptions = doShow;
        OverlayOptions?.SetActive(doShow);
    }

    private Texture2D lastFrameTexture; // Declare a field to hold the last frame texture

    private void RenderPhotoConfirmation()
    {
        if (!VideoInstruction.IsVideoAvailable() || !EnablePicturePoVConfirmation || isARCompassEnabled)
            return;

        DirectionNavIcon.gameObject.SetActive(false);
        VideoConfirmation.SetActive(true);

        VideoPanel?.SetActive(true);
        PhotoPanel?.SetActive(false);

    }


    private void PrepareVideoInstruction(Way way, Pathpoint poi)
    {
        string filename = poi.VideoFilename;
        string filepath = $"{FileManagement.persistentDataPath}/{way.Name}/Videos/{filename}";

        Debug.Log("Loading Video: " + filepath);
        
        VideoInstruction.CleanupView();        
        VideoInstruction.LoadVideo(filepath);
        VideoInstruction.SetupPlayback();
    }

    public void CleanUpView()
    {        
        VideoInstruction.CleanupView();
        SlideShow.CleanUpView();
    }


    private void OnDestroy()
    {
        Card.OnCardTransition.RemoveListener(Card_OnCardTransition);
        //Destroy(PictureConfirmation.sprite);

        CleanUpView();
    }

    // Handlers
    private void Card_OnCardTransition()
    {
        OnTaskCompleted.Invoke();
    }


}
