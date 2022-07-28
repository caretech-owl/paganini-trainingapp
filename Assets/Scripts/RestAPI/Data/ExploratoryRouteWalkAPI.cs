[System.Serializable]
public class ExploratoryRouteWalkAPI
{
	public int erw_id;
	public int way_id;
    public string erw_name;
	public string erw_datum;
	public int erw_pin;
}

public class ExploratoryRouteWalkAPIList
{
    public ExploratoryRouteWalkAPI[] erw;
}