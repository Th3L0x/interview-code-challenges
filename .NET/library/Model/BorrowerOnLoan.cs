namespace OneBeyondApi.Model
{
    public class BorrowerOnLoan
    {
        public string BookTitle { get; set; }
        public Borrower? OnLoanTo { get; set; }
    }
}
