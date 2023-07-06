using UnityEngine;

public static class AppState
{

    public static User CurrentUser=null;

    public static string APIToken = null;


    //Erstbegehung
    public static int SelectedWeg = -1;
    public static int SelectedBegehung = -1;
    public static bool recording = false;
    public static bool pausedRec=false;
    public static bool wrapBegehung = false;
    public static int poiType = -1;
    public static string picturePath = "";
    public static float connectionCheckIntervall = 3.0f;

    public static bool isMute = false;

    public static bool isIncognito = false;


    // Current State

    public static string currentScene = "";
    public static string lastScene = "";
    public static string currentBegehung = "";

    public static int screenSleepTimeout = Screen.sleepTimeout;

}