using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace LibraryManagementSystem.Data
{
    public static class DatabaseHelper
    {
        private const string DatabaseName = "LibraryDB";

        private static string ServerConnectionString =>
            $"Server={GetSetting("LIBRARY_DB_SERVER", "localhost")};Uid={GetSetting("LIBRARY_DB_USER", "root")};Pwd={GetSetting("LIBRARY_DB_PASSWORD", "jobayer2022")};AllowUserVariables=True;";

        private static string DatabaseConnectionString =>
            $"{ServerConnectionString}Database={DatabaseName};";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(DatabaseConnectionString);
        }

        public static void EnsureDatabaseSetup()
        {
            using var serverConnection = new MySqlConnection(ServerConnectionString);
            serverConnection.Open();

            using (var createDatabase = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS {DatabaseName};", serverConnection))
            {
                createDatabase.ExecuteNonQuery();
            }

            using var connection = GetConnection();
            connection.Open();

            string schema = @"
CREATE TABLE IF NOT EXISTS Users (
    UserID INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL DEFAULT 'Admin',
    LastLogin DATETIME NULL
);

CREATE TABLE IF NOT EXISTS Books (
    BookID INT AUTO_INCREMENT PRIMARY KEY,
    ISBN VARCHAR(30) NOT NULL UNIQUE,
    Title VARCHAR(150) NOT NULL,
    Author VARCHAR(120) NOT NULL,
    Publisher VARCHAR(120) NULL,
    Category VARCHAR(80) NULL,
    YearPublished INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 0,
    Available INT NOT NULL DEFAULT 0,
    ShelfLocation VARCHAR(50) NULL,
    CoverImagePath VARCHAR(255) NULL
);

CREATE TABLE IF NOT EXISTS Members (
    MemberID INT AUTO_INCREMENT PRIMARY KEY,
    FullName VARCHAR(120) NOT NULL,
    Email VARCHAR(120) NULL,
    Phone VARCHAR(30) NULL,
    Address VARCHAR(255) NULL,
    MembershipDate DATETIME NOT NULL,
    IsActive BIT NOT NULL DEFAULT b'1'
);

CREATE TABLE IF NOT EXISTS Transactions (
    TransactionID INT AUTO_INCREMENT PRIMARY KEY,
    BookID INT NOT NULL,
    MemberID INT NOT NULL,
    IssueDate DATETIME NOT NULL,
    DueDate DATETIME NOT NULL,
    ReturnDate DATETIME NULL,
    Status VARCHAR(20) NOT NULL,
    FOREIGN KEY (BookID) REFERENCES Books(BookID),
    FOREIGN KEY (MemberID) REFERENCES Members(MemberID)
);

-- Add CoverImagePath column if it doesn't exist
SET @dbname = DATABASE();
SET @tablename = 'Books';
SET @columnname = 'CoverImagePath';
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (table_schema = @dbname)
            AND (table_name = @tablename)
            AND (column_name = @columnname)
    ) > 0,
    'SELECT 1',
    CONCAT('ALTER TABLE ', @tablename, ' ADD COLUMN ', @columnname, ' VARCHAR(255) NULL;')
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

INSERT INTO Users (Username, PasswordHash, Role)
SELECT 'admin', SHA2('admin123', 256), 'Admin'
WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin');

INSERT INTO Books (ISBN, Title, Author, Publisher, Category, YearPublished, Quantity, Available, ShelfLocation, CoverImagePath)
SELECT '9780000000010', 'Clean Code', 'Robert C. Martin', 'Prentice Hall', 'Programming', 2008, 5, 5, 'A1', ''
WHERE NOT EXISTS (SELECT 1 FROM Books WHERE ISBN = '9780000000001');

INSERT INTO Books (ISBN, Title, Author, Publisher, Category, YearPublished, Quantity, Available, ShelfLocation, CoverImagePath)
SELECT '97800000000019', 'The Pragmatic Programmer', 'Andrew Hunt, David Thomas', 'Addison-Wesley', 'Programming', 1999, 4, 4, 'A1', ''
WHERE NOT EXISTS (SELECT 1 FROM Books WHERE ISBN = '9780000000002');

INSERT INTO Books (ISBN, Title, Author, Publisher, Category, YearPublished, Quantity, Available, ShelfLocation, CoverImagePath)
SELECT '97800000000018', 'Introduction to Algorithms', 'Thomas H. Cormen', 'MIT Press', 'Computer Science', 2009, 3, 3, 'A2', ''
WHERE NOT EXISTS (SELECT 1 FROM Books WHERE ISBN = '9780000000003');

INSERT INTO Books (ISBN, Title, Author, Publisher, Category, YearPublished, Quantity, Available, ShelfLocation, CoverImagePath)
SELECT '97800000000017', 'Design Patterns', 'Erich Gamma', 'Addison-Wesley', 'Software Engineering', 1994, 6, 6, 'A3', ''
WHERE NOT EXISTS (SELECT 1 FROM Books WHERE ISBN = '9780000000004');

INSERT INTO Books (ISBN, Title, Author, Publisher, Category, YearPublished, Quantity, Available, ShelfLocation, CoverImagePath)
SELECT '97800000000016', 'Code Complete', 'Steve McConnell', 'Microsoft Press', 'Software Development', 2004, 4, 4, 'A2', ''
WHERE NOT EXISTS (SELECT 1 FROM Books WHERE ISBN = '9780000000005');

INSERT INTO Books (ISBN, Title, Author, Publisher, Category, YearPublished, Quantity, Available, ShelfLocation, CoverImagePath)
SELECT '97800000000015', 'Effective Java', 'Joshua Bloch', 'Addison-Wesley', 'Programming', 2008, 5, 5, 'A4', ''
WHERE NOT EXISTS (SELECT 1 FROM Books WHERE ISBN = '97800000000006');

INSERT INTO Books (ISBN, Title, Author, Publisher, Category, YearPublished, Quantity, Available, ShelfLocation, CoverImagePath)
SELECT '97800000000014', 'You Don''t Know JS', 'Kyle Simpson', 'O''Reilly Media', 'Web Development', 2015, 7, 7, 'B1', ''
WHERE NOT EXISTS (SELECT 1 FROM Books WHERE ISBN = '97800000000007');

INSERT INTO Books (ISBN, Title, Author, Publisher, Category, YearPublished, Quantity, Available, ShelfLocation, CoverImagePath)
SELECT '97800000000013', 'Refactoring', 'Martin Fowler', 'Addison-Wesley', 'Programming', 2018, 3, 3, 'A1', ''
WHERE NOT EXISTS (SELECT 1 FROM Books WHERE ISBN = '97800000000008');

-- Add some more sample books
INSERT INTO Books (ISBN, Title, Author, Publisher, Category, YearPublished, Quantity, Available, ShelfLocation, CoverImagePath)
SELECT '97800000000012', 'The Clean Coder', 'Robert C. Martin', 'Prentice Hall', 'Programming', 2011, 3, 3, 'B2', ''
WHERE NOT EXISTS (SELECT 1 FROM Books WHERE ISBN = '9780000000009');

INSERT INTO Books (ISBN, Title, Author, Publisher, Category, YearPublished, Quantity, Available, ShelfLocation, CoverImagePath)
SELECT '9780000000011', 'C# in Depth', 'Jon Skeet', 'Manning Publications', 'Programming', 2019, 4, 4, 'B2', ''
WHERE NOT EXISTS (SELECT 1 FROM Books WHERE ISBN = '9780000000010');

INSERT INTO Members (FullName, Email, Phone, Address, MembershipDate, IsActive)
SELECT 'John Doe', 'john.doe@example.com', '0123456789', '123 Main St, City', NOW(), b'1'
WHERE NOT EXISTS (SELECT 1 FROM Members WHERE Email = 'john.doe@example.com');

INSERT INTO Members (FullName, Email, Phone, Address, MembershipDate, IsActive)
SELECT 'Jane Smith', 'jane.smith@example.com', '0987654321', '456 Oak Ave, Town', DATE_SUB(NOW(), INTERVAL 60 DAY), b'1'
WHERE NOT EXISTS (SELECT 1 FROM Members WHERE Email = 'jane.smith@example.com');

INSERT INTO Members (FullName, Email, Phone, Address, MembershipDate, IsActive)
SELECT 'Robert Johnson', 'robert.j@example.com', '0112233445', '789 Pine Rd, Village', DATE_SUB(NOW(), INTERVAL 120 DAY), b'1'
WHERE NOT EXISTS (SELECT 1 FROM Members WHERE Email = 'robert.j@example.com');

INSERT INTO Members (FullName, Email, Phone, Address, MembershipDate, IsActive)
SELECT 'Emily Davis', 'emily.d@example.com', '0554433221', '101 Cedar Ln, Hamlet', DATE_SUB(NOW(), INTERVAL 30 DAY), b'0'
WHERE NOT EXISTS (SELECT 1 FROM Members WHERE Email = 'emily.d@example.com');

INSERT INTO Members (FullName, Email, Phone, Address, MembershipDate, IsActive)
SELECT 'Michael Brown', 'michael.b@example.com', '0123987456', '112 Birch Ct, Borough', DATE_SUB(NOW(), INTERVAL 45 DAY), b'1'
WHERE NOT EXISTS (SELECT 1 FROM Members WHERE Email = 'michael.b@example.com');
";

            using var command = new MySqlCommand(schema, connection);
            command.ExecuteNonQuery();

            // Now, let's add some sample transactions (if there are no transactions yet)
            // First check if there are any transactions
            var checkTransactions = new MySqlCommand("SELECT COUNT(*) FROM Transactions", connection);
            var count = Convert.ToInt32(checkTransactions.ExecuteScalar());
            if (count == 0)
            {
                // Get some books and members
                var getBooks = new MySqlCommand("SELECT BookID FROM Books WHERE Available > 0 LIMIT 2", connection);
                var booksTable = new DataTable();
                using (var adapter = new MySqlDataAdapter(getBooks))
                {
                    adapter.Fill(booksTable);
                }

                var getMembers = new MySqlCommand("SELECT MemberID FROM Members WHERE IsActive = 1 LIMIT 2", connection);
                var membersTable = new DataTable();
                using (var adapter = new MySqlDataAdapter(getMembers))
                {
                    adapter.Fill(membersTable);
                }

                // If we have both books and members, add transactions
                if (booksTable.Rows.Count > 0 && membersTable.Rows.Count > 0)
                {
                    // Add first transaction
                    var book1Id = Convert.ToInt32(booksTable.Rows[0]["BookID"]);
                    var member1Id = Convert.ToInt32(membersTable.Rows[0]["MemberID"]);
                    var trans1 = new MySqlCommand(@"
INSERT INTO Transactions (BookID, MemberID, IssueDate, DueDate, Status)
VALUES (@BookID, @MemberID, DATE_SUB(NOW(), INTERVAL 5 DAY), DATE_ADD(DATE_SUB(NOW(), INTERVAL 5 DAY), INTERVAL 14 DAY), 'Issued');
UPDATE Books SET Available = Available - 1 WHERE BookID = @BookID;
", connection);
                    trans1.Parameters.AddWithValue("@BookID", book1Id);
                    trans1.Parameters.AddWithValue("@MemberID", member1Id);
                    trans1.ExecuteNonQuery();

                    // Add second transaction
                    if (booksTable.Rows.Count > 1 && membersTable.Rows.Count > 1)
                    {
                        var book2Id = Convert.ToInt32(booksTable.Rows[1]["BookID"]);
                        var member2Id = Convert.ToInt32(membersTable.Rows[1]["MemberID"]);
                        var trans2 = new MySqlCommand(@"
INSERT INTO Transactions (BookID, MemberID, IssueDate, DueDate, Status)
VALUES (@BookID, @MemberID, DATE_SUB(NOW(), INTERVAL 10 DAY), DATE_ADD(DATE_SUB(NOW(), INTERVAL 10 DAY), INTERVAL 14 DAY), 'Issued');
UPDATE Books SET Available = Available - 1 WHERE BookID = @BookID;
", connection);
                        trans2.Parameters.AddWithValue("@BookID", book2Id);
                        trans2.Parameters.AddWithValue("@MemberID", member2Id);
                        trans2.ExecuteNonQuery();
                    }
                }
            }
        }

        public static DataTable ExecuteQuery(string query, MySqlParameter[]? parameters = null)
        {
            using var connection = GetConnection();
            using var command = new MySqlCommand(query, connection);

            if (parameters is { Length: > 0 })
            {
                command.Parameters.AddRange(parameters);
            }

            using var adapter = new MySqlDataAdapter(command);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public static int ExecuteNonQuery(string query, MySqlParameter[]? parameters = null)
        {
            using var connection = GetConnection();
            connection.Open();

            using var command = new MySqlCommand(query, connection);
            if (parameters is { Length: > 0 })
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteNonQuery();
        }

        public static object? ExecuteScalar(string query, MySqlParameter[]? parameters = null)
        {
            using var connection = GetConnection();
            connection.Open();

            using var command = new MySqlCommand(query, connection);
            if (parameters is { Length: > 0 })
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteScalar();
        }

        private static string GetSetting(string key, string fallback)
        {
            return Environment.GetEnvironmentVariable(key) ?? fallback;
        }
    }
}
