using UnityEngine;

public static class AppState
{

    public static User currentUser=null;



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

    // Scenes
    public static string UserLoginScene = "UserLogin";
    public static string MyExploratoryRouteWalkScenes = "MyExploratoryRouteWalks";
    public static string ExploratoryRouteWalkRecodingScene = "ExploratoryRouteWalkRecording";

    public static string startScene = "01Start";
    public static string allOkScene = "02AllOk";
    public static string wegeFührungScene = "03Wegeführung";
    public static string pausedScene = "04Paused";
    public static string goalReachedScene = "05ZielErreicht";
    public static string videoHelpScene = "06VideoHelp";
    public static string settingsScene = "07Settings";
    public static string profileScene = "08ShowProfile";
    public static string overviewScene = "09Overview";
    // Current State

    public static string currentScene = "";
    public static string lastScene = "";
    public static string currentBegehung = "";

    public static int screenSleepTimeout = Screen.sleepTimeout;

}