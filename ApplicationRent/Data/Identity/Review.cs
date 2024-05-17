namespace ApplicationRent.Data.Identity
{
    public class Review
    {
        public int Id { get; set; }
        public int PlaceId { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }

        public Place Place { get; set; }
    }
}
