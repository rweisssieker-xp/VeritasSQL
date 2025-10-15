# VeritasSQL - Demo Setup

Step-by-step guide to test VeritasSQL with a demo database.

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB is sufficient)
- OpenAI API Key

## Step 1: Create Demo Database

Create a simple test database with SQL Server Management Studio or sqlcmd:

```sql
-- Create database
CREATE DATABASE VeritasSQLDemo;
GO

USE VeritasSQLDemo;
GO

-- Create sample tables
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100),
    Country NVARCHAR(50),
    CreatedDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50),
    Price DECIMAL(10,2),
    Stock INT
);

CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    CustomerID INT FOREIGN KEY REFERENCES Customers(CustomerID),
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(10,2)
);

-- Insert sample data
INSERT INTO Customers (FirstName, LastName, Email, Country) VALUES
('Max', 'Mustermann', 'max@example.com', 'Germany'),
('Anna', 'Schmidt', 'anna@example.com', 'Germany'),
('John', 'Doe', 'john@example.com', 'USA'),
('Jane', 'Smith', 'jane@example.com', 'UK'),
('Hans', 'Mueller', 'hans@example.com', 'Germany');

INSERT INTO Products (ProductName, Category, Price, Stock) VALUES
('Laptop', 'Electronics', 1299.99, 15),
('Mouse', 'Electronics', 29.99, 100),
('Keyboard', 'Electronics', 79.99, 50),
('Monitor', 'Electronics', 399.99, 25),
('USB Cable', 'Accessories', 9.99, 200);

INSERT INTO Orders (CustomerID, OrderDate, TotalAmount) VALUES
(1, '2024-01-15', 1329.98),
(2, '2024-01-20', 479.98),
(3, '2024-02-01', 109.98),
(1, '2024-02-15', 29.99),
(4, '2024-03-01', 1699.98);
```

## Step 2: Create Connection Profile

Create file `%AppData%\VeritasSQL\connections.json`:

```json
[
  {
    "Id": "demo-local",
    "Name": "VeritasSQL Demo (Local)",
    "DatabaseType": 0,
    "Server": "(localdb)\\MSSQLLocalDB",
    "Database": "VeritasSQLDemo",
    "AuthType": 0,
    "Username": null,
    "EncryptedPassword": null,
    "ConnectionTimeout": 30,
    "CreatedAt": "2025-10-15T00:00:00Z",
    "LastUsed": null
  }
]
```

For SQL Server Express or Standard:

```json
[
  {
    "Id": "demo-express",
    "Name": "VeritasSQL Demo (Express)",
    "DatabaseType": 0,
    "Server": "localhost\\SQLEXPRESS",
    "Database": "VeritasSQLDemo",
    "AuthType": 0,
    "Username": null,
    "EncryptedPassword": null,
    "ConnectionTimeout": 30,
    "CreatedAt": "2025-10-15T00:00:00Z",
    "LastUsed": null
  }
]
```

## Step 3: Configure Settings

Create file `%AppData%\VeritasSQL\settings.json`:

```json
{
  "EncryptedOpenAIApiKey": null,
  "DefaultRowLimit": 100,
  "MaxRowLimit": 10000,
  "QueryTimeoutSeconds": 30,
  "Language": "en-US",
  "DryRunByDefault": false,
  "ShowExplanations": true,
  "OpenAIModel": "gpt-4"
}
```

**Important**: The OpenAI API key must be encrypted. There are two options:

### Option A: Temporarily set in code (testing only)

Open `VeritasSQL.WPF/App.xaml.cs` and change:

```csharp
services.AddTransient<OpenAIService>(sp =>
{
    // For demo: enter API key directly here
    return new OpenAIService("sk-YOUR-API-KEY-HERE", "gpt-4");
});
```

### Option B: Settings Dialog (not yet implemented)

The Settings dialog for encrypted storage of the API key is not yet implemented. You can create it yourself or use Option A.

## Step 4: Start Application

```bash
cd C:\tmp\VeritasSQL
dotnet run --project VeritasSQL.WPF
```

## Step 5: First Steps in Application

1. **Select Connection**: Choose "VeritasSQL Demo" from dropdown
2. **Connect**: Click "Connect"
3. **Load Schema**: Click "Load Schema"
4. **Execute Query**: Enter a natural language question

### Example Queries

```
Show all customers from Germany

Which products cost more than 100 dollars?

Show the top 5 most expensive products

How many customers per country?

Show all orders with customer names

Which customer has the most orders?

Show products grouped by category

List all Electronics products with stock below 50
```

## Error Handling

### "Connection failed"

- Check if SQL Server LocalDB is installed:
  ```bash
  sqllocaldb info
  ```
- Start LocalDB:
  ```bash
  sqllocaldb start MSSQLLocalDB
  ```

### "OpenAI API Key not configured"

- Set the API key as described in Step 3
- Check if key is valid

### "Schema empty"

- Check if database exists
- Check if tables exist:
  ```sql
  SELECT * FROM INFORMATION_SCHEMA.TABLES
  ```

## Next Steps

After successful demo setup you can:

1. **Connect own databases**: Create more connection profiles
2. **Use history**: All queries are automatically saved
3. **Create favorites**: Mark frequently used queries (feature to be implemented)
4. **Test export**: Export results as CSV or Excel
5. **Get SQL explained**: Click "Explain SQL" for detailed descriptions

## Production Setup

For production use:

1. **Separate database users**: Create read-only users
2. **API Key management**: Implement Settings dialog
3. **Audit review**: Regularly check `%AppData%\VeritasSQL\audit.db`
4. **Backup**: Regularly backup connection profiles and history

## Cost Note

OpenAI API costs (approx. as of 2024):
- GPT-4: ~$0.03 per request (varies by prompt length)
- GPT-3.5-Turbo: ~$0.002 per request

For testing, using GPT-3.5-Turbo or setting an API budget limit is recommended.
