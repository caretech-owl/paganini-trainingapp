﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class FormRequirement
{
    public enum FormRequirementType { Optional, Required};
    public enum FieldType { Text, ToggleGroup };
    
    public Object Field;
    public FieldType Type;
    public FormRequirementType RequirementType;

    public bool IsRequirementValid()
    {

        Debug.Log("This is the type of the field: " + Field.GetType());

        if (RequirementType == FormRequirement.FormRequirementType.Optional) return true;

        bool isValid = false;
        if (Type == FieldType.Text)
        {
            TMPro.TMP_InputField item;
            if (Field is TMPro.TMP_InputField)
            {
                item = (TMPro.TMP_InputField)Field;
            }
            else
            {
                item = ((GameObject)Field).GetComponent<TMP_InputField>();
            }            
            
            isValid = item.text.Trim() != "";
        }
        else if (Type == FieldType.ToggleGroup)
        {
            ToggleGroup item = ((GameObject)Field).GetComponent<ToggleGroup>();
            isValid = item.AnyTogglesOn();
        }

        return isValid;        
    }

    public void ClearField()
    {

        if (Type == FieldType.Text)
        {
            TMPro.TMP_InputField item = (TMPro.TMP_InputField)Field;
            item.text = "";
        }
        else if (Type == FieldType.ToggleGroup)
        {
            ToggleGroup item = ((GameObject)Field).GetComponent<ToggleGroup>();
            item.SetAllTogglesOff();
        }

    }

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
            if (!item.IsRequirementValid())
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
        ClearOutFields();
        OnValidationSkipped.Invoke();
    }

    private void ClearOutFields()
    {
        foreach (FormRequirement item in Fields)
        {
            item.ClearField();
        }
    }
}
