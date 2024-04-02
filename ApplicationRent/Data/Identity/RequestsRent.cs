namespace ApplicationRent.Data.Identity
{
    public class RequestsRent
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int PlaceId { get; set; }
        public DateTime StartRent { get; set; }
        public DateTime EndRent { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public bool Status { get; set; } = false;

        public ApplicationIdentityUser User { get; set; }
        public Place Place { get; set; }
    }
}
