[System.Serializable]
public class WayAPI
{
	public int way_id;
	public AddressAPI way_start;
	public AddressAPI way_destination;
	public string way_name;
	public string way_description;
	//TODO: start_address
}

public class WegAPIList
{
	public WayAPI[] wege;
}