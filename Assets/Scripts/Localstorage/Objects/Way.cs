using System.Collections.Generic;
using System.Linq;
using SQLite4Unity3d;

public class Way : BaseModel<Way>
{

    [PrimaryKey]
    public int Id { set; get; }
    public string Start { set; get; }
    public string Destination { set; get; }
    public string StartType { set; get; }
    public string DestinationType { set; get; }
    public string Name { set; get; }
    public string Description { set; get; }
    public int UserId { set; get; }

    [Ignore]
    public List<Route> Routes { get; set; }


    public override string ToString()
    {
        return string.Format("[Weg: way_id={0}, way_start={1}, way_destination={2}, "
            + "way_start_type {3}, way_destination_type={4}, way_name={5},  way_description={6}]",
            Id, Start, Destination, StartType, DestinationType, Name, Description);
    }

    public Way() { }
    public Way(WayAPIResult way)
    {
        this.Id = way.way_id;
        if(way.way_start != null)
        {
            this.Start =  way.way_start.adr_name;
            this.StartType = way.way_start.adr_icon;
        }
        if (way.way_destination != null)
        {
            this.Destination = way.way_destination.adr_name;
            this.DestinationType = way.way_destination.adr_icon;
        }        

        this.Name = way.way_name;
        this.Description = way.way_description;
        this.FromAPI = true;
    }

    public WayAPI ToAPI()
    {

        WayAPI way;

        if (!FromAPI)
        {
            way = new WayAPI();
            way.IsNew = true;
            way.way_start = new AddressAPI();
            way.way_destination = new AddressAPI();
        }
        else
        {
            way = new WayAPIUpdate();
            way.way_id = this.Id;
            way.way_start = new AddressAPIUpdate();
            way.way_destination = new AddressAPIUpdate();
        }


        way.way_start.adr_name = this.Start;
        way.way_start.adr_icon = this.StartType;

        way.way_destination.adr_name = this.Destination;
        way.way_destination.adr_icon = this.DestinationType;


        way.way_name = this.Name;
        way.way_description = this.Description;


        return way;
    }



    public static List<Way> GetWayListByUser(int userId)
    {
        List<Way> ways;

        var conn = DBConnector.Instance.GetConnection();

        // Query all Ways and their related Routes using sqlite-net's built-in mapping functionality

        ways = conn.Table<Way>().Where(w => w.UserId == userId).ToList();

        return ways;
    }
}