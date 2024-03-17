using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace ApplicationRent.Data.Identity
{
    [Table("Place", Schema = "data")]
    public class Place
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartRent { get; set; }
        public DateTime EndRent { get; set; }
        public bool InRent { get; set; }
        public decimal Price { get; set; }
        public double SizePlace { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }
}
