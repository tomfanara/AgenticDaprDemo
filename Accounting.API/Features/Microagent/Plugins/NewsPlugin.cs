namespace Accounting.API.Features.Microagent.Plugins;

using Microsoft.SemanticKernel;
using SimpleFeedReader;
using System.ComponentModel;
using System.Data.SQLite;

    
    public class NewsPlugin
    {
        [KernelFunction("execute_query"), Description("Get employees from accounting")]
        [return: Description("A list of employees from accounting.")]
        public string ExecuteQuery(Kernel kernel)
        {
            // Connection string for an in-memory database
            string connectionString = "Data Source=:memory:;Version=3;New=True;";
            string result = "";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Create a table
                string createTableQuery = "CREATE TABLE SampleTable (Id INTEGER PRIMARY KEY, Name TEXT)";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Insert data into the table
                string insertDataQuery = "INSERT INTO SampleTable (Name) VALUES ('Alice'), ('Bob'), ('Jimmy')";
                using (var command = new SQLiteCommand(insertDataQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Query the data
                string selectQuery = "SELECT * FROM SampleTable";
                
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Console.WriteLine($"Id: {reader["Id"]}, Name: {reader["Name"]}");
                            result = result + $"Id: {reader["Id"]}, Name: {reader["Name"]}";
                        }
                    }
                }
            }

           return result;
        }

        [KernelFunction("get_news"), Description("Gets news items for today's date.")]
        [return: Description("A list of current news stories.")]
        public List<FeedItem> GetNews()
        {
            var reader = new FeedReader();
            return reader.RetrieveFeed($"https://rss.nytimes.com/services/xml/rss/nyt/technology.xml").Take(1).ToList();
           
        }
        
    }


