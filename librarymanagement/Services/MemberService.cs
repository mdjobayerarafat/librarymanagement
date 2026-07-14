using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace LibraryManagementSystem.Services
{
    public class MemberService
    {
        public List<Member> GetAllMembers()
        {
            return LoadMembers("SELECT * FROM Members ORDER BY FullName");
        }

        public List<Member> GetActiveMembers()
        {
            return LoadMembers("SELECT * FROM Members WHERE IsActive = b'1' ORDER BY FullName");
        }

        public bool AddMember(Member member)
        {
            const string query = @"
INSERT INTO Members (FullName, Email, Phone, Address, MembershipDate, IsActive)
VALUES (@FullName, @Email, @Phone, @Address, @MembershipDate, @IsActive)";

            return DatabaseHelper.ExecuteNonQuery(query, CreateParameters(member)) > 0;
        }

        public bool UpdateMember(Member member)
        {
            const string query = @"
UPDATE Members
SET FullName = @FullName,
    Email = @Email,
    Phone = @Phone,
    Address = @Address,
    MembershipDate = @MembershipDate,
    IsActive = @IsActive
WHERE MemberID = @MemberID";

            return DatabaseHelper.ExecuteNonQuery(query, CreateParameters(member, includeId: true)) > 0;
        }

        public bool DeleteMember(int memberId)
        {
            const string query = "DELETE FROM Members WHERE MemberID = @MemberID";
            var parameters = new[] { new MySqlParameter("@MemberID", memberId) };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public List<Member> SearchMembers(string keyword)
        {
            const string query = @"
SELECT * FROM Members
WHERE FullName LIKE @Keyword OR Email LIKE @Keyword OR Phone LIKE @Keyword
ORDER BY FullName";

            var parameters = new[] { new MySqlParameter("@Keyword", $"%{keyword}%") };
            return LoadMembers(query, parameters);
        }

        private static List<Member> LoadMembers(string query, MySqlParameter[]? parameters = null)
        {
            var table = DatabaseHelper.ExecuteQuery(query, parameters);
            var members = new List<Member>();

            foreach (DataRow row in table.Rows)
            {
                members.Add(new Member
                {
                    MemberID = Convert.ToInt32(row["MemberID"]),
                    FullName = row["FullName"]?.ToString() ?? string.Empty,
                    Email = row["Email"]?.ToString() ?? string.Empty,
                    Phone = row["Phone"]?.ToString() ?? string.Empty,
                    Address = row["Address"]?.ToString() ?? string.Empty,
                    MembershipDate = Convert.ToDateTime(row["MembershipDate"]),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    Status = Convert.ToBoolean(row["IsActive"]) ? "Active" : "Inactive"
                });
            }

            return members;
        }

        private static MySqlParameter[] CreateParameters(Member member, bool includeId = false)
        {
            var parameters = new List<MySqlParameter>
            {
                new("@FullName", member.FullName),
                new("@Email", member.Email),
                new("@Phone", member.Phone),
                new("@Address", member.Address),
                new("@MembershipDate", member.MembershipDate),
                new("@IsActive", member.IsActive)
            };

            if (includeId)
            {
                parameters.Add(new MySqlParameter("@MemberID", member.MemberID));
            }

            return parameters.ToArray();
        }
    }
}
