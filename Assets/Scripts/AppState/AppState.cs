public static class AppState
{

    public static string authtoken = "";



    //Erstbegehung
    public static int SelectedWeg = -1;
    public static int SelectedBegehung = -1;
    public static bool recording = false;
    public static bool wrapBegehung = false;
    public static int poiType = -1;
    public static string picturePath = "";
    public static float connectionCheckIntervall = 3.0f;

    // Screnes
    public static string startScene = "01Start";
    public static string allOkScene = "02AllOk";
    public static string wegeFührungScene = "03Wegeführung";

    public static string pausedScene = "04Paused";

    public static string goalReachedScene = "05ZielErreicht";

    public static string videoHelpScene = "06VideoHelp";


}