using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Recorders.Inputs;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
public class PhoneCam : MonoBehaviour
{
    [Header(@"preview")]
    public RawImage rawImage;
    public AspectRatioFitter aspectRatioFitter;
    [Header(@"Recording")]
    public int videoWidth = 640;
    public int videoHeight = 480;
    public int fps = 12;
    public bool recordMicrophone;

    private MP4Recorder recorder;
    private CameraInput cameraInput;
    private AudioInput audioInput;
    private AudioSource microphoneSource;
    private WebCamTexture webCamTexture;
    private Color32[] pixelBuffer;
    private RealtimeClock clock;
    private bool recording = false;

    private IEnumerator Start()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
#endif
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.Log("no Cam Detected");
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if (!devices[i].isFrontFacing)
            {
                webCamTexture = new WebCamTexture(devices[i].name, videoWidth, videoWidth, fps);
            }
        }

        if (webCamTexture == null)
        {
            Debug.Log("unable to find Cam");
        }
        webCamTexture.Play();
        rawImage.texture = webCamTexture;
        aspectRatioFitter.aspectRatio = (float)webCamTexture.width / webCamTexture.height;

        // Start microphone
        microphoneSource = gameObject.AddComponent<AudioSource>();
        microphoneSource.mute =
        microphoneSource.loop = true;
        microphoneSource.bypassEffects =
        microphoneSource.bypassListenerEffects = false;
        microphoneSource.clip = Microphone.Start(null, true, 1, AudioSettings.outputSampleRate);
        yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);
        microphoneSource.Play();
    }

    private void OnDestroy()
    {
        // Stop microphone
        microphoneSource.Stop();
        Microphone.End(null);
    }

    // Update is called once per frame
    void Update()
    {
        if (recording && webCamTexture.didUpdateThisFrame)
        {
            webCamTexture.GetPixels32(pixelBuffer);
            recorder.CommitFrame(pixelBuffer, clock.timestamp);
        }
    }

    public void StartRecording()
    {
        // Start recording
        var sampleRate = recordMicrophone ? AudioSettings.outputSampleRate : 0;
        var channelCount = recordMicrophone ? (int)AudioSettings.speakerMode : 0;
        clock = new RealtimeClock();
        recorder = new MP4Recorder(videoWidth, videoHeight, fps, sampleRate, channelCount, audioBitRate: 96_000);
        // Create recording inputs
        pixelBuffer = webCamTexture.GetPixels32();
        audioInput = recordMicrophone ? new AudioInput(recorder, clock, microphoneSource, true) : null;
        // Unmute microphone
        microphoneSource.mute = audioInput == null;
        recording = true;
    }

    public async void StopRecording()
    {
        recording = false;
        // Mute microphone
        microphoneSource.mute = true;
        // Stop recording
        audioInput?.Dispose();
        var path = await recorder.FinishWriting();
        // Playback recording
        Debug.Log($"Saved recording to: {path}");
    }

    public async void TakePicture()
    {
        JPGRecorder rec = new JPGRecorder(videoWidth, videoHeight);
        rec.CommitFrame(pixelBuffer);
        var path = await rec.FinishWriting();
        Debug.Log($"Saved recording to: {path}");
    }

    public void TogglePause()
    {
        clock.paused = !clock.paused;
        recording = !recording;
    }
}
