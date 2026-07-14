using System;

namespace LibraryManagementSystem.Models
{
    public class Member
    {
        public int MemberID { get; set; }
        public string MemberCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public DateTime MembershipDate { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = "Active";
        public bool IsActive { get; set; } = true;
        public int MaxBooksAllowed { get; set; } = 5;
        public int CurrentBorrowedCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool IsMembershipActive => Status == "Active" && (ExpiryDate == null || ExpiryDate >= DateTime.Now);
        public bool CanBorrow => IsMembershipActive && CurrentBorrowedCount < MaxBooksAllowed;
    }
}
