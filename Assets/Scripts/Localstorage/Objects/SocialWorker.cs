using System.Collections.Generic;
using SQLite4Unity3d;

public class SocialWorker : BaseModel<User>
{
    [PrimaryKey]
    public int Id { set; get; }
    public string Username { set; get; }
    public string Firstname { set; get; }
    public string Surname { set; get; }
    public string PhotoURL { set; get; }


    /*
 "socialw_id": 5,
  "socialw_username": "test",
  "socialw_firstname": "tester",
  "socialw_sirname": "test",
  "socialw_photo": null,
  "works_name": "Institut für Inteligente Systeme",
  "works_street": "Artilleriestraße 9",
  "works_city": "Minden",
  "works_zip": 32427
     */


    public override string ToString()
    {
        return string.Format("[SW: socialw_id={0}, socialw_username={1} ]", Id, Username);
    }

    public SocialWorker() { }
    public SocialWorker(SocialWorkerAPI profil)
    {
        Id = profil.socialw_id;      
        Username = profil.socialw_username;
        Firstname = profil.socialw_firstname;
        Surname = profil.socialw_sirname;
        PhotoURL = profil.socialw_photo;       
    }

    public SocialWorkerAPI ToAPI()
    {
        SocialWorkerAPI user = new SocialWorkerAPI
        {
            socialw_id = Id,
            socialw_username = Username,
            socialw_firstname = Firstname,
            socialw_sirname = Surname,
            socialw_photo = PhotoURL
        };

        return user;
    }

}