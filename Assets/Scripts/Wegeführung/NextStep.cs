using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextStep : MonoBehaviour
{

    private GameObject direction;

    // Start is called before the first frame update
    void Start()
    {
        this.direction = gameObject.transform.Find("Anweisung").gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void GetCurrentStep()
    {

    }

    private void GetNextStep()
    {

    }

    private string GetStepText()
    {
        return "HELLO THERE!!!";
    }

    private bool IsLastStep()
    {
        return false;
    }

    private void WriteNextStep(string text)
    {

    }
}
