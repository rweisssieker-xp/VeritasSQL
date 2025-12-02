# VeritasSQL User Manual

**Version 1.0.0 | Last Updated: 2025-12-02**

---

## Table of Contents

1. [Introduction](#introduction)
2. [Installation](#installation)
3. [First Launch Setup](#first-launch-setup)
4. [Main Interface](#main-interface)
5. [Database Connections](#database-connections)
6. [Writing Queries](#writing-queries)
7. [AI Features](#ai-features)
8. [Exporting Results](#exporting-results)
9. [History and Favorites](#history-and-favorites)
10. [Settings](#settings)
11. [Troubleshooting](#troubleshooting)

---

## Introduction

VeritasSQL is an AI-powered desktop application that translates your natural language questions into SQL queries. Instead of writing complex SQL, simply ask questions like:

- "Show me all customers from Germany"
- "What are the top 10 products by revenue?"
- "How many orders were placed last month?"

The application generates secure, validated SQL and executes it against your database.

### Key Benefits

- **No SQL Knowledge Required**: Ask questions in plain English or German
- **Enterprise Security**: Only SELECT queries allowed, 6-layer validation
- **AI-Powered Insights**: Automatic data analysis and recommendations
- **Audit Trail**: Every action is logged for compliance

---

## Installation

### System Requirements

- Windows 10 or Windows 11
- .NET 8.0 Runtime
- SQL Server access (local or remote)
- OpenAI API Key
- Minimum 4GB RAM
- 100MB disk space

### Installation Steps

1. Download the latest release from the repository
2. Extract the ZIP file to your preferred location
3. Run `VeritasSQL.WPF.exe`

Alternatively, build from source:

```bash
git clone https://github.com/your-repo/VeritasSQL.git
cd VeritasSQL
dotnet restore
dotnet build
dotnet run --project VeritasSQL.WPF
```

---

## First Launch Setup

When you first launch VeritasSQL, a setup wizard guides you through:

### Step 1: OpenAI API Key

1. Click **Settings** (⚙️) in the top-right corner
2. Enter your OpenAI API Key
3. Select your preferred model (default: gpt-4)
4. Click **Save**

> **Note**: Your API key is encrypted using Windows DPAPI and stored securely.

### Step 2: Database Connection

1. Click **New** next to the Connection dropdown
2. Fill in connection details:
   - **Name**: A friendly name (e.g., "Production DB")
   - **Server**: SQL Server instance (e.g., `localhost\SQLEXPRESS`)
   - **Database**: Database name
   - **Authentication**: Windows or SQL Server
3. Click **Test Connection** to verify
4. Click **Save**

---

## Main Interface

The application uses a three-panel layout:

```
┌─────────────────────────────────────────────────────────────────┐
│  Connection: [Dropdown] [New] [Edit] [Connect] [Load Schema]   │
├──────────────┬────────────────────────────┬─────────────────────┤
│              │                            │                     │
│   Schema     │   Query & Results          │   Validation        │
│   Browser    │                            │   & AI Insights     │
│              │   ┌──────────────────────┐ │                     │
│   Tables     │   │ Natural Language     │ │   Errors            │
│   Views      │   │ Query Input          │ │   Warnings          │
│   Columns    │   └──────────────────────┘ │   Info              │
│              │   ┌──────────────────────┐ │                     │
│   AI Tabs:   │   │ Generated SQL        │ │                     │
│   💡 Suggest │   │ Editor               │ │                     │
│   📚 Insights│   └──────────────────────┘ │                     │
│   🎯 Filters │   ┌──────────────────────┐ │                     │
│   💬 Chat    │   │ Results / History /  │ │                     │
│              │   │ Favorites Tabs       │ │                     │
│              │   └──────────────────────┘ │                     │
└──────────────┴────────────────────────────┴─────────────────────┘
│                        Status Bar                               │
└─────────────────────────────────────────────────────────────────┘
```

### Left Panel: Schema & AI Features

- **Schema Tab**: Browse tables, views, and columns
- **💡 Suggestions**: AI-generated query suggestions
- **📚 Insights**: Schema relationship explanations
- **🎯 Filters**: Smart filter recommendations
- **📋 Templates**: Pre-built query templates
- **🔮 Predictive**: "You might also ask" suggestions
- **🔗 JOIN Path**: Find optimal JOIN paths between tables
- **💬 Chat**: Conversational SQL assistant

### Center Panel: Query & Results

- **Natural Language Input**: Type your question here
- **SQL Editor**: View and edit generated SQL
- **Results Tab**: Query results in a data grid
- **History Tab**: Previous queries
- **Favorites Tab**: Saved queries

### Right Panel: Validation & Analysis

- **Validation Results**: Security checks and warnings
- **AI Analysis**: Data insights and recommendations

---

## Database Connections

### Creating a Connection

1. Click **New** in the connection bar
2. Enter connection details:

| Field | Description | Example |
|-------|-------------|---------|
| Name | Friendly identifier | "Sales Database" |
| Server | SQL Server instance | `localhost\SQLEXPRESS` |
| Database | Database name | `AdventureWorks` |
| Auth Type | Windows or SQL Server | Windows |
| Username | SQL login (if SQL Auth) | `sa` |
| Password | SQL password (encrypted) | `****` |

3. Click **Test Connection**
4. Click **Save**

### Connecting and Loading Schema

1. Select a connection from the dropdown
2. Click **Connect**
3. Click **Load Schema**
4. The schema tree populates with tables and views

### Managing Connections

- **Edit**: Modify existing connection settings
- **Delete**: Remove a connection profile
- All passwords are encrypted with Windows DPAPI

---

## Writing Queries

### Natural Language Queries

Type your question in plain language:

```
Show all customers from Germany

Which products cost more than $100?

Top 10 orders by total amount

How many employees per department?

Show orders from last month with customer names
```

### Generating SQL

1. Type your question in the natural language input
2. Click **Generate SQL** (or press `Ctrl+G`)
3. Review the generated SQL in the editor
4. Check validation results in the right panel
5. Click **Execute** (or press `Ctrl+Enter`)

### Previewing Results

Click **👁️ Preview** to see 5 sample rows before executing the full query.

### Editing SQL

You can manually edit the generated SQL in the editor. The application validates your changes in real-time.

### Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+G` | Generate SQL |
| `Ctrl+Enter` | Execute Query |
| `Ctrl+Shift+P` | Preview Query |
| `Ctrl+S` | Add to Favorites |
| `F5` | Reload Schema |
| `Ctrl+E` | Export to CSV |

---

## AI Features

### 💡 Smart Query Suggestions

After loading a schema, click **Generate AI Query Suggestions** to get:

- Common analytical queries
- Queries using table relationships
- Data quality checks
- Business-focused templates

Each suggestion shows:
- **Title**: Brief description
- **Category**: analytics, reporting, data_quality, relationships
- **Complexity**: low, medium, high

Click **Use This Query** to apply a suggestion.

### 🤖 AI Data Insights

After executing a query, click **🤖 AI Data Insights** to analyze results:

- **Summary**: Overview of the data
- **Patterns**: Statistical trends and distributions
- **Data Quality Issues**: Nulls, duplicates, outliers
- **Recommendations**: Suggestions for further analysis

### 📝 AI Summary

Click **📝 AI Summary** to get a natural language summary of your results, perfect for sharing with stakeholders.

### ⚠️ Anomaly Detection

Click **⚠️ Detect Anomalies** to find unusual patterns:

- Outliers in numeric columns
- Suspicious null patterns
- Data integrity issues

### 📊 Visualization Recommendations

Click **📊 Viz Recommendations** to get AI suggestions for the best chart types for your data.

### 🔧 Query Optimization

Click **Optimize Query** to get:

- Performance rating (Excellent/Good/Fair/Poor)
- Optimized SQL with improvements
- Index recommendations
- JOIN optimization suggestions

### 💬 Conversational Chat

Use the Chat tab for multi-turn conversations:

1. Ask a question
2. Follow up with "and also show..." or "filter that by..."
3. The AI remembers context from previous messages

### 🔗 JOIN Path Finder

Find the optimal path between two tables:

1. Enter source table name
2. Enter target table name
3. Click **Find JOIN Path**
4. Get the complete JOIN SQL

---

## Exporting Results

### CSV Export

1. Execute a query
2. Click **Export as CSV**
3. Choose delimiter (comma or semicolon)
4. Select save location

CSV files include metadata comments with:
- Query text
- Data source
- Export timestamp
- Row count

### Excel Export

1. Execute a query
2. Click **Export as Excel**
3. Select save location

Excel files include:
- Formatted data with headers
- Auto-fitted columns
- Metadata sheet with query info

---

## History and Favorites

### Query History

The History tab shows all executed queries with:

- Timestamp
- Natural language question
- Connection used
- Row count
- Execution time
- Success/failure status

**Actions:**
- Double-click to reload a query
- Search/filter history
- Export selected entries
- Delete old entries

### Favorites

Save frequently used queries:

1. Execute a query
2. Click **⭐ Add to Favorites** (or `Ctrl+S`)
3. Enter a name and description
4. Access from the Favorites tab

---

## Settings

Access settings via **⚙️ Settings** button.

### OpenAI Configuration

| Setting | Description | Default |
|---------|-------------|---------|
| API Key | Your OpenAI API key | (required) |
| Model | GPT model to use | gpt-4 |

### Query Settings

| Setting | Description | Default |
|---------|-------------|---------|
| Default Row Limit | Rows returned by default | 100 |
| Max Row Limit | Maximum allowed rows | 10,000 |
| Query Timeout | Seconds before timeout | 30 |
| Dry Run Mode | Preview without executing | Off |
| Show Explanations | Display SQL explanations | On |

### Storage Locations

All data is stored in `%AppData%\VeritasSQL`:

- `connections.json` - Connection profiles
- `settings.json` - Application settings
- `history.db` - Query history
- `audit.db` - Audit log

---

## Troubleshooting

### Connection Issues

**Problem**: Cannot connect to database

**Solutions**:
1. Verify server name and instance
2. Check if SQL Server is running
3. Verify firewall allows connections
4. Test with SQL Server Management Studio

### OpenAI Errors

**Problem**: "API Key not configured"

**Solution**: Go to Settings and enter your OpenAI API key.

**Problem**: "Model not found"

**Solution**: Ensure you're using a valid model name (gpt-4, gpt-3.5-turbo).

### Query Validation Errors

**Problem**: "Query blocked by security validation"

**Solutions**:
1. Only SELECT statements are allowed
2. Remove any INSERT, UPDATE, DELETE keywords
3. Ensure referenced tables exist in schema

### Performance Issues

**Problem**: Queries are slow

**Solutions**:
1. Use the Query Optimization feature
2. Add WHERE clauses to limit data
3. Reduce row limit in settings
4. Check database indexes

---

## Getting Help

- **Documentation**: Check the [docs folder](./index.md)
- **Issues**: Open an issue on GitHub
- **Logs**: Check `%AppData%\VeritasSQL\logs`

---

## Keyboard Shortcuts Reference

| Shortcut | Action |
|----------|--------|
| `Ctrl+G` | Generate SQL from natural language |
| `Ctrl+Enter` | Execute current query |
| `Ctrl+Shift+P` | Preview query (5 rows) |
| `Ctrl+S` | Add to favorites |
| `Ctrl+N` | New connection |
| `Ctrl+O` | Open settings |
| `Ctrl+H` | Refresh history |
| `Ctrl+E` | Export to CSV |
| `Ctrl+Shift+D` | Database First Date |
| `F5` | Reload schema |
