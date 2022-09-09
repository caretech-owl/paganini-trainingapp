using SQLite4Unity3d;

public class Way
{

	[PrimaryKey]
	public int Id { set; get; }
	public string Start { set; get; }
	public string Destination { set; get; }
    public string StartType { set; get; }
    public string DestinationType { set; get; }
    public string Name { set; get; }
	public string Description { set; get; }

	public override string ToString()
	{
		return string.Format("[Weg: way_id={0}, way_start={1}, way_destination={2}, "
			+"way_start_type {3}, way_destination_type={4}, way_name={5},  way_description={6}]",
			Id, Start, Destination, StartType, DestinationType, Name, Description);
	}

	public Way() { }
	public Way(WayAPI way)
	{
		this.Id = way.way_id;
		this.Start = way.way_start.adr_name;
		this.Destination = way.way_destination.adr_name;
        this.StartType = way.way_start.adr_icon;
        this.DestinationType = way.way_destination.adr_icon;
        this.Name = way.way_name;
		this.Description = way.way_description;
	}

	public WayAPI ToAPI()
	{
		AddressAPI addrStart = new AddressAPI
		{
			adr_name = this.Start,
			adr_icon = this.StartType
		};

        AddressAPI addrDestination = new AddressAPI
        {
            adr_name = this.Destination,
            adr_icon = this.DestinationType
        };

        WayAPI way = new WayAPI
        {
            way_id = this.Id,
            way_start = addrStart,
            way_destination = addrDestination,
            way_name = this.Name,
            way_description = this.Description
        };
        return way;
	}
}