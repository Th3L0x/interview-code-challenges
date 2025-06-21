namespace OneBeyondApi.Model
{
    public class ReserveBookParameter
    {
        public Guid BookId { get; set; }
        public Borrower? Reserver { get; set; }
        public int LoanIntervalInDays { get; set; }
    }
}
