namespace OneBeyondApi.Model
{
    public class BorrowerFine
    {
        public Guid Id { get; set; }
        public double FineAmount { get; set; }
        public Guid FineToBorrowerId { get; set; }
        public Borrower? FineToBorrower { get; set; }
        public DateTime OnFineIssued { get; set; } = DateTime.UtcNow;
        public DateTime? OnFineResolved { get; set; }
        public bool IsFineResolved { get; set; } = false;
    }
}
