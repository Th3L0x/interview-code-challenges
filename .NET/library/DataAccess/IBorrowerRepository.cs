using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public interface IBorrowerRepository
    {
        public List<Borrower> GetBorrowers();

        Borrower? GetBorrowerByEmail(string email);

        public Guid AddBorrower(Borrower borrower);
    }
}
