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
	public long wegp_timestamp { set; get; }
	public bool wegp_idPOI { set; get; }

	public override string ToString()
	{
		return string.Format("[Wegpunkt: wegp_id={0}, beg_id={1}, wegp_longitude={2},  wegp_latitude={3},  wegp_altitude={4},  wegp_accuracy={5},  wegp_timestamp={6},  wegp_isPOI={7}",wegp_id,beg_id,wegp_longitude,wegp_latitude,wegp_altitude,wegp_accuracy,wegp_timestamp,wegp_idPOI);
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
		this.wegp_timestamp = wegpunkt.wegp_timestamp;
		this.wegp_idPOI = wegpunkt.wegp_idPOI;
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
		wegpunkt.wegp_timestamp = this.wegp_timestamp;
		wegpunkt.wegp_idPOI = this.wegp_idPOI;
		return wegpunkt;
	}
}