[System.Serializable]
public class BegehungAPI
{
	public int beg_id;
	public string beg_name;
	public string beg_datum;
	public string beg_videopath;
}

public class BegehungAPIList
{
	public BegehungAPI[] begehungen;
}