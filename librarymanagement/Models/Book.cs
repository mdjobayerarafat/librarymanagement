namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public int BookID { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int YearPublished { get; set; }
        public int Quantity { get; set; }
        public int Available { get; set; }
        public string ShelfLocation { get; set; } = string.Empty;
        public string CoverImagePath { get; set; } = string.Empty; // Path to book cover image
    }
}
