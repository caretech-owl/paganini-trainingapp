using SQLite4Unity3d;

public class Adresse
{

	[PrimaryKey]
	public int adr_id { set; get; }
	public string adr_strasse { set; get; }
	public string adr_hausnr { set; get; }
	public int adr_plz { set; get; }
	public string adr_ort { set; get; }
	public int adr_icon { set; get; }

	public override string ToString()
	{
		return string.Format("[Adresse: adr_id={0}, adr_strasse={1}, adr_hausnr={2}, adr_plz={3},  adr_ort={4}, adr_icon={5}]", adr_id, adr_strasse,adr_hausnr,adr_plz,adr_ort,adr_icon);
	}

	public Adresse() { }
	public Adresse(AdresseAPI adresse)
	{
		this.adr_id = adresse.adr_id;
		this.adr_strasse = adresse.adr_strasse;
		this.adr_hausnr = adresse.adr_hausnr;
		this.adr_plz = adresse.adr_plz;
		this.adr_ort = adresse.adr_ort;
		this.adr_icon = adresse.adr_icon;
	}

	public AdresseAPI toAPI()
	{
		AdresseAPI adresse = new AdresseAPI();
		adresse.adr_id = this.adr_id;
		adresse.adr_strasse = this.adr_strasse;
		adresse.adr_hausnr = this.adr_hausnr;
		adresse.adr_plz = this.adr_plz;
		adresse.adr_ort = this.adr_ort;
		adresse.adr_icon = this.adr_icon;
		return adresse;
	}
}