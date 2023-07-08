using UnityEngine;

//TODO: To be a model, stored in the local storage

public class TextToSpeechUtilsConfig
{

    public string Language = "en-US";

    public float Pitch = 1.5f;

    public float SpeechRate = 1.5f;
}

public class Text2SpeechUtils : PersistentLazySingleton<Text2SpeechUtils>
{

    public TextToSpeechUtilsConfig Config = new ();

    private AndroidJavaObject textToSpeech;

    public void Initialise()
    {
        // Create an instance of the Android TextToSpeech class
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        textToSpeech = new AndroidJavaObject("android.speech.tts.TextToSpeech", currentActivity, new TextToSpeechListener());

        // Set the language
        textToSpeech.Call("setLanguage", new AndroidJavaObject("java.util.Locale", Config.Language));

        // Set the pitch (range: 0.0 to 2.0, default: 1.0)
        textToSpeech.Call("setPitch", Config.Pitch);

        // Set the speech rate (range: 0.0 to 2.0, default: 1.0)        
        textToSpeech.Call("setSpeechRate", Config.SpeechRate);
    }

    public void Speak(string text)
    {
        // Speak the provided text
        textToSpeech.Call("speak", text, 0, null);
    }

    private void OnDestroy()
    {
        // Release the TextToSpeech resources
        textToSpeech.Call("shutdown");        
    }

    private class TextToSpeechListener : AndroidJavaProxy
    {
        public TextToSpeechListener() : base("android.speech.tts.TextToSpeech$OnInitListener")
        {
        }

        public void onInit(int status)
        {
            if (status != -1)
            {
                Debug.Log("TextToSpeech initialized successfully.");
            }
            else
            {
                Debug.LogError("Failed to initialize TextToSpeech.");
            }
        }
    }
}
