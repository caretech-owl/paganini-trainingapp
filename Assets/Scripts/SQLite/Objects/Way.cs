using SQLite4Unity3d;

public class Way
{

	[PrimaryKey]
	public int Id { set; get; }
	public int Start { set; get; }
	public int Destination { set; get; }
	public string Name { set; get; }
	public string Description { set; get; }

	public override string ToString()
	{
		return string.Format("[Weg: way_id={0}, way_start={1}, way_destination={2}, way_name={3},  way_description={4}]", Id, Start, Destination,Name,Description);
	}

	public Way() { }
	public Way(WayAPI way)
	{
		this.Id = way.way_id;
		this.Start = way.way_start;
		this.Destination = way.way_destination;
		this.Name = way.way_name;
		this.Description = way.way_description;
	}

	public WayAPI ToAPI()
	{
        WayAPI way = new WayAPI
        {
            way_id = this.Id,
            way_start = this.Start,
            way_destination = this.Destination,
            way_name = this.Name,
            way_description = this.Description
        };
        return way;
	}
}