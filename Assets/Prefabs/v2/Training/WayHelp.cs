using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WayHelp : MonoBehaviour
{  
 

   public void CallViaPhone()
   {
        string phoneNumber = AppState.CurrentUser.Contact;
        CallManager.MakePhoneCall(phoneNumber);
   }


}
