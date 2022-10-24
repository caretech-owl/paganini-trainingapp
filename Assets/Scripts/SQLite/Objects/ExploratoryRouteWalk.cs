using SQLite4Unity3d;
using static Way;

public class ExploratoryRouteWalk
{

	[PrimaryKey]
	public int Id { set; get; }
	public int Way_id { set; get; }
	public string Name{ set; get; }
	public System.DateTime Date { set; get; }
	public int Pin { set; get; }
    public int Status { set; get; }


    public override string ToString()
	{
		return string.Format("[exploratory_route_walk: erw_id={0}, way_id={1}, erw_name={2},  erw_datum={3}, erw_pin={4}, erw_status=(5)]", Id,Way_id,Name,Date,Pin, Status);
	}

	public ExploratoryRouteWalk() { }
    public ExploratoryRouteWalk(ExploratoryRouteWalkAPI erw)
	{
		this.Id = erw.erw_id;
		this.Way_id = erw.way_id;
		this.Name = erw.erw_name;
		this.Date = System.DateTime.Parse(erw.erw_datum);
		this.Pin = erw.erw_pin;
        this.Status = (int)WayStatus.FromAPI;
    }

	public ExploratoryRouteWalkAPI ToAPI()
	{
        ExploratoryRouteWalkAPI erw = new ExploratoryRouteWalkAPI
        {
            erw_id = this.Id,
            way_id = this.Way_id,
            erw_name = this.Name,
            erw_datum = this.Date.Year + "-" + this.Date.Month + "-" + this.Date.Day,
            erw_pin = this.Pin,
        };
        return erw;
	}
}