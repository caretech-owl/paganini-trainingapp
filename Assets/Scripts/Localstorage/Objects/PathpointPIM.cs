using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using SQLite4Unity3d;


public class PathpointPIM : BaseModel<PathpointPIM>
{

    [PrimaryKey]
    public int Id { set; get; }
    public System.DateTime ActiveSinceDateTime { set; get; }
    public bool IsAtPOINewToUser { set; get; }
    public bool IsToPOINewToUser { set; get; }
    public int PathpointId { set; get; }     
    public SupportMode AtPOIMode { set; get; }
    public SupportMode ToPOIMode { set; get; }


    public enum SupportMode
    {
        None = 0,
        Instruction = 1,
        Trivia = 2,
        Challenge = 3,
        Mute = 4
    }


    public PathpointPIM() { }

    public PathpointPIM(PathpointPIMAPI instructionModeAPI)
    {
        Id = instructionModeAPI.pim_id;
        ActiveSinceDateTime = (DateTime)DateUtils.ConvertUTCStringToUTCDate(instructionModeAPI.pim_timestamp, "yyyy-MM-dd'T'HH:mm:ss"); 
        IsAtPOINewToUser = instructionModeAPI.pim_atpoi_isnew;
        IsToPOINewToUser = instructionModeAPI.pim_topoi_isnew;
        PathpointId = instructionModeAPI.ppoint_id;
        AtPOIMode = (SupportMode)instructionModeAPI.pim_atpoi_mode;
        ToPOIMode = (SupportMode)instructionModeAPI.pim_topoi_mode;
    }

 

}