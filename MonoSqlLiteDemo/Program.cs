using System;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Threading;

namespace MonoSqlLiteDemo
{
    /// <summary>
    /// Demonstrates Mono functionality in a small demo executable.
    /// </summary>
    /// <remarks>
    /// More or less pulled directly from http://www.mono-project.com/docs/database-access/providers/sqlite/, created using:
    /// sqlite3
    /// > .open demo.db
    /// > CREATE TABLE employee ( firstname nvarchar(32), lastname nvarchar(32) );
    /// > INSERT INTO employee VALUES ("First", "Last");
    /// > INSERT INTO employee VALUES ("Second", "Also Last");
    /// > .exit
    /// </remarks>
    public class Program
    {
        private static void Client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Console.WriteLine($"Email transmission status: {e.UserState as string} -> Cancelled: {e.Cancelled}. {e.Error?.ToString() ?? "No Error"}");
        }

        private static void MailTest(string[] args)
        {
            SmtpClient client = new SmtpClient(args[0], int.Parse(args[1]));
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(args[2], args[3]);
            client.SendCompleted += Client_SendCompleted;

            MailAddress from = new MailAddress("admin@napack.net", "TestUser");
            MailAddress to = new MailAddress("gus.gran@helium24.net");

            string subject = "Napack Framework Server User Email Verification";
            MailMessage message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = "Test Data",
                IsBodyHtml = false,
            };

            try
            {
                client.SendAsync(message, "gus.gran@helium24.net");
            }
            catch (Exception ex)
            {
                Client_SendCompleted(null, new AsyncCompletedEventArgs(ex, false, "gus.gran@helium24.net"));
            }

            Thread.Sleep(20000);
        }

        private static void SqlTest(string[] args)
        {
            string filePath = "demo.db";
            File.Delete(filePath);
            SQLiteConnection.CreateFile(filePath);

            string uri = $"Data Source={filePath}";
            using (IDbConnection dbcon = new SQLiteConnection(uri))
            {
                dbcon.Open();
                Console.WriteLine("Create Table");
                using (IDbCommand dbcmd = dbcon.CreateCommand())
                {
                    dbcmd.CommandText = "CREATE TABLE test (column TEXT PRIMARY KEY COLLATE NOCASE NOT NULL, value TEXT)";
                    dbcmd.ExecuteNonQuery();
                }

                Console.WriteLine("Insert One");
                using (IDbCommand dbcmd = dbcon.CreateCommand())
                {
                    dbcmd.CommandText = "INSERT INTO test VALUES (\"A\", \"B\")";
                    dbcmd.ExecuteNonQuery();
                }

                Console.WriteLine("Insert Two");
                using (IDbCommand dbcmd = dbcon.CreateCommand())
                {
                    dbcmd.CommandText = "INSERT INTO test VALUES (\"a\", \"B\")";
                    try
                    {
                        dbcmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException se)
                    {
                        if (se.ErrorCode != (int)SQLiteErrorCode.Constraint)
                        {
                            throw;
                        }
                        else
                        {
                            Console.WriteLine("Hit expected constraint validation failure.");
                        }
                    }
                }
                // const string sql =
                //    "SELECT firstname, lastname " +
                //    "FROM employee";
                // dbcmd.CommandText = sql;
                // 
                // using (IDataReader reader = dbcmd.ExecuteReader())
                // {
                //     while (reader.Read())
                //     {
                //         string firstName = reader.GetString(0);
                //         string lastName = reader.GetString(1);
                //         Console.WriteLine("Name: {0} {1}",
                //             firstName, lastName);
                //     }
                // }
            }
        }

        public static void Main(string[] args)
        {
            // MailTest(args);
            // return;

            SqlTest(args);
            return;
        }
    }
}
