using System;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;

namespace MonoSqlLiteDemo
{
    /// <summary>
    /// Demonstrates usage of Sqlite on Mono.
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
        static void Main(string[] args)
        {
            string filePath = "demo.db";
            File.Delete(filePath);
            SqliteConnection.CreateFile(filePath);

            string uri = $"Data Source={filePath}";
            using (IDbConnection dbcon = new SqliteConnection(uri))
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
                    catch (SqliteException se)
                    {
                        if (se.ErrorCode != SQLiteErrorCode.Constraint)
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
    }
}
