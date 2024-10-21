using SQLite4Unity3d;
using UnityEngine.Profiling;

public class Workshop : BaseModel<Workshop>
{
    [PrimaryKey]
    public int Id { set; get; }
    public string Name { set; get; }
    public string Street { set; get; }
    public string City { set; get; }
    public string OrgZip { set; get; }
    public OrganizationType OrgType { set; get; }

  public enum OrganizationType
    {
        None = 0,
        Workshop = 1,
        Residence = 2
    }

    public override string ToString()
    {
        return string.Format("[WS: WorksName={0}]", Name);
    }

    public Workshop() { }
    public Workshop(WorkshopAPI workshopData)
    {
        Id = workshopData.works_id;
        Name = workshopData.works_name;
        Street = workshopData.works_street;
        City = workshopData.works_city;
        OrgZip = workshopData.works_zip;
    }

    public WorkshopAPI ToAPI()
    {
        WorkshopAPI workshopRes = new WorkshopAPI
        {
            works_name = Name,
            works_street = Street,
            works_city = City,
            works_zip = OrgZip
        };

        return workshopRes;
    }


}
