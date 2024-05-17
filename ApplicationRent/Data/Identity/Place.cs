using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace ApplicationRent.Data.Identity
{
    [Table("Place", Schema = "data")]
    public class Place
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartRent { get; set; } = new DateTime(2024, 4, 12);
        public DateTime EndRent { get; set; } = new DateTime(2024, 4, 13);
        public bool InRent { get; set; } = false;
        public decimal Price { get; set; }
        public double SizePlace { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string? ImageFileName1 { get; set; }
        public string? ImageFileName2 { get; set; }
        public string? ImageFileName3 { get; set; }

        // Добавляем связь с отзывами
        public ICollection<Review> Reviews { get; set; }
    }
}
