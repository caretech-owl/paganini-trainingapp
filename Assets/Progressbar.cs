using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Progressbar : MonoBehaviour
{
    Slider slider;
    public TMP_Text percentage_txt;

    // Start is called before the first frame update
    void Start()
    {
        //Fetch the Slider from the GameObject
        slider = GetComponent<Slider>();
        ResetProgress();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetProgress()
    {
        slider.value = 0;
        percentage_txt.text = slider.value + "%";
    }
    
    public void SetProgressbar(float value)
    {
        slider.value = value;
        percentage_txt.text = ((int)Mathf.Floor(slider.value * 100)) + "%";

        Debug.Log("Slider value is" + value);
    }
}
