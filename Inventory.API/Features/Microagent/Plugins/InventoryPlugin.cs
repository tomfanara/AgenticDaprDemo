namespace Accounting.API.Features.Microagent.Plugins;

using Microsoft.SemanticKernel;
using System.ComponentModel;
using Microsoft.Data.Sqlite;


public class InventoryPlugin
{
    [KernelFunction("get_inventory"), Description("Get current inventory")]
    [return: Description("A list of current inventory.")]
    public string GetEmployees(Kernel kernel)
    {
        // Connection string for an in-memory database
        string connectionString = "Data Source=:memory:";
        string result = "";
        using (var connection = new SqliteConnection(connectionString)) // Change SQLiteConnection to SqliteConnection
        {
            connection.Open();

            // Create a table
            string createTableQuery = "CREATE TABLE Inventory (Id INTEGER PRIMARY KEY, Item TEXT, Description TEXT, Quantity TEXT)";
            using (var command = new SqliteCommand(createTableQuery, connection)) // Change SQLiteCommand to SqliteCommand
            {
                command.ExecuteNonQuery();
            }

            // Insert data into the table
            string insertDataQuery = "INSERT INTO Inventory (Item, Description, Quantity) VALUES ('Laptop', 'Dell Latitude', '43'), ('Mobile Phone', 'iPhone', '20' ), ('Tablet', 'iPad', '56'), ('Tablet', 'Samsung Galaxy', '34')";
            using (var command = new SqliteCommand(insertDataQuery, connection)) // Change SQLiteCommand to SqliteCommand
            {
                command.ExecuteNonQuery();
            }

            // Query the data
            string selectQuery = "SELECT * FROM Inventory";

            using (var command = new SqliteCommand(selectQuery, connection)) // Change SQLiteCommand to SqliteCommand
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = result + $"Id: {reader["Id"]}, Item: {reader["Item"]}, Description: {reader["Description"]}, Quantity: {reader["Quantity"]}";
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


