using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorHandler : MonoBehaviour
{
    public TMPro.TMP_Text title;
    public TMPro.TMP_Text description;

    // Start is called before the first frame update
    void Start()
    {
        // blank
        title.text = "";
        description.text = "";

        // register
        Assets.ErrorHandlerSingleton.GetErrorHandler().SetupNewErrorUI(this.gameObject);

        // hide
        this.gameObject.SetActive(false);
    }

    void Update()
    {
        var error = Assets.ErrorHandlerSingleton.GetErrorHandler().GetLastError();
       
        title.text = error.Title;
        description.text = error.Description;

    }
}
