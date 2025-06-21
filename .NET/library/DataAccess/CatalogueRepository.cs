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

        public List<BorrowerOnLoan> GetBorrowersOnActiveLoans()
        {
            using (var context = new LibraryContext())
            {
                var result = context.Catalogue
                    .Include(x => x.Book)
                    .Include(x => x.OnLoanTo)
                    .Where(x => x.OnLoanTo != null && x.LoanEndDate != null)
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
                    var bookStock = context.Catalogue
                        .Include(x => x.Book)
                        .Include(x => x.OnLoanTo)
                        .Where(x => x.OnLoanTo != null && x.LoanEndDate != null)
                        .FirstOrDefault(x => x.Book.Id == returnBookParameter.BookId && x.OnLoanTo!.Id == returnBookParameter.BorrowerId) 
                        ?? throw new NullReferenceException($"Could not find loan with book id: {returnBookParameter.BorrowerId} and with borrower id {returnBookParameter.BorrowerId}");
                    bookStock.OnLoanTo = null;
                    bookStock.LoanEndDate = null;
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
    }
}
