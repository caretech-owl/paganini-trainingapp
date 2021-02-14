using SQLite4Unity3d;
public class AuthToken
{

	[PrimaryKey]
	public string ben_authtoken { set; get; }

	public override string ToString()
	{
		return string.Format("authToken= {0}", ben_authtoken);
	}
}