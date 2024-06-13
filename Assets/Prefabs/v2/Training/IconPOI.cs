using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconPOI : MonoBehaviour
{
    public LandmarkIcon WayIconChoice;

    [Header(@"Way Icon")]
    public GameObject WayIcon;
    public GameObject DecisionIcon;       
    public GameObject SafetyIcon;
    public GameObject QuizIcon;
    public GameObject ChallengeIcon;

    [Header(@"Decision Signs")]
    public GameObject TurnLeftIcon;
    public GameObject TurnRightIcon;
    public GameObject StraightIcon;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RenderIcon(Pathpoint poi, Way w)
    {
        var poiType = poi.POIType;
        if (poiType == Pathpoint.POIsType.WayStart)
        {
            RenderOnly(WayIcon);
            WayIconChoice.SetSelectedLandmark(w.StartType);
        }
        else if (poiType == Pathpoint.POIsType.WayDestination)
        {
            RenderOnly(WayIcon);
            WayIconChoice.SetSelectedLandmark(w.DestinationType);
        }
        else if (poiType == Pathpoint.POIsType.Landmark)
        {
            RenderOnly(DecisionIcon);
            RenderDecisionPoint(poi.Instruction);
        }
        else if (poiType == Pathpoint.POIsType.Reassurance)
        {
            RenderOnly(SafetyIcon);
        }
    }

    public void RenderQuiz()
    {
        RenderOnly(QuizIcon);
    }

    public void RenderChallenge()
    {
        RenderOnly(ChallengeIcon);
    }

    private void RenderOnly(GameObject panel)
    {
        WayIcon.SetActive(panel == WayIcon);
        DecisionIcon.SetActive(panel == DecisionIcon);
        SafetyIcon.SetActive(panel == SafetyIcon);
        QuizIcon.SetActive(panel == QuizIcon);
        ChallengeIcon.SetActive(panel == ChallengeIcon);
    }


    private void RenderDecisionPoint(Pathpoint.NavDirection navDirection)
    {
        TurnLeftIcon.SetActive(Pathpoint.NavDirection.LeftTurn == navDirection);
        TurnRightIcon.SetActive(Pathpoint.NavDirection.RightTurn == navDirection);
        StraightIcon.SetActive(Pathpoint.NavDirection.Straight == navDirection);
    }

}
