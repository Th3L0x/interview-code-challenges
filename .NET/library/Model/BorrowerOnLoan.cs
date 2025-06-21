namespace OneBeyondApi.Model
{
    public class BorrowerOnLoan
    {

        public Guid BookId { get; set; }
        public string BookTitle { get; set; }
        public Borrower? OnLoanTo { get; set; }
    }
}
