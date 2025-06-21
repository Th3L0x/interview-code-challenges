using Microsoft.EntityFrameworkCore;
using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public class CatalogueRepository : ICatalogueRepository
    {
        private readonly ILogger<CatalogueRepository> _logger;
        private IBorrowerRepository _borrowerRepository;

        public CatalogueRepository(ILogger<CatalogueRepository> logger, IBorrowerRepository borrowerRepository)
        {
            _logger = logger;
            _borrowerRepository = borrowerRepository;
        }

        public List<BookStock> GetCatalogue()
        {
            using (var context = new LibraryContext())
            {
                var list = context.Catalogue
                    .Include(x => x.Book)
                    .ThenInclude(x => x.Author)
                    .Include(x => x.OnLoanTo)
                    .AsNoTracking()
                    .ToList();
                return list;
            }
        }

        public BookStock? GetCatalogueById(Guid id)
        {
            using (var context = new LibraryContext())
            {
                var bookStock = context.Catalogue
                              .Include(x => x.Book)
                              .ThenInclude(x => x.Author)
                              .Include(x => x.OnLoanTo)
                              .FirstOrDefault(x => x.Book.Id == id);
                return bookStock;
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

        public List<BorrowerOnLoan> GetBorrowersWithActiveLoans()
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
                        BookId = x.Book.Id,
                        BookTitle = x.Book.Name,
                        OnLoanTo = x.OnLoanTo,
                    })
                    .ToList();
                return result;
            }
        }

        public bool ReserveBook(ReserveBookParameter reserveBookParameter)
        {
            try
            {
                DateTime now = DateTime.Now;

                if (reserveBookParameter == null || reserveBookParameter.Reserver == null)
                {
                    throw new ArgumentException("Invalid argument(s)!");
                }
                Guid reserverId = reserveBookParameter.Reserver.Id;
                var borrower = _borrowerRepository.GetBorrowerByEmail(reserveBookParameter.Reserver.EmailAddress);

                bool isBorrowerExist = borrower != null;

                if (!isBorrowerExist) 
                {
                    reserverId = _borrowerRepository.AddBorrower(reserveBookParameter.Reserver);
                }

                var activeLoans = GetBorrowersWithActiveLoans();

                using (var context = new LibraryContext())
                {

                    if (activeLoans.Any(x => x.BookId == reserveBookParameter.BookId))
                    {
                        //this book is already under a loan
                        //reserve it

                        var reserve = new BookReserve()
                        {
                            BookOnReservedId = reserveBookParameter.BookId,
                            ReserverId = reserverId,
                            OnReserved = now,
                            LoanIntervalInDays = reserveBookParameter.LoanIntervalInDays,
                        };
                        context.BookReserves.Add(reserve);
                        context.SaveChanges();
                    }
                    else
                    {
                        //this book is not under any loan
                        //check if there any previous reserve, remove it
                        //borrow it

                        var revervedBooks = context.BookReserves
                             .Include(x => x.BookOnReserved)
                             .Include(x => x.Reserver)
                             .AsNoTracking()
                             .Where(x => x.BookOnReserved.Id == reserveBookParameter.BookId && x.Reserver.Id == reserveBookParameter.Reserver.Id)
                             .ToArray();

                        if(revervedBooks != null && revervedBooks.Length > 0)
                        {
                            context.BookReserves.RemoveRange(revervedBooks);
                            context.SaveChanges();
                        }


                        var bookStock = context.Catalogue
                             .Include(x => x.Book)
                             .Include(x => x.OnLoanTo)
                             .FirstOrDefault(x => x.Book.Id == reserveBookParameter.BookId)
                               ?? throw new NullReferenceException($"Could not find book with book id: {reserveBookParameter.BookId}");
                        bookStock.OnLoanToId = reserveBookParameter.Reserver.Id;
                        bookStock.LoanEndDate = now.AddDays(reserveBookParameter.LoanIntervalInDays);
                        context.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return false;
            }
        }

        public string GetBookAvaibality(Guid bookId)
        {
            using (var context = new LibraryContext())
            {
                DateTime avaiblity = DateTime.Now;
                var bookStock = context.Catalogue
                          .Include(x => x.Book)
                          .Include(x => x.OnLoanTo)
                          .AsNoTracking()
                          .FirstOrDefault(x => x.Book.Id == bookId)
                          ?? throw new NullReferenceException($"Could not find book with id {bookId}");

                if(bookStock.OnLoanTo != null && bookStock.LoanEndDate != null)
                {
                    double days = (bookStock.LoanEndDate.Value - DateTime.Now).TotalDays;
                    if (days > 0) 
                    {
                        avaiblity = avaiblity.AddDays((bookStock.LoanEndDate.Value - DateTime.Now).TotalDays);
                    }
                }
                var reservations = context.BookReserves
                    .AsNoTracking()
                    .Where(x => x.BookOnReservedId == bookId)
                    .ToList();
                reservations.ForEach(x => avaiblity = avaiblity.AddDays(x.LoanIntervalInDays));
                int result = (avaiblity - DateTime.Now).Days;
                return result <= 0 ? "This book is available right now!" : $"This book will be available after {result} day(s)";
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
                        ?? throw new NullReferenceException($"Could not find book under loan with book id: {returnBookParameter.BorrowerId} and with borrower id {returnBookParameter.BorrowerId}");

                    DateTime endDate = bookStock.LoanEndDate!.Value;
                    Guid borrowerId = bookStock.OnLoanTo!.Id;
                    bookStock.OnLoanToId = null;
                    bookStock.OnLoanTo = null;
                    bookStock.LoanEndDate = null;
                    int result = context.SaveChanges();

                    if(result > 0 && endDate > now) 
                    {
                        bool isFineAdded = AddFine(borrowerId); //TODO raise error if false?
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
