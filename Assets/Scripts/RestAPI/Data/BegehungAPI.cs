[System.Serializable]
public class BegehungAPI
{
	public int beg_id;
	public int weg_id;
	public string beg_name;
	public string beg_datum;
	public int beg_pin;
}

public class BegehungAPIList
{
	public BegehungAPI[] begehungen;
}