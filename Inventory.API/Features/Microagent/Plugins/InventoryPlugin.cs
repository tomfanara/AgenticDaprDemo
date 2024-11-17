namespace Accounting.API.Features.Microagent.Plugins;

using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Data.SQLite;

    
    public class InventoryPlugin
    {
        [KernelFunction("get_inventory"), Description("Get current inventory")]
        [return: Description("A list of current inventory.")]
        public string GetEmployees(Kernel kernel)
        {
            // Connection string for an in-memory database
            string connectionString = "Data Source=:memory:;Version=3;New=True;";
            string result = "";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Create a table
                string createTableQuery = "CREATE TABLE Inventory (Id INTEGER PRIMARY KEY, Item TEXT)";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Insert data into the table
                string insertDataQuery = "INSERT INTO Inventory (Item) VALUES ('Laptop'), ('iPhone'), ('iPad')";
                using (var command = new SQLiteCommand(insertDataQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Query the data
                string selectQuery = "SELECT * FROM Inventory";
                
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {                            
                            result = result + $"Id: {reader["Id"]}, Name: {reader["Item"]}";
                        }
                    }
                }
            }

            // save to txt file for now
            SaveData(kernel, result);

            return result;
        }

    [KernelFunction("save_data"), Description("Saves data to a file on your computer")]
    [return: Description("A list of employees from accounting.")]
    public async void SaveData(Kernel kernel, string data)
    {
        await File.WriteAllTextAsync($@"..\inventory.txt", data);
    }
}


