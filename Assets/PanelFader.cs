using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelFader : MonoBehaviour
{
    bool fadeIn;
    bool fadeOut;
    float fadeSpeed;
    float initVerticalPosition;
    // Start is called before the first frame update
    void Start()
    {
        fadeIn = false;
        fadeOut = false;
        fadeSpeed = 1;

        initVerticalPosition = this.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeIn)
        {
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - fadeSpeed, this.transform.position.z);
         
            if (this.transform.position.y <= initVerticalPosition - 30)
            {
                fadeIn = false;
                fadeOut = true;
            }
                
        }

        if (fadeOut)
        {
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + fadeSpeed, this.transform.position.z);

            if (this.transform.position.y >= initVerticalPosition + 30)
            {
                fadeOut = false;
            }

        }

        //Debug.Log("PanelFader => initVerticalPosition: " + initVerticalPosition);


    }

    public void FadeIn()
    {
        fadeIn = true;
    }
}
