using SQLite4Unity3d;

public class Address: BaseModel<Address>
{
    [PrimaryKey]
    public int Id { set; get; }
    public string Streetname { set; get; }
    public string Housenumber { set; get; }
    public int Zipcode { set; get; }
    public string City { set; get; }
    public string Icon { set; get; }
    public string Name { set; get; }

    public override string ToString()
    {
        return string.Format("[Adresse: adr_id={0}, adr_streetname={1}, adr_housenumber={2}, adr_zipcode={3},  adr_city={4}, adr_icon={5}, adr_name={6}]", Id, Streetname, Housenumber, Zipcode, City, Icon, Name);
    }

    public Address() { }
    public Address(AddressAPI adresse)
    {
        this.Id = adresse.adr_id;
        this.Streetname = adresse.adr_streetname;
        this.Housenumber = adresse.adr_housenumber;
        this.Zipcode = adresse.adr_zipcode;
        this.City = adresse.adr_city;
        this.Icon = adresse.adr_icon;
        this.Name = adresse.adr_name;
    }

    public AddressAPI ToAPI()
    {
        AddressAPI address = new AddressAPI
        {
            adr_id = this.Id,
            user_id = AppState.CurrentUser.Id,
            adr_streetname = this.Streetname,
            adr_housenumber = this.Housenumber,
            adr_zipcode = this.Zipcode,
            adr_city = this.City,
            adr_icon = this.Icon,
            adr_name = this.Name
        };
        return address;
    }
}