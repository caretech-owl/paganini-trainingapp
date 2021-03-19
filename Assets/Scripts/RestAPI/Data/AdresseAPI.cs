[System.Serializable]
public class AdresseAPI
{
	public int adr_id;
	public string adr_strasse;
	public string adr_hausnr;
	public int adr_plz;
	public string adr_ort;
	public int adr_icon;
}

public class AdresseAPIList
{
	public AdresseAPI[] adressen;
}