using SQLite4Unity3d;

public class Begehung
{

	[PrimaryKey]
	public int beg_id { set; get; }
	public string beg_name{ set; get; }
	public System.DateTime beg_datum { set; get; }
	public  string beg_videopath { set; get; }

	public override string ToString()
	{
		return string.Format("[Begehung: beg_id={0}, beg_name={1},  beg_datum={2},  beg_videopath={3}",beg_id,beg_name,beg_datum,beg_videopath);
	}

	public Begehung() { }
	public Begehung(BegehungAPI begehung)
	{
		this.beg_id = begehung.beg_id;
		this.beg_name = begehung.beg_name;
		this.beg_datum = System.DateTime.Parse(begehung.beg_datum);
		this.beg_videopath = begehung.beg_videopath;
	}

	public BegehungAPI toAPI()
	{
		BegehungAPI begehung = new BegehungAPI();
		begehung.beg_id = this.beg_id;
		begehung.beg_name = this.beg_name;
		begehung.beg_datum = this.beg_datum.Year+"-"+this.beg_datum.Month+"-"+this.beg_datum.Day;
		begehung.beg_videopath = this.beg_videopath;
		return begehung;
	}
}