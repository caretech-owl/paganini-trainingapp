using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SQLite4Unity3d;
using static RouteWalk;


public class RouteWalk : BaseModel<RouteWalk>
{

    [PrimaryKey]
    public int Id { set; get; }
    public System.DateTime StartDateTime { set; get; }
    public System.DateTime EndDateTime { set; get; }
    public int RouteId { set; get; }
    public RouteWalkCompletion WalkCompletion { set; get; }
    public double WalkCompletionPercentage { set; get; }

    // Completion percentage
    // number of points reached (total pathpoints walked over, we can use closest segment index)

    public enum RouteWalkCompletion
    {
        None = 0,
        Completed = 1,
        Cancelled = 2       
    }

    public RouteWalk() { }


    public RouteWalk(IRouteWalkAPI routeWalkAPI)
    {
        Id = routeWalkAPI.rw_id;
        StartDateTime = (DateTime)DateUtils.ConvertUTCStringToUTCDate(routeWalkAPI.rw_start_time, "yyyy-MM-dd'T'HH:mm:ss");
        EndDateTime = (DateTime)DateUtils.ConvertUTCStringToUTCDate(routeWalkAPI.rw_end_time, "yyyy-MM-dd'T'HH:mm:ss");
        RouteId = routeWalkAPI.erw_id;
        WalkCompletion = (RouteWalkCompletion)routeWalkAPI.rw_complete;
        WalkCompletionPercentage = routeWalkAPI.rw_complete_percentage;
    }

    public RouteWalkAPI ToAPI()
    {
        RouteWalkAPI routeWalk = new RouteWalkAPI();

        routeWalk.rw_start_time = DateUtils.ConvertUTCDateToUTCString(StartDateTime, "yyyy-MM-dd'T'HH:mm:ss");
        routeWalk.rw_end_time = DateUtils.ConvertUTCDateToUTCString(EndDateTime, "yyyy-MM-dd'T'HH:mm:ss");
        routeWalk.erw_id = RouteId;
        routeWalk.rw_complete = (int)WalkCompletion;
        routeWalk.rw_complete_percentage = WalkCompletionPercentage;

        return routeWalk;
    }

}