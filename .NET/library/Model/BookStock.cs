using System.Linq.Expressions;

namespace OneBeyondApi.Model
{
    public class BookStock
    {
        public Guid Id { get; set; }
        public Book Book { get; set; }
        public DateTime? LoanEndDate { get; set; }
        public Borrower? OnLoanTo { get; set; }

        public Expression<Func<BookStock, BorrowerOnLoan>> EntityToDTO()
        {
            return (x) => new BorrowerOnLoan()
            {
                BookTitle = this.Book.Name,
                OnLoanTo = this.OnLoanTo,
            };
        }
    }
}
