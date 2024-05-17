namespace ApplicationRent.Data.Identity
{
    public class TransactionHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }

        public ApplicationIdentityUser User { get; set; }
    }
}
