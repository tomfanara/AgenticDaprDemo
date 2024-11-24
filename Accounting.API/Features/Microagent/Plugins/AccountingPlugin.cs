﻿namespace Accounting.API.Features.Microagent.Plugins;

using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Data.SQLite;


public class AccountingPlugin
{
    [KernelFunction("get_employees"), Description("Get employees")]
    [return: Description("A list of employees from accounting.")]
    public string GetEmployees(Kernel kernel)
    {
        // Connection string for an in-memory database
        string connectionString = "Data Source=:memory:;Version=3;New=True;";
        string result = "";
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            // Create a table
            string createTableQuery = "CREATE TABLE Employees (Id INTEGER PRIMARY KEY, Name TEXT, Title TEXT, StartDate TEXT)";
            using (var command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            // Insert data into the table
            string insertDataQuery = "INSERT INTO Employees (Name, Title, StartDate) VALUES ('Alice', 'manager', '02/05/2024'), ('Bobby', 'developer', '04/15/2023'), ('Carol', 'scrum master', '06/25/2021')";
            using (var command = new SQLiteCommand(insertDataQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            // Query the data
            string selectQuery = "SELECT * FROM Employees";

            using (var command = new SQLiteCommand(selectQuery, connection))
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