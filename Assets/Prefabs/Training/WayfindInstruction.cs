using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PaganiniRestAPI;

public class WayfindInstruction : MonoBehaviour
{

    public PhotoSlideShow SlideShow;
    public PhotoSlideShow StartSlideShow;

    public GameObject StartPanel;
    public GameObject StartButton;

    [Header("Wayfind Instruction UI")]
    [SerializeField] private GameObject InstructionPanel;
    [SerializeField] private TMPro.TMP_Text InstructionText;
    [SerializeField] private DirectionIcon DirectionSymbol;

    [Header("Off Track UI")]
    [SerializeField] private GameObject OffTrackPanel;
    [SerializeField] private TMPro.TMP_Text OffTrackReasonText;

    [Header("Arrived UI")]
    public GameObject ArrivedPanel;

    private Pathpoint POI;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadStart(Pathpoint pathtpoint)
    {
        POI = pathtpoint;
        StartPanel.SetActive(true);
        InstructionPanel.SetActive(false);
        OffTrackPanel.SetActive(false);
        StartButton.SetActive(false);

        StartSlideShow.LoadSlideShow(pathtpoint.Photos);
    }

    public void LoadInstruction(Pathpoint pathtpoint)
    {
        POI = pathtpoint;
        InstructionPanel.SetActive(true);
        DirectionSymbol.gameObject.SetActive(false);

        StartPanel.SetActive(false);        
        OffTrackPanel.SetActive(false);
        
        SlideShow.LoadSlideShow(pathtpoint.Photos);
        InstructionText.text = "Walk until you see this place.";
    }

    public void LoadInstructionDirection(bool withPhotos = false)
    {
        if (withPhotos) {
            SlideShow.LoadSlideShow(POI.Photos);
        }
        InstructionPanel.SetActive(true);
        DirectionSymbol.gameObject.SetActive(false);

        StartPanel.SetActive(false);
        OffTrackPanel.SetActive(false);

        DirectionIcon.DirectionType directionType;
        if (POI == null)
        {
            Debug.Log("POI is null!!");
            return;
        }
        if (Enum.TryParse(POI.Instruction.ToString(), out directionType))
        {
            DirectionSymbol.gameObject.SetActive(true);
            DirectionSymbol.RenderDirection(directionType);
            InstructionText.text = directionType.GetDescription();
        }

    }

    public void LoadOffTrack(string reason)
    {
        StartPanel.SetActive(false);
        InstructionPanel.SetActive(false);
        OffTrackPanel.SetActive(true);
        if (reason != null)
        {
            OffTrackReasonText.text = reason;
        }
    }

    public void LoadOnTrack()
    {
        StartPanel.SetActive(false);
        InstructionPanel.SetActive(true);
        OffTrackPanel.SetActive(false);
    }

    public void LoadArrived()
    {
        InstructionPanel.SetActive(false);
        ArrivedPanel.SetActive(true);
    }

    public void EnableStartTraining()
    {
        StartButton.SetActive(true);
    }


}
