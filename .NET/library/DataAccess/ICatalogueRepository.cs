using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public interface ICatalogueRepository
    {
        public List<BookStock> GetCatalogue();

        List<BorrowerOnLoan> GetBorrowersOnActiveLoans();

        bool ReturnBook(ReturnBookParameter returnBookParameter);

        List<BorrowerFine> GetFines();

        public List<BookStock> SearchCatalogue(CatalogueSearch search);
    }
}
