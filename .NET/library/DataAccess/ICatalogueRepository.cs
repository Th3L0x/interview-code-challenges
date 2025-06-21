using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public interface ICatalogueRepository
    {
        public List<BookStock> GetCatalogue();

        List<BorrowerOnLoan> GetBorrowersWithActiveLoans();

        bool ReturnBook(ReturnBookParameter returnBookParameter);

        bool ReserveBook(ReserveBookParameter reserveBookParameter);

        string GetBookAvaibality(Guid bookId);

        List<BorrowerFine> GetFines();

        public List<BookStock> SearchCatalogue(CatalogueSearch search);
    }
}
