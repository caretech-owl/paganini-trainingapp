using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using SQLite4Unity3d;


public class InstructionMode : BaseModel<InstructionMode>
{

    [PrimaryKey]
    public int Id { set; get; }
    public System.DateTime ActiveSinceDateTime { set; get; }
    public bool IsNewToUser { set; get; }
    public int PathpointId { set; get; }     
    public SupportMode Mode { set; get; }

    // Completion percentage
    // number of points reached (total pathpoints walked over, we can use closest segment index)

    public enum SupportMode
    {
        None = 0,
        Instruction = 1,
        TriviaDecision = 2,
        TriviaGoto = 3,
        ChallengeDecision = 4,
        ChallengeGoto = 5
    }

    //public RouteWalk() { }

    public InstructionMode() { }

    public InstructionMode(IInstructionModeAPI instructionModeAPI)
    {
        Id = instructionModeAPI.im_id;
        ActiveSinceDateTime = (DateTime)DateUtils.ConvertUTCStringToUTCDate(instructionModeAPI.im_date, "yyyy-MM-dd'T'HH:mm:ss"); 
        IsNewToUser = instructionModeAPI.im_new;
        PathpointId = instructionModeAPI.ppoint_id;
        Mode = (SupportMode)instructionModeAPI.im_mode;
    }

 

}