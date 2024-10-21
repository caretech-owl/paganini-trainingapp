using System;
using SQLite4Unity3d;
using UnityEngine.Profiling;

public class SocialWorker : BaseModel<SocialWorker>
{
    [PrimaryKey]
    public int Id { set; get; }
    public string Username { set; get; }
    public string Firstname { set; get; }
    public string Surname { set; get; }
    [Ignore]
    public Workshop AtWorkshop { set; get; }

    public byte[] ProfilePic { set; get; }

    public override string ToString()
    {
        return string.Format("[SW: socialw_id={0}, socialw_username={1} ]", Id, Username);
    }

    public SocialWorker() { }
    public SocialWorker(ISocialWorkerAPI profile)
    {
        Id = profile.socialw_id;
        Username = profile.socialw_username;
        Firstname = profile.socialw_firstname;
        Surname = profile.socialw_sirname;
    
        AtWorkshop = new Workshop(profile.socialw_workshop);

        ProfilePic = PictureUtils.ConvertBase64ToByteArray(profile.socialw_photo);
    }

    public SocialWorkerAPI ToAPI()
    {
        SocialWorkerAPI user = new SocialWorkerAPI
        {
            socialw_id = Id,
            socialw_username = Username,
            socialw_firstname = Firstname,
            socialw_sirname = Surname
        };

        user.socialw_photo = PictureUtils.ConvertByteArrayToBase64(ProfilePic);

        return user;
    }


}
