﻿using System.Linq.Expressions;

namespace OneBeyondApi.Model
{
    public class BookStock
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public Book Book { get; set; }
        public DateTime? LoanEndDate { get; set; }
        public Guid? OnLoanToId { get; set; }
        public Borrower? OnLoanTo { get; set; }
    }
}
