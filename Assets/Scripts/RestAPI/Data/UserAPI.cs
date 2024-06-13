[System.Serializable]
public class UserAPI
{
    public int user_id;
    public string user_mnemonic_token;
    public string user_username;
    public string user_apitoken;
    public bool user_righthanded;
    public bool user_canread;
    public bool user_activatetts;
    public bool user_vibration;
    public string user_contact;
}

public class UserAPIList
{
    public UserAPI[] users;
}
