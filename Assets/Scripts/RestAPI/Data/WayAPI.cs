[System.Serializable]
public class WayAPI
{
	public int way_id;
	public int way_start;
	public int way_destination;
	public string way_name;
	public string way_description;
}

public class WegAPIList
{
	public WayAPI[] wege;
}