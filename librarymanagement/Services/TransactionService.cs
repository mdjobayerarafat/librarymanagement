using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace LibraryManagementSystem.Services
{
    public class TransactionService
    {
        public List<Transaction> GetAllTransactions()
        {
            const string query = @"
SELECT t.TransactionID, t.BookID, t.MemberID, t.IssueDate, t.DueDate, t.ReturnDate, t.Status,
       b.Title AS BookTitle, m.FullName AS MemberName
FROM Transactions t
INNER JOIN Books b ON b.BookID = t.BookID
INNER JOIN Members m ON m.MemberID = t.MemberID
ORDER BY t.IssueDate DESC";

            return LoadTransactions(query);
        }

        public List<Transaction> SearchTransactions(string keyword)
        {
            const string query = @"
SELECT t.TransactionID, t.BookID, t.MemberID, t.IssueDate, t.DueDate, t.ReturnDate, t.Status,
       b.Title AS BookTitle, m.FullName AS MemberName
FROM Transactions t
INNER JOIN Books b ON b.BookID = t.BookID
INNER JOIN Members m ON m.MemberID = t.MemberID
WHERE b.Title LIKE @Keyword OR m.FullName LIKE @Keyword OR t.Status LIKE @Keyword
ORDER BY t.IssueDate DESC";

            var parameters = new[] { new MySqlParameter("@Keyword", $"%{keyword}%") };
            return LoadTransactions(query, parameters);
        }

        public bool IssueBook(int bookId, int memberId, DateTime dueDate)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var availabilityCommand = new MySqlCommand(
                    "SELECT Available FROM Books WHERE BookID = @BookID FOR UPDATE",
                    connection,
                    transaction);
                availabilityCommand.Parameters.AddWithValue("@BookID", bookId);
                var available = Convert.ToInt32(availabilityCommand.ExecuteScalar() ?? 0);

                if (available <= 0)
                {
                    transaction.Rollback();
                    return false;
                }

                var insertCommand = new MySqlCommand(@"
INSERT INTO Transactions (BookID, MemberID, IssueDate, DueDate, Status)
VALUES (@BookID, @MemberID, @IssueDate, @DueDate, 'Issued')", connection, transaction);
                insertCommand.Parameters.AddWithValue("@BookID", bookId);
                insertCommand.Parameters.AddWithValue("@MemberID", memberId);
                insertCommand.Parameters.AddWithValue("@IssueDate", DateTime.Now);
                insertCommand.Parameters.AddWithValue("@DueDate", dueDate);
                insertCommand.ExecuteNonQuery();

                var updateBookCommand = new MySqlCommand(
                    "UPDATE Books SET Available = Available - 1 WHERE BookID = @BookID",
                    connection,
                    transaction);
                updateBookCommand.Parameters.AddWithValue("@BookID", bookId);
                updateBookCommand.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool ReturnBook(int transactionId)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var bookLookupCommand = new MySqlCommand(
                    "SELECT BookID FROM Transactions WHERE TransactionID = @TransactionID AND ReturnDate IS NULL FOR UPDATE",
                    connection,
                    transaction);
                bookLookupCommand.Parameters.AddWithValue("@TransactionID", transactionId);
                var bookIdObj = bookLookupCommand.ExecuteScalar();

                if (bookIdObj == null)
                {
                    transaction.Rollback();
                    return false;
                }

                var bookId = Convert.ToInt32(bookIdObj);

                var returnCommand = new MySqlCommand(@"
UPDATE Transactions
SET ReturnDate = @ReturnDate, Status = 'Returned'
WHERE TransactionID = @TransactionID", connection, transaction);
                returnCommand.Parameters.AddWithValue("@ReturnDate", DateTime.Now);
                returnCommand.Parameters.AddWithValue("@TransactionID", transactionId);
                returnCommand.ExecuteNonQuery();

                var updateBookCommand = new MySqlCommand(
                    "UPDATE Books SET Available = Available + 1 WHERE BookID = @BookID",
                    connection,
                    transaction);
                updateBookCommand.Parameters.AddWithValue("@BookID", bookId);
                updateBookCommand.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static List<Transaction> LoadTransactions(string query, MySqlParameter[]? parameters = null)
        {
            var table = DatabaseHelper.ExecuteQuery(query, parameters);
            var transactions = new List<Transaction>();

            foreach (DataRow row in table.Rows)
            {
                transactions.Add(new Transaction
                {
                    TransactionID = Convert.ToInt32(row["TransactionID"]),
                    BookID = Convert.ToInt32(row["BookID"]),
                    MemberID = Convert.ToInt32(row["MemberID"]),
                    BookTitle = row["BookTitle"]?.ToString() ?? string.Empty,
                    MemberName = row["MemberName"]?.ToString() ?? string.Empty,
                    IssueDate = Convert.ToDateTime(row["IssueDate"]),
                    DueDate = Convert.ToDateTime(row["DueDate"]),
                    ReturnDate = row["ReturnDate"] == DBNull.Value ? null : Convert.ToDateTime(row["ReturnDate"]),
                    Status = row["Status"]?.ToString() ?? string.Empty
                });
            }

            return transactions;
        }
    }
}
