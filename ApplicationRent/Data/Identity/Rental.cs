namespace ApplicationRent.Data.Identity
{
    public class Rental
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int PlaceId { get; set; }
        public DateTime StartRent { get; set; }
        public DateTime EndRent { get; set; }

        public ApplicationIdentityUser User { get; set; }
        public Place Place { get; set; }
    }
}
