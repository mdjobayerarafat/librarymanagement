using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace LibraryManagementSystem.Services
{
    public class BookService
    {
        public List<Book> GetAllBooks()
        {
            return LoadBooks("SELECT * FROM Books ORDER BY Title");
        }

        public List<Book> GetAvailableBooks()
        {
            return LoadBooks("SELECT * FROM Books WHERE Available > 0 ORDER BY Title");
        }

        public bool AddBook(Book book)
        {
            const string query = @"
INSERT INTO Books (ISBN, Title, Author, Publisher, Category, YearPublished, Quantity, Available, ShelfLocation, CoverImagePath)
VALUES (@ISBN, @Title, @Author, @Publisher, @Category, @YearPublished, @Quantity, @Available, @ShelfLocation, @CoverImagePath)";

            return DatabaseHelper.ExecuteNonQuery(query, CreateParameters(book)) > 0;
        }

        public bool UpdateBook(Book book)
        {
            const string query = @"
UPDATE Books
SET ISBN = @ISBN,
    Title = @Title,
    Author = @Author,
    Publisher = @Publisher,
    Category = @Category,
    YearPublished = @YearPublished,
    Quantity = @Quantity,
    Available = @Available,
    ShelfLocation = @ShelfLocation,
    CoverImagePath = @CoverImagePath
WHERE BookID = @BookID";

            return DatabaseHelper.ExecuteNonQuery(query, CreateParameters(book, includeId: true)) > 0;
        }

        public bool DeleteBook(int bookId)
        {
            const string query = "DELETE FROM Books WHERE BookID = @BookID";
            var parameters = new[] { new MySqlParameter("@BookID", bookId) };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public List<Book> SearchBooks(string keyword)
        {
            const string query = @"
SELECT * FROM Books
WHERE Title LIKE @Keyword OR Author LIKE @Keyword OR ISBN LIKE @Keyword OR Category LIKE @Keyword
ORDER BY Title";

            var parameters = new[] { new MySqlParameter("@Keyword", $"%{keyword}%") };
            return LoadBooks(query, parameters);
        }

        private static List<Book> LoadBooks(string query, MySqlParameter[]? parameters = null)
        {
            var table = DatabaseHelper.ExecuteQuery(query, parameters);
            var books = new List<Book>();

            foreach (DataRow row in table.Rows)
            {
                books.Add(new Book
                {
                    BookID = Convert.ToInt32(row["BookID"]),
                    ISBN = row["ISBN"]?.ToString() ?? string.Empty,
                    Title = row["Title"]?.ToString() ?? string.Empty,
                    Author = row["Author"]?.ToString() ?? string.Empty,
                    Publisher = row["Publisher"]?.ToString() ?? string.Empty,
                    Category = row["Category"]?.ToString() ?? string.Empty,
                    YearPublished = Convert.ToInt32(row["YearPublished"]),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    Available = Convert.ToInt32(row["Available"]),
                    ShelfLocation = row["ShelfLocation"]?.ToString() ?? string.Empty,
                    CoverImagePath = row["CoverImagePath"]?.ToString() ?? string.Empty
                });
            }

            return books;
        }

        private static MySqlParameter[] CreateParameters(Book book, bool includeId = false)
        {
            var parameters = new List<MySqlParameter>
            {
                new("@ISBN", book.ISBN),
                new("@Title", book.Title),
                new("@Author", book.Author),
                new("@Publisher", book.Publisher),
                new("@Category", book.Category),
                new("@YearPublished", book.YearPublished),
                new("@Quantity", book.Quantity),
                new("@Available", book.Available),
                new("@ShelfLocation", book.ShelfLocation),
                new("@CoverImagePath", book.CoverImagePath)
            };

            if (includeId)
            {
                parameters.Add(new MySqlParameter("@BookID", book.BookID));
            }

            return parameters.ToArray();
        }
    }
}
