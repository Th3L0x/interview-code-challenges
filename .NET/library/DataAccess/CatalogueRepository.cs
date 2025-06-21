using Microsoft.EntityFrameworkCore;
using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public class CatalogueRepository : ICatalogueRepository
    {
        private readonly ILogger<CatalogueRepository> _logger;

        public CatalogueRepository(ILogger<CatalogueRepository> logger)
        {
            _logger = logger;
        }

        public List<BookStock> GetCatalogue()
        {
            using (var context = new LibraryContext())
            {
                var list = context.Catalogue
                    .Include(x => x.Book)
                    .ThenInclude(x => x.Author)
                    .Include(x => x.OnLoanTo)
                    .ToList();
                return list;
            }
        }

        public List<BorrowerFine> GetFines()
        {
            using (var context = new LibraryContext())
            {
                var list = context.BorrowerFines
                    .Include(x => x.FineToBorrower)
                    .AsNoTracking()
                    .ToList();
                return list;
            }
        }

        public List<BorrowerOnLoan> GetBorrowersOnActiveLoans()
        {
            using (var context = new LibraryContext())
            {
                var result = context.Catalogue
                    .Include(x => x.Book)
                    .Include(x => x.OnLoanTo)
                    .Where(x => x.OnLoanTo != null && x.LoanEndDate != null)
                    .AsNoTracking()
                    .Select(x => new BorrowerOnLoan()
                    {
                        BookTitle = x.Book.Name,
                        OnLoanTo = x.OnLoanTo,
                    })
                    .ToList();
                return result;
            }
        }

        public bool ReturnBook(ReturnBookParameter returnBookParameter)
        {
            try
            {
                using (var context = new LibraryContext())
                {
                    DateTime now = DateTime.Now;

                    var bookStock = context.Catalogue
                        .Include(x => x.Book)
                        .Include(x => x.OnLoanTo)
                        .Where(x => x.OnLoanTo != null && x.LoanEndDate != null)
                        .FirstOrDefault(x => x.Book.Id == returnBookParameter.BookId && x.OnLoanTo!.Id == returnBookParameter.BorrowerId) 
                        ?? throw new NullReferenceException($"Could not find loan with book id: {returnBookParameter.BorrowerId} and with borrower id {returnBookParameter.BorrowerId}");

                    DateTime endDate = bookStock.LoanEndDate!.Value;
                    Guid borrowerId = bookStock.OnLoanTo!.Id;

                    bookStock.OnLoanTo = null;
                    bookStock.LoanEndDate = null;
                    int result = context.SaveChanges();

                    if(result > 0 && endDate > now) 
                    {
                        bool isFineAdded = AddFine(borrowerId);
                        //TODO raise error?
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return false;
            }
        }

        public List<BookStock> SearchCatalogue(CatalogueSearch search)
        {
            using (var context = new LibraryContext())
            {
                var list = context.Catalogue
                    .Include(x => x.Book)
                    .ThenInclude(x => x.Author)
                    .Include(x => x.OnLoanTo)
                    .AsQueryable();

                if (search != null)
                {
                    if (!string.IsNullOrEmpty(search.Author)) {
                        list = list.Where(x => x.Book.Author.Name.Contains(search.Author));
                    }
                    if (!string.IsNullOrEmpty(search.BookName)) {
                        list = list.Where(x => x.Book.Name.Contains(search.BookName));
                    }
                }
                    
                return list.ToList();
            }
        }

        private bool AddFine(Guid borrowerId)
        {
            try
            {
                using (var context = new LibraryContext())
                {
                    DateTime now = DateTime.UtcNow;
                    var fine = new BorrowerFine()
                    {
                        FineAmount = 10.05,
                        FineToBorrowerId = borrowerId,
                        OnFineIssued = now,
                    };
                    context.BorrowerFines.Add(fine);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return false;
            }
        }
    }
}
