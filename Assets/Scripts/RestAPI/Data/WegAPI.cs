[System.Serializable]
public class WegAPI
{
	public int weg_id;
	public int start;
	public int ziel;
	public string weg_name;
	public string weg_beschreibung;
}

public class WegAPIList
{
	public WegAPI[] wege;
}