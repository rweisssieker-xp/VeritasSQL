# VeritasSQL - Quick Start Guide

Get up and running with VeritasSQL in 5 minutes.

---

## Prerequisites

- Windows 10/11
- .NET 8.0 SDK
- SQL Server (LocalDB, Express, or full)
- OpenAI API Key

---

## Step 1: Build and Run

```bash
# Clone or navigate to project
cd VeritasSQL

# Restore and build
dotnet restore
dotnet build

# Run the application
dotnet run --project VeritasSQL.WPF
```

The application launches with a First Run Wizard if no configuration exists.

---

## Step 2: Configure OpenAI API Key

On first launch, or via **⚙️ Settings**:

1. Click **⚙️ Settings** in the top-right corner
2. Enter your OpenAI API Key
3. Select model (recommended: `gpt-4`)
4. Click **Save**

> Your API key is encrypted using Windows DPAPI and stored securely.

---

## Step 3: Create Database Connection

1. Click **New** next to the Connection dropdown
2. Enter connection details:

| Field | Example |
|-------|---------|
| Name | My Database |
| Server | `localhost` or `(localdb)\MSSQLLocalDB` |
| Database | AdventureWorks2019 |
| Authentication | Windows |

3. Click **Test Connection** to verify
4. Click **Save**

---

## Step 4: Connect and Load Schema

1. Select your connection from the dropdown
2. Click **Connect**
3. Click **Load Schema**

The schema tree appears in the left panel showing all tables and views.

---

## Step 5: Your First Query

Type a question in natural language:

```
Show me the top 10 customers by name
```

Then:

1. Click **Generate SQL** (or press `Ctrl+G`)
2. Review the generated SQL
3. Click **Execute** (or press `Ctrl+Enter`)

Results appear in the data grid below.

---

## Step 6: Explore AI Features

Try these powerful AI features:

| Button | What it does |
|--------|--------------|
| **💡 Suggestions** | Get AI-generated query ideas |
| **🤖 AI Data Insights** | Analyze your results for patterns |
| **📝 AI Summary** | Get a natural language summary |
| **⚠️ Detect Anomalies** | Find data quality issues |
| **👁️ Preview** | See 5 rows before full execution |

---

## Example Queries

Try these natural language queries:

```
Show all customers from Germany

What are the top 10 products by price?

How many orders per month in 2023?

Show employees with their manager names

Which products have never been ordered?
```

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+G` | Generate SQL |
| `Ctrl+Enter` | Execute Query |
| `Ctrl+Shift+P` | Preview (5 rows) |
| `Ctrl+S` | Add to Favorites |
| `F5` | Reload Schema |

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "API Key not configured" | Go to Settings and enter your OpenAI API key |
| "Connection failed" | Check server name, ensure SQL Server is running |
| "Schema empty" | Verify database user has SELECT permissions |
| "Query blocked" | Only SELECT statements are allowed |

---

## Next Steps

- Read the full [User Manual](docs/user-manual.md)
- Explore the [Developer Guide](docs/developer-guide.md)
- Check out all [45 AI Features](README.md#ai-features)

---

## Important Notes

- **API Costs**: OpenAI calls cost ~$0.01-0.03 per request with GPT-4
- **Security**: Only SELECT queries are allowed (read-only)
- **Encryption**: Credentials are encrypted with Windows DPAPI
- **Audit**: All actions are logged for compliance

---

## Need Help?

- 📖 [Full Documentation](docs/index.md)
- 🐛 [Report Issues](../../issues)
- 💬 [Discussions](../../discussions)
