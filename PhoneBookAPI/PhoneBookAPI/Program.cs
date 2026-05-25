using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors();

string dbPath = "PhoneBook.db";
CreateDatabase(dbPath);
SeedData(dbPath);

string connectionString = $"Data Source={dbPath}";


app.MapGet("/api/contacts", () =>
{
    using IDbConnection db = new SqliteConnection(connectionString);
    db.Open();

    var contacts = db.Query<Contact>(@"
        SELECT * FROM Contacts 
        ORDER BY LastName, FirstName").ToList();

    foreach (var contact in contacts)
    {
        contact.Phones = db.Query<Phone>(@"
            SELECT * FROM Phones 
            WHERE ContactId = @ContactId",
            new { ContactId = contact.Id }).ToList();
    }

    return Results.Ok(contacts.Select(c => new
    {
        c.Id,
        c.LastName,
        c.FirstName,
        c.MiddleName,
        FullName = $"{c.LastName} {c.FirstName} {c.MiddleName}".Trim(),
        Phones = c.Phones.Select(p => new
        {
            p.Id,
            p.PhoneNumber,
            p.PhoneType
        })
    }));
});

app.Run();


void CreateDatabase(string path)
{
    using var connection = new SqliteConnection($"Data Source={path}");
    connection.Open();

    connection.Execute(@"
        CREATE TABLE IF NOT EXISTS Contacts (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            LastName TEXT NOT NULL,
            FirstName TEXT NOT NULL,
            MiddleName TEXT,
            CreatedAt TEXT DEFAULT (datetime('now')),
            UpdatedAt TEXT DEFAULT (datetime('now'))
        );
        
        CREATE TABLE IF NOT EXISTS Phones (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ContactId INTEGER NOT NULL,
            PhoneNumber TEXT NOT NULL,
            PhoneType TEXT DEFAULT 'Mobile',
            CreatedAt TEXT DEFAULT (datetime('now')),
            FOREIGN KEY (ContactId) REFERENCES Contacts(Id) ON DELETE CASCADE
        );
        
        CREATE TABLE IF NOT EXISTS ContactsHistory (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ContactId INTEGER NOT NULL,
            LastName TEXT,
            FirstName TEXT,
            MiddleName TEXT,
            ActionType TEXT NOT NULL,
            ChangedAt TEXT DEFAULT (datetime('now'))
        );
        
        CREATE TABLE IF NOT EXISTS PhonesHistory (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            PhoneId INTEGER NOT NULL,
            ContactId INTEGER NOT NULL,
            PhoneNumber TEXT,
            PhoneType TEXT,
            ActionType TEXT NOT NULL,
            ChangedAt TEXT DEFAULT (datetime('now'))
        );
    ");
}

void SeedData(string path)
{
    using var connection = new SqliteConnection($"Data Source={path}");
    connection.Open();

    var count = connection.QuerySingle<int>("SELECT COUNT(*) FROM Contacts");
    if (count > 0) return;

    var contacts = new[]
    {
        new { LastName = "Иванов", FirstName = "Иван", MiddleName = "Иванович", Phone = "+79001234567" },
        new { LastName = "Петров", FirstName = "Петр", MiddleName = "Петрович", Phone = "+79007654321" },
        new { LastName = "Сидорова", FirstName = "Анна", MiddleName = "Сергеевна", Phone = "+79001112233" }
    };

    foreach (var c in contacts)
    {
        var id = connection.QuerySingle<int>(@"
            INSERT INTO Contacts (LastName, FirstName, MiddleName) 
            VALUES (@LastName, @FirstName, @MiddleName);
            SELECT last_insert_rowid();",
            new { c.LastName, c.FirstName, c.MiddleName });

        connection.Execute(@"
            INSERT INTO Phones (ContactId, PhoneNumber, PhoneType) 
            VALUES (@ContactId, @PhoneNumber, 'Mobile')",
            new { ContactId = id, PhoneNumber = c.Phone });
    }
}


public class Contact
{
    public int Id { get; set; }
    public string LastName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string? MiddleName { get; set; }
    public List<Phone> Phones { get; set; } = new();
}

public class Phone
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public string PhoneNumber { get; set; } = "";
    public string PhoneType { get; set; } = "Mobile";
}