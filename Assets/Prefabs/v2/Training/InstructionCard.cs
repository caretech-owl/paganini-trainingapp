using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics;
using System;
using UnityEngine.Events;

public class InstructionCard : MonoBehaviour
{
    [Header("Component UI")]
    public GameObject InstructionPanel;
    public GameObject ConfirmationPanel;
    public Button ButtonSolid;
    public ButtonTimed ButtonConfirmationTimed;

    [Header("Content")]
    [SerializeField] private TMPro.TMP_Text TitleText;
    [SerializeField] private TMPro.TMP_Text SubtitleText;

    [SerializeField] private TMPro.TMP_Text TitleConfirmationText;
    [SerializeField] private TMPro.TMP_Text SubtitleConfirmationText;

    [SerializeField] private SVGImage IconInstruction;
    [SerializeField] private SVGImage IconConfirmation;


    public IconPOI InstructionIconPOI;

    public UnityEvent OnCardTransition;

    // private attributes
    private Button ButtonConfirmation;


    // Start is called before the first frame update
    void Start()
    {
        ButtonConfirmation = ButtonConfirmationTimed?.gameObject.GetComponent<Button>();

        ButtonSolid?.onClick.AddListener(TriggerCardTransition);
        ButtonConfirmation?.onClick.AddListener(TriggerCardTransition);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        ButtonSolid?.onClick.RemoveListener(TriggerCardTransition);
        ButtonConfirmation?.onClick.RemoveListener(TriggerCardTransition);
    }

    public void SetCardTransitionTimer(int delaySeconds)
    {
        if (ButtonConfirmationTimed != null)
            ButtonConfirmationTimed.Timeout = delaySeconds;
    }

    public void FillInstruction(string title, string subtitle, bool useDoubleColon = false)
    {
        if (title != null)
        {
            TitleText.text = title;
        }

        if (subtitle != null)
        {
            SubtitleText.text = subtitle;
            if (useDoubleColon && subtitle.Trim() != "")
            {
                TitleText.text = TitleText.text + ":";
            }
        }
    }

    public void FillInstructionSubtitle(string title, string subtitle)
    {
        if (title != null)
        {
            TitleConfirmationText.text = title;
        }

        if (subtitle != null)
        {
            SubtitleConfirmationText.text = subtitle;
        }
    }

    public void RenderInstruction()
    {
        SwitchOnConfirmationMode(false);
    }

    public void RenderConfirmation()
    {
        SwitchOnConfirmationMode(true);
        ButtonConfirmationTimed.StartTiming();

        ResetOriginalLocation();
    }

    private void SwitchOnConfirmationMode(bool active)
    {
        ConfirmationPanel.SetActive(active);
        InstructionPanel.SetActive(!active);
    }

    private void TriggerCardTransition()
    {
        OnCardTransition.Invoke();
    }

    public void ResetOriginalLocation()
    {
        // is hidding effect available?
        var obj = gameObject.GetComponent<MoveEffect>();
        if (obj != null && !obj.IsAtOriginalPosition())
        {
            obj.TriggerMoveToOriginal();
        }
    }

}
