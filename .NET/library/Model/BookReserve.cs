namespace OneBeyondApi.Model
{
    public class BookReserve
    {
        public Guid Id { get; set; }
        public Guid BookOnReservedId { get; set; }
        public Book BookOnReserved { get; set; }
        public Guid ReserverId { get; set; }
        public Borrower Reserver { get; set; }
        public DateTime OnReserved { get; set; }
        public int LoanIntervalInDays { get; set; }
    }
}
