using SQLite4Unity3d;
public class UserProfile
{

	[PrimaryKey]
	public int ben_id { set; get; }
    public string ben_benutzername { set; get; }
    public string ben_mnemonic_token { set; get; }

	public override string ToString()
	{
		return string.Format("[UserProfile: ben_id={0}, ben_benutzername={1},  ben_mnemonic_token={2}]", ben_id, ben_benutzername, ben_mnemonic_token);
	}

	public UserProfile() { }
	public UserProfile(UserProfileAPI profil)
    {
		ben_id = profil.ben_id;
		ben_benutzername = profil.ben_benutzername;
		ben_mnemonic_token = profil.ben_mnemonic_token;
    }
}