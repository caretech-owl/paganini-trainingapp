using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class FormRequirement
{
    public enum FormRequirementType { Optional, Required};
    public TMPro.TMP_InputField Field;
    public FormRequirementType Type;

}

public class FormValidator : MonoBehaviour
{

    public List<FormRequirement> Fields;
    public Button SubmitButton;
    public Button SkipButton;

    // TODO: Implement specific Event Types
    public UnityEvent OnValidationSuccess;
    public UnityEvent OnValidationSkipped;
    public UnityEvent OnValidationFail;

    // Start is called before the first frame update
    void Start()
    {
        SubmitButton.onClick.AddListener(HandleSubmission);

        if (SkipButton) {
            SkipButton.onClick.AddListener(HandleSkip);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleSubmission()
    {
        List<FormRequirement> failed = new List <FormRequirement> ();
        foreach (FormRequirement item in Fields)
        {
            if (item.Type == FormRequirement.FormRequirementType.Required && item.Field.text.Trim() == "")
            {
                failed.Add(item);
            }
        }

        if(failed.Capacity == 0)
        {
            OnValidationSuccess.Invoke();
        }
        else
        {
            OnValidationFail.Invoke();
        }
    }
    private void HandleSkip()
    {

        foreach (FormRequirement item in Fields)
        {
            item.Field.text = "";
        }

        OnValidationSkipped.Invoke();
    }
}
