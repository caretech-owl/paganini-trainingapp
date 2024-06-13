using UnityEngine;


public static class CallManager 
{
    public static void MakePhoneCall(string phoneNumber)
    {
        string dialerString = "tel:" + phoneNumber;
        Application.OpenURL(dialerString);
    }

}