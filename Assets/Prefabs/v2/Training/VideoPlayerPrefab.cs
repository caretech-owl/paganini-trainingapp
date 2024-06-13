using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerPrefab : MonoBehaviour
{
    public VideoPlayer VideoManager;

    [Header("UI States")]
    public GameObject DataState;
    public GameObject BlankState;

    [Header("Video Controllers")]
    public GameObject ReplayControl;
    public GameObject ControlWrapper;

    [Header("Events")]
    public UnityEvent OnPlayBackFinished;


    private double StartTimestamp;
    private double EndTimestamp;
    private bool awaitingPlaybackAction;
    private bool showControls = true;
    private float videoAspectRatio;
    private bool isVideoAvailable = false;


    // Start is called before the first frame update
    void Start()
    {
      //  VideoManager.prepareCompleted += OnVideoPrepared;
      //  VideoManager.seekCompleted += OnSeekCompleted;
      
    }

    // Update is called once per frame
    void Update()
    {
        if (VideoManager == null) return;
        // We wait until the replay or skip action is performed by the video player,
        // otherwise, it would bring up the end of video screen again in the next frame
        if (awaitingPlaybackAction && VideoManager.time < EndTimestamp) {
            awaitingPlaybackAction = false;
        }
        else if (awaitingPlaybackAction)
        {
            return;
        }

        if (showControls && Math.Round(VideoManager.time) >= Math.Round(StartTimestamp) && VideoManager.isPaused && VideoManager.isPrepared)
        {
            //EnableReplayControl(true);
            //EnableVideoControls();

            OnPlayBackFinished?.Invoke();
        }

    }

    public void SetupPlayback()
    {
        showControls = true;
        VideoManager.time = 0;

        StartTimestamp = 0;
        EndTimestamp = VideoManager.length;

        awaitingPlaybackAction = false;
        videoJustLoaded = true;

        // Default controls when we load the video
        EnableReplayControl(false);

        if (!VideoManager.isPrepared)
        {
            VideoManager.prepareCompleted -= OnVideoPrepareCompleted;
            VideoManager.prepareCompleted += OnVideoPrepareCompleted;
            VideoManager.Play();
        }

    }

    public void SetupPlayback(double startTime, double endTime)
    {
        Debug.Log("SkipToVideoFrame from: " + VideoManager.time + "o Timestamp: " + startTime);

        showControls = true;
        VideoManager.time = startTime;

        StartTimestamp = startTime;
        EndTimestamp = endTime;

        awaitingPlaybackAction = false;
        videoJustLoaded = true;

        // Default controls when we load the video
        EnableReplayControl(false);

        if (!VideoManager.isPrepared)
        {
            VideoManager.prepareCompleted -= OnVideoPrepareCompleted;
            VideoManager.prepareCompleted += OnVideoPrepareCompleted;
            VideoManager.Play();
        }
            
    }

    private bool videoJustLoaded = true;
    public void EnableVideoControls() {
        VideoManager.Pause();
        ControlWrapper.SetActive(true);        

        //EnableReplayControl(VideoManager.time >= EndTimestamp);

        videoJustLoaded = false;
    }

    public void ResumePlayback() {
        //SetAspectRatioToVideo(VideoManager.gameObject, videoAspectRatio);
        VideoManager.Play();
        ControlWrapper.SetActive(false);
    }

    public void Replay()
    {
        // Takes some time (frames) to actually play
        awaitingPlaybackAction = true;
        VideoManager.time = StartTimestamp;        
        ControlWrapper.SetActive(false);

        EnableReplayControl(false);

        VideoManager.Play();
    }

    public void HideControls()
    {
        showControls = false;
        ControlWrapper.SetActive(false);
    }

    public void RenderFirstFrame()
    {
        Debug.Log($"RenderFirstFrame VideoPrepared: {VideoManager.isPrepared}");
        StartTimestamp = 0;
        VideoManager.frame = 0;
    }

    public void RenderLastFrame()
    {
        Debug.Log($"RenderLastFrame VideoPrepared: {VideoManager.isPrepared}");
        StartTimestamp = -1;
        VideoManager.frame = (long)(VideoManager.frameCount - 1);
    }

    public void SkipForward() {
        //double playtime = VideoManager.clip.length;
        SkipTimeAndPlay(10);
    }

    public void SkipBackwards() {
        SkipTimeAndPlay(-10);
    }

    private void SkipTimeAndPlay(double skipTime)
    {
        awaitingPlaybackAction = true;
        double targetTime = VideoManager.time + skipTime;
        // Let's check the boundaries of our playback
        if (skipTime>0) {
            skipTime = targetTime > EndTimestamp ?  EndTimestamp - VideoManager.time : skipTime;
        } else {
            skipTime = targetTime < StartTimestamp ? StartTimestamp - VideoManager.time  : skipTime;
        }
        

        VideoManager.time += skipTime;
        VideoManager.Play();
        ControlWrapper.SetActive(false);
    }

    private void OnVideoPrepareCompleted(VideoPlayer player)
    {
        Debug.Log("Video prepared!");

        VideoManager.prepareCompleted -= OnVideoPrepareCompleted;

        // render last frame if set to -1
        StartTimestamp = StartTimestamp == -1 ? VideoManager.length : StartTimestamp;
        VideoManager.time = StartTimestamp;
        EndTimestamp = VideoManager.length;

        EnableVideoControls();
    }

    private void EnableReplayControl(bool activate) {
        ReplayControl.SetActive(activate);
    }


    public void LoadVideo(string url, float? aspectRatio = null)
    {
        showControls = true;

        if (File.Exists(url))
        {
            BlankState.SetActive(false);
            DataState.SetActive(true);
            isVideoAvailable = true;

            if (VideoManager != null)
            {
                VideoManager.url = url;
                if (aspectRatio == null)
                {
                    StartCoroutine(DetectVideoResolution());
                }
                else
                {
                    videoAspectRatio = (float)aspectRatio;
                    SetAspectRatioToVideo(VideoManager.gameObject, (float)aspectRatio);
                }

            }
        } else {
            DataState.SetActive(false);
            BlankState.SetActive(true);
            isVideoAvailable = false;
        }

    }

    public bool IsVideoAvailable()
    {
        return isVideoAvailable;
    }

    private IEnumerator DetectVideoResolution()
    {
        yield return new WaitUntil(() => VideoManager.isPrepared); // Wait until the video is prepared.

        long width = VideoManager.width;   // Width of the video
        long height = VideoManager.height; // Height of the video

        Debug.Log("Video Resolution: " + width + "x" + height);

        videoAspectRatio = (float) VideoManager.width / VideoManager.height;

        SetAspectRatioToVideo(VideoManager.gameObject, videoAspectRatio);

    }

    public void SetAspectRatioToVideo(GameObject targetObject, float videoAspectRatio)
    {        
        var wrapperTransform = DataState.GetComponent<RectTransform>();

        float height = wrapperTransform.rect.height;
        float width = height / videoAspectRatio;

        float wrapperAspectRatio = wrapperTransform.rect.width / wrapperTransform.rect.height;
        if (wrapperAspectRatio < videoAspectRatio)
        {
            width = wrapperTransform.rect.width;
            height = width * videoAspectRatio;
        }
        else
        {
            height = wrapperTransform.rect.height;
            width = width / videoAspectRatio;
        }

        Vector3 newScale = targetObject.transform.localScale;
        newScale.y = width;
        newScale.x = height; // it's rotated 90 degrees

        targetObject.transform.localScale = newScale;

        //wrapperTransform.sizeDelta = new Vector2(width, height);
    }

    public Texture2D ExtractLastFrame()
    {
        Texture2D lastFrameTexture = new Texture2D((int)VideoManager.width, (int)VideoManager.height, TextureFormat.RGB24, false);
        RenderTexture renderTexture = new RenderTexture(lastFrameTexture.width, lastFrameTexture.height, 24);
        VideoManager.targetTexture = renderTexture;
        VideoManager.frame = (long)(VideoManager.frameCount - 1); // Jump to the last frame

        VideoManager.frameReady -= OnFrameReady;
        VideoManager.frameReady += OnFrameReady;

        Graphics.Blit(VideoManager.texture, renderTexture);
        RenderTexture.active = renderTexture;
        lastFrameTexture.ReadPixels(new Rect(0, 0, lastFrameTexture.width, lastFrameTexture.height), 0, 0);
        lastFrameTexture.Apply();
        VideoManager.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
        return lastFrameTexture;
    }

    void OnFrameReady(VideoPlayer source, long frameIdx)
    {
        // Perform actions when a frame is ready
        Debug.Log("Frame " + frameIdx + " is ready!");
    }

    public void CleanupView()
    {
        // Unsubscribe from events to prevent memory leaks
        VideoManager.prepareCompleted -= OnVideoPrepareCompleted;

        // Unload the video to release its resources
        VideoManager.url = null;
        VideoManager.frame = 0;

        // Deactivate any active game objects
        ControlWrapper.SetActive(false);

        // Reset variables to their initial state
        StartTimestamp = 0;
        EndTimestamp = 0;
        awaitingPlaybackAction = false;
        videoJustLoaded = true;
    }

}
