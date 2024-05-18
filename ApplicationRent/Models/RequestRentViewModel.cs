using ApplicationRent.Data.Identity;

namespace ApplicationRent.Models
{
    /*public class RequestRentViewModel
    {
        public int Id { get; set; }
        public string PlaceName { get; set; }
        public DateTime StartRent { get; set; }
        public DateTime EndRent { get; set; }
        public string UserName { get; set; }
        public bool Status { get; set; }
    }*/
    public class RequestRentViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public DateTime StartRent { get; set; }
        public DateTime EndRent { get; set; }
        public bool Status { get; set; }
        public Place Place { get; set; }
    }
}
