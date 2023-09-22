using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Recorders.Inputs;
using System;
#if PLATFORM_ANDROID
using UnityEngine.Android;
using System.IO;
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

    [Header(@"UI Configuration")]
    public GameObject IconFrom;
    public GameObject IconTo;

    //private MP4Recorder recorder;
    private HEVCRecorder recorder;    
    private AudioInput audioInput;
    private AudioSource microphoneSource;
    private WebCamTexture webCamTexture;
    private Color32[] pixelBuffer;
    private RealtimeClock clock;

    private string RouteName;


    private void Start()
    {

    }

    public void StartCamera()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }

#endif

        if (AppState.currentBegehung == null || AppState.currentBegehung.Trim().Length == 0)
        {
            SceneManager.LoadScene(SceneSwitcher.UserLoginScene);
        }
        else
        {
            StartCoroutine(InitialiseCamera());
        }


        
    }

    private IEnumerator InitialiseCamera()
    {

        Way w = SessionData.Instance.GetData<Way>("SelectedWay");
        IconFrom.GetComponent<LandmarkIcon>().selectedLandmarkType = (LandmarkIcon.LandmarkType)Int32.Parse(w.StartType);
        IconTo.GetComponent<LandmarkIcon>().selectedLandmarkType = (LandmarkIcon.LandmarkType)Int32.Parse(w.DestinationType);


        RouteName = AppState.currentBegehung;

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.Log("no Cam Detected");
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if (!devices[i].isFrontFacing)
            {
                webCamTexture = new WebCamTexture(devices[i].name, videoWidth, videoHeight, fps);
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

        string deviceName = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        Debug.Log(deviceName);

        //try
        //{
        //    microphoneSource.clip = Microphone.Start(null, true, 1, AudioSettings.outputSampleRate);
        //}
        //catch
        //{
        //    Debug.Log("Problems Initialising microphone clip");
        //}

        //if (MicrophoneInitialised() > 0)
        //{
        //    yield return new WaitUntil(() => MicrophoneInitialised() > 0);
        //    microphoneSource.Play();
        //}


        microphoneSource.clip = Microphone.Start(null, true, 1, AudioSettings.outputSampleRate);
        yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);
        microphoneSource.Play();

        
    }

    //private int MicrophoneInitialised()
    //{
    //    try
    //    {
    //        return Microphone.GetPosition(null);
    //    }
    //    catch (Exception e)
    //    {
    //        return -1;
    //    }


    //}


    private void OnDestroy()
    {
        if (microphoneSource != null)
        {
            // Stop microphone
            microphoneSource.Stop();
            Microphone.End(null);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (AppState.recording && webCamTexture.didUpdateThisFrame && !AppState.pausedRec)
        {
            webCamTexture.GetPixels32(pixelBuffer);
            recorder.CommitFrame(pixelBuffer, clock.timestamp);
        }
    }

    public void StartRecording()
    {
        if (!AppState.recording)
        {
            if (Directory.Exists(Path.Combine(Application.persistentDataPath, RouteName)))
                Directory.Delete(Path.Combine(Application.persistentDataPath, RouteName),true);

            // Start recording
            var sampleRate = recordMicrophone ? AudioSettings.outputSampleRate : 0;
            var channelCount = recordMicrophone ? (int)AudioSettings.speakerMode : 0;
            clock = new RealtimeClock();
            //recorder = new MP4Recorder(videoWidth, videoHeight, fps, sampleRate, channelCount, audioBitRate: 96_000);
            //recorder = new HEVCRecorder(videoWidth, videoHeight, fps, sampleRate, channelCount, audioBitRate: 96_000, videoBitRate: 500_000);
            recorder = new HEVCRecorder(videoWidth, videoHeight, fps, sampleRate, channelCount, videoBitRate: 600_000); 

            // Create recording inputs
            pixelBuffer = webCamTexture.GetPixels32();
            audioInput = recordMicrophone ? new AudioInput(recorder, clock, microphoneSource, true) : null;
            // Unmute microphone
            microphoneSource.mute = audioInput == null;
            AppState.recording = true;
        }
    }

    public async void StopRecording()
    {
        if (AppState.recording)
        {
            AppState.recording = false;
            // Mute microphone
            microphoneSource.mute = true;
            // Stop recording
            audioInput?.Dispose();
            var path = await recorder.FinishWriting();
            // Playback recording
            Debug.LogWarning($"Saved recording to: {path}");
            string[] split = path.Split('/');
            string filename = "/" + split[split.Length - 1]; //".mp4";
            if(!Directory.Exists(Path.Combine(Application.persistentDataPath, RouteName))){
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, RouteName));
            }

            string VidDir= Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, RouteName ,"Videos")).FullName;
            File.Move(path, VidDir+filename);
     
        }

        //Debug.Log(Application.persistentDataPath);
    }


    public async void CancelRecording()
    {
        AppState.recording = false;

        string ImgDir = Path.Combine(Application.persistentDataPath, RouteName, "Fotos");
        Directory.Delete(ImgDir, true);

        var VidPath = await recorder.FinishWriting();
        File.Delete(VidPath);
    }


    public async void TakePicture(string filename)
    {
        if (AppState.recording)
        {
            string ImgDir = Path.Combine(Application.persistentDataPath, RouteName, "Fotos");
            if (!Directory.Exists(ImgDir)){
                Directory.CreateDirectory(ImgDir);
            }

            JPGRecorder rec = new JPGRecorder(videoWidth, videoHeight);
            rec.CommitFrame(pixelBuffer);
            var path = await rec.FinishWriting();
            Debug.LogWarning($"Saved recording to: {path}");
            string[] split = path.Split('/');
            //string filename = split[split.Length - 1]+".jpg";
            string[] fotos=Directory.GetFiles(path);
            foreach (string foto in fotos)
            {
                File.Move(foto, Path.Combine(ImgDir, filename));
            }
            Debug.LogWarning(path);
            Directory.Delete(path, true);
        }
    }

    public void TogglePause()
    {
        clock.paused = !clock.paused;
        AppState.pausedRec = !AppState.pausedRec;
        if (clock.paused)
        {
            audioInput.Dispose();
        }
        else
        {
            audioInput = recordMicrophone ? new AudioInput(recorder, clock, microphoneSource, true) : null;

        }
    }
}
