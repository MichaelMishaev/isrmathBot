﻿using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;


public abstract class DatabaseService
{
    private readonly string _connectionString;

    protected DatabaseService(IConfiguration configuration)
    {
        // Read the connection string from the configuration
        _connectionString = "Server=localhost;Database=bmp;User Id=root;Password=Jtbdtjtb6262;Port=3306;";// configuration.GetConnectionString("DefaultConnection");
    }

    // Method to get a new database connection
    protected MySqlConnection GetConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
