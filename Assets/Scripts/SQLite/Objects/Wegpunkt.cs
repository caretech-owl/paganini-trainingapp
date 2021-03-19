using SQLite4Unity3d;
public class Wegpunkt
{

	[PrimaryKey,AutoIncrement]
	public int wegp_id { set; get; }
	public int beg_id { set; get; }
	public float wegp_longitude { set; get; }
	public float wegp_latitude { set; get; }
	public float wegp_altitude { set; get; }
	public float wegp_accuracy { set; get; }
	public int wegp_POIType { set; get; }
	public long wegp_timestamp { set; get; }
	public int wegp_lernstand { set; get; }

	public override string ToString()
	{
		return string.Format("[Wegpunkt: wegp_id={0}, beg_id={1}, wegp_longitude={2},  wegp_latitude={3},  wegp_altitude={4},  wegp_accuracy={5},  wegp_POIType={6}, wegp_timestamp={7},  wegp_lernstand={8}]", wegp_id,beg_id,wegp_longitude,wegp_latitude,wegp_altitude,wegp_accuracy,wegp_POIType, wegp_timestamp,wegp_lernstand);
	}

	public Wegpunkt() { }
	public Wegpunkt(WegpunktAPI wegpunkt)
	{
		this.wegp_id = wegpunkt.wegp_id;
		this.beg_id = wegpunkt.beg_id;
		this.wegp_longitude = wegpunkt.wegp_longitude;
		this.wegp_latitude = wegpunkt.wegp_latitude;
		this.wegp_altitude = wegpunkt.wegp_altitude;
		this.wegp_accuracy = wegpunkt.wegp_accuracy;
		this.wegp_POIType = wegpunkt.wegp_POIType;
		this.wegp_timestamp = wegpunkt.wegp_timestamp;
		this.wegp_lernstand = wegpunkt.wegp_lernstand;
	}

	public WegpunktAPI toAPI()
    {
		WegpunktAPI wegpunkt = new WegpunktAPI();
		wegpunkt.wegp_id = this.wegp_id;
		wegpunkt.beg_id = this.beg_id;
		wegpunkt.wegp_longitude = this.wegp_longitude;
		wegpunkt.wegp_latitude = this.wegp_latitude;
		wegpunkt.wegp_altitude = this.wegp_altitude;
		wegpunkt.wegp_accuracy = this.wegp_accuracy;
		wegpunkt.wegp_POIType = this.wegp_POIType;
		wegpunkt.wegp_timestamp = this.wegp_timestamp;
		wegpunkt.wegp_lernstand = this.wegp_lernstand;
		return wegpunkt;
	}
}