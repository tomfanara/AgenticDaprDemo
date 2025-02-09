namespace Accounting.API.Features.Microagent.Plugins;

using Microsoft.SemanticKernel;
using System.ComponentModel;
using Microsoft.Data.Sqlite;


public class AccountingPlugin
{
    [KernelFunction("get_employees"), Description("Get employees")]
    [return: Description("A list of employees from accounting.")]
    public string GetEmployees(Kernel kernel)
    {
        // Connection string for an in-memory database
        string connectionString = "Data Source=:memory:";
        string result = "";
        using (var connection = new SqliteConnection(connectionString)) // Fix: Corrected the class name to SqliteConnection
        {
            connection.Open();

            // Create a table
            string createTableQuery = "CREATE TABLE Employees (Id INTEGER PRIMARY KEY, Name TEXT, Title TEXT, StartDate TEXT)";
            using (var command = new SqliteCommand(createTableQuery, connection)) // Fix: Corrected the class name to SqliteCommand
            {
                command.ExecuteNonQuery();
            }

            // Insert data into the table
            string insertDataQuery = "INSERT INTO Employees (Name, Title, StartDate) VALUES ('Alice', 'Inventory manager', '02/05/2024'), ('Bobby', 'Accounting manager', '04/15/2023'), ('Carol', 'Sales manager', '06/25/2021')";
            using (var command = new SqliteCommand(insertDataQuery, connection)) // Fix: Corrected the class name to SqliteCommand
            {
                command.ExecuteNonQuery();
            }

            // Query the data
            string selectQuery = "SELECT * FROM Employees";

            using (var command = new SqliteCommand(selectQuery, connection)) // Fix: Corrected the class name to SqliteCommand
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = result + $"Id: {reader["Id"]}, Name: {reader["Name"]}, Title: {reader["Title"]}, Start Date: {reader["StartDate"]}";
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
        await File.WriteAllTextAsync($@"..\employee.txt", data);
    }
}
