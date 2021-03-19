using SQLite4Unity3d;

public class Weg
{

	[PrimaryKey]
	public int weg_id { set; get; }
	public int start { set; get; }
	public int ziel { set; get; }
	public string weg_name { set; get; }
	public string weg_beschreibung { set; get; }

	public override string ToString()
	{
		return string.Format("[Weg: weg_id={0}, start={1},ziele={2}, weg_name={3},  weg_beschreibung={4}]", weg_id, start, ziel,weg_name,weg_beschreibung);
	}

	public Weg() { }
	public Weg(WegAPI weg)
	{
		this.weg_id = weg.weg_id;
		this.start = weg.start;
		this.ziel = weg.ziel;
		this.weg_name = weg.weg_name;
		this.weg_beschreibung = weg.weg_beschreibung;
	}

	public WegAPI toAPI()
	{
		WegAPI weg = new WegAPI();
		weg.weg_id = this.weg_id;
		weg.start = this.start;
		weg.ziel = this.ziel;
		weg.weg_name = this.weg_name;
		weg.weg_beschreibung = this.weg_beschreibung;
		return weg;
	}
}