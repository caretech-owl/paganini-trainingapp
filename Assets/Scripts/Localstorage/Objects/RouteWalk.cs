using System.Collections.Generic;
using System.Data;
using System.Linq;
using SQLite4Unity3d;


public class RouteWalk : BaseModel<RouteWalk>
{

    [PrimaryKey]
    public int Id { set; get; }
    public System.DateTime StartDateTime { set; get; }
    public System.DateTime EndDateTime { set; get; }
    public int RouteId { set; get; }

    public RouteWalk() { }

}