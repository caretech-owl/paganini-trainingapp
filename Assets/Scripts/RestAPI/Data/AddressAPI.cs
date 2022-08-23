[System.Serializable]
public class AddressAPI
{
	public int adr_id;
	public int user_id;
	public string adr_streetname;
	public string adr_housenumber;
	public int adr_zipcode;
	public string adr_city;
	public string adr_icon;
	public string adr_name;
}

public class AddressAPIList
{
	public AddressAPI[] adressen;
}