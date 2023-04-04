using SQLite4Unity3d;
public class Pathpoint
{

	[PrimaryKey,AutoIncrement]
	public int Id { set; get; }
	public int RouteId { set; get; }
	public double Longitude { set; get; }
	public double Latitude { set; get; }
	public double Altitude { set; get; }
	public double Accuracy { set; get; }
	public int POIType { set; get; }
	public long Timestamp { set; get; }
	public string Description { set; get; }
    public string PhotoFilename { set; get; }


    public override string ToString()
	{
		return string.Format("[pathpoint: ppoint_id={0}, erw_id={1}, ppoint_lon={2},  ppoint_lat={3},  ppoint_altitude={4},  ppoint_accuracy={5},  ppoint_poitype={6}, ppoint_timestamp={7},  ppoint_description={8}]", Id, RouteId, Longitude,Latitude,Altitude,Accuracy,POIType, Timestamp,Description);
	}

	public Pathpoint() { }
	public Pathpoint(PathpointAPI pathpoint)
	{
		this.Id = pathpoint.ppoint_id;
		this.RouteId = pathpoint.erw_id;
		this.Longitude = pathpoint.ppoint_lon;
		this.Latitude = pathpoint.ppoint_lat;
		this.Altitude = pathpoint.ppoint_altitude;
		this.Accuracy = pathpoint.ppoint_accuracy;
		this.POIType = pathpoint.ppoint_poitype;
		this.Timestamp = pathpoint.ppoint_timestamp;
		this.Description = pathpoint.ppoint_description;
	}

	public PathpointAPI ToAPI()
    {
        PathpointAPI wegpunkt = new PathpointAPI
        {
            ppoint_id = this.Id,
            erw_id = this.RouteId,
            ppoint_lon = this.Longitude,
            ppoint_lat = this.Latitude,
            ppoint_altitude = this.Altitude,
            ppoint_accuracy = this.Accuracy,
            ppoint_poitype = this.POIType,
            ppoint_timestamp = this.Timestamp,
            ppoint_description = this.Description
        };
        return wegpunkt;
	}
}