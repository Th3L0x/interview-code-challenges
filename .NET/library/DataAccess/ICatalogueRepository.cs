using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public interface ICatalogueRepository
    {
        public List<BookStock> GetCatalogue();

        List<BorrowerOnLoan> GetBorrowersOnActiveLoans();

        bool ReturnBook(ReturnBookParameter returnBookParameter);

        public List<BookStock> SearchCatalogue(CatalogueSearch search);
    }
}
