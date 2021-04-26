using SQLite4Unity3d;

public class Begehung
{

	[PrimaryKey]
	public int beg_id { set; get; }
	public int weg_id { set; get; }
	public string beg_name{ set; get; }
	public System.DateTime beg_datum { set; get; }
	public int beg_pin { set; get; }

	public override string ToString()
	{
		return string.Format("[Begehung: beg_id={0}, weg_id={1}, beg_name={2},  beg_datum={3}, beg_pin={4}]", beg_id,weg_id,beg_name,beg_datum,beg_pin);
	}

	public Begehung() { }
	public Begehung(BegehungAPI begehung)
	{
		this.beg_id = begehung.beg_id;
		this.weg_id = begehung.weg_id;
		this.beg_name = begehung.beg_name;
		this.beg_datum = System.DateTime.Parse(begehung.beg_datum);
		this.beg_pin = begehung.beg_pin;
	}

	public BegehungAPI toAPI()
	{
		BegehungAPI begehung = new BegehungAPI();
		begehung.beg_id = this.beg_id;
		begehung.weg_id = this.weg_id;
		begehung.beg_name = this.beg_name;
		begehung.beg_datum = this.beg_datum.Year+"-"+this.beg_datum.Month+"-"+this.beg_datum.Day;
		begehung.beg_pin = this.beg_pin;
		return begehung;
	}
}