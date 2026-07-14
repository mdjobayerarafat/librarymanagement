using System;

namespace LibraryManagementSystem.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public int BookID { get; set; }
        public int MemberID { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(14);
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Issued";
    }
}
