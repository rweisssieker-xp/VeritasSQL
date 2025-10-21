# VeritasSQL - Natural Language to SQL Tool

A WPF desktop application that translates natural language questions into secure SQL SELECT queries and executes them.

## Overview

### Core Features

- **Natural Language Queries**: Formulate your database queries in plain English or German
- **OpenAI Integration**: Uses GPT-4 for intelligent translation to SQL
- **Schema Recognition**: Automatic loading of database schema (tables, views, columns, relationships)
- **Security Validation**: Multi-stage validation against SQL injection and unsafe operations
- **SQL Preview**: Review generated SQL before execution
- **Results Display**: Tabular display with sorting and filtering
- **History & Favorites**: Save and reuse previous queries
- **Export**: CSV and Excel export with optional metadata
- **Audit Trail**: Complete logging of all actions

### AI-Powered Features

- **Smart Query Suggestions**: AI generates intelligent query suggestions based on your database schema
- **Data Insights**: AI analyzes query results and provides actionable insights about data quality, patterns, and anomalies
- **Query Optimization**: AI-powered performance recommendations with optimized SQL rewrites
- **Schema Relationship Insights**: AI explains complex database relationships in business-friendly language
- **Smart Filters**: AI suggests useful filters based on actual data distribution

### Security Features

1. **Whitelist**: Only SELECT statements allowed
2. **Blacklist**: Blocks dangerous keywords (INSERT, UPDATE, DELETE, DROP, ALTER, EXEC, etc.)
3. **Schema Gate**: Only approved tables/views/columns can be referenced
4. **TOP Injection**: Automatic addition of row limits
5. **Read-Only**: No data modifications possible

## Installation

### Prerequisites

- Windows 10/11
- .NET 8.0 SDK or newer
- SQL Server access
- OpenAI API Key

### Build & Run

```bash
# Navigate to project
cd VeritasSQL

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run application
dotnet run --project VeritasSQL.WPF
```

## Quick Start

### 1. Configure OpenAI API Key

On first start:
1. Open Settings
2. Enter your OpenAI API Key (stored encrypted)
3. Select model (default: gpt-4)

### 2. Setup Database Connection

1. Click "New Connection"
2. Enter connection details:
   - **Name**: Descriptive name (e.g., "Production DB")
   - **Server**: SQL Server instance (e.g., `localhost` or `server.domain.com`)
   - **Database**: Database name
   - **Authentication**: Windows or SQL Server
3. Click "Test Connection"
4. Save profile

### 3. Load Schema

1. Select a connection profile
2. Click "Connect"
3. Click "Load Schema"
4. Schema overview appears in left panel

### 4. Execute Query

1. Enter your question in natural language:
   ```
   Show me the top 50 customers by revenue in the last year
   ```
2. Click "Generate SQL"
3. Review generated SQL in SQL editor
4. Check validation results in right panel
5. Click "Execute"
6. Results appear in "Results" tab

## Example Queries

```
Show all customers from Germany

Which products have a price over 100 dollars?

Top 10 orders sorted by date

Show customers with their orders (with join)

How many customers per country?

Show average order value per month in 2024
```

## AI Features in Detail

### 1. Smart Query Suggestions
After connecting and loading the schema, click **"Generate AI Suggestions"** to get:
- Common analytical queries (aggregations, trends, top N)
- Queries leveraging foreign key relationships (JOINs)
- Data quality checks
- Business-focused query templates

Each suggestion includes:
- **Title**: Short description
- **Category**: analytics | reporting | data_quality | relationships
- **Complexity**: low | medium | high
- **Natural Language Query**: Ready-to-use query text

Click any suggestion to automatically populate the query field.

### 2. AI Data Insights
After executing a query, click **"Analyze Data with AI"** to get:
- **Summary**: Overview of the data
- **Insights**: Statistical patterns and business trends
- **Data Quality Issues**: Null values, duplicates, outliers
- **Recommendations**: Suggestions for data cleanup or further analysis
- **Statistics**: Null percentage, unique values, potential duplicates

Perfect for exploratory data analysis and data quality monitoring.

### 3. Query Optimization
After generating SQL, click **"Optimize Query"** to get:
- **Performance Rating**: excellent | good | fair | poor
- **Optimized SQL**: Rewritten query for better performance
- **Recommendations**: Detailed suggestions with priorities
  - Index suggestions
  - JOIN optimization
  - WHERE clause improvements
  - Avoiding SELECT *
- **Estimated Improvement**: Expected performance gain

Click **"Apply Optimized SQL"** to use the improved query.

### 4. Schema Relationship Insights
Click **"Explain Schema"** to get AI-powered explanations of:
- How tables are related via foreign keys
- Business meaning of relationships
- Common query patterns for your schema
- Data model structure (fact/dimension tables)

Focus on a specific table to get detailed insights about its usage and relationships.

### 5. Smart Filters
Select a table and click **"Generate Smart Filters"** to get AI suggestions for:
- Common filter values (categories, status codes)
- Useful date ranges
- Outlier detection (min/max values)
- Aggregation groupings

Each filter includes:
- **Column**: Which field to filter
- **Filter Type**: equals | range | in | like | date_range
- **Suggested Value**: Example or recommended value
- **Reason**: Why this filter is useful

Click to automatically add filters to your natural language query.

## Architecture

### Project Structure

```
VeritasSQL/
├── VeritasSQL.Core/           # Business Logic
│   ├── Models/                # Data models
│   ├── Services/              # Core services
│   │   ├── ConnectionManager  # Connection management
│   │   ├── SchemaService      # Schema discovery
│   │   ├── OpenAIService      # OpenAI integration
│   │   └── QueryExecutor      # SQL execution
│   ├── Validation/            # Query validation
│   ├── Data/                  # Data persistence
│   │   ├── HistoryService     # History & favorites
│   │   └── AuditLogger        # Audit logging
│   └── Export/                # Export services
│       ├── CsvExporter
│       └── ExcelExporter
├── VeritasSQL.WPF/            # WPF UI
│   ├── ViewModels/            # MVVM ViewModels
│   ├── Views/                 # XAML Views
│   ├── Dialogs/               # Dialogs
│   └── Converters/            # Value Converters
└── VeritasSQL.Tests/          # Unit Tests
```

### Technology Stack

- **.NET 8**: Modern .NET platform
- **WPF**: Windows Presentation Foundation
- **MVVM**: Model-View-ViewModel pattern with CommunityToolkit.Mvvm
- **Microsoft.Data.SqlClient**: SQL Server access
- **OpenAI SDK**: GPT-4 integration
- **AvalonEdit**: SQL syntax highlighting
- **SQLite**: Local data storage (history/audit)
- **EPPlus**: Excel export
- **CsvHelper**: CSV export
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection

## Configuration

### Storage Locations

All configuration files are stored in `%AppData%\VeritasSQL`:

- `connections.json`: Connection profiles (with encrypted passwords via DPAPI)
- `settings.json`: Application settings (incl. encrypted API key)
- `domain_dictionary.json`: Synonyms/technical terms
- `history.db`: SQLite database for query history
- `audit.db`: SQLite database for audit log

### Settings

- **Default Row Limit**: Default row limit (default: 100)
- **Max Row Limit**: Maximum row limit (default: 10,000)
- **Query Timeout**: SQL timeout in seconds (default: 30)
- **OpenAI Model**: Model to use (default: gpt-4)
- **Dry Run by Default**: Preview mode enabled by default
- **Show Explanations**: Display explanations

## Security Notes

### Data Encryption

- **OpenAI API Key**: Encrypted with Windows DPAPI
- **Database Passwords**: Encrypted with Windows DPAPI
- **Scope**: CurrentUser (only current user can decrypt)

### SQL Validation

The application implements multiple security layers:

1. **Whitelist Check**: Only SELECT allowed
2. **Blacklist Check**: Dangerous keywords blocked
3. **Single Statement**: Only one statement per execution
4. **Schema Validation**: Only known objects
5. **TOP Injection**: Automatic row limits

**Important**: This application is designed for **Read-Only** access. Database users should only have SELECT permissions.

### Audit Logging

Every action is logged:
- Timestamp
- User
- Action (Connect, GenerateSQL, ExecuteQuery)
- Data source
- NL query and generated SQL
- Validation status
- Execution status and time
- Errors (if any)

## License

This project is licensed under the MIT License. See LICENSE file for details.

### Third-Party Licenses

- **EPPlus**: Polyform Noncommercial License 1.0.0
- **OpenAI SDK**: MIT License
- **AvalonEdit**: LGPL

## Roadmap

Planned features:

- [x] **AI Query Suggestions** - Schema-based intelligent query recommendations
- [x] **AI Data Insights** - Automated data quality and pattern analysis
- [x] **AI Query Optimization** - Performance recommendations and SQL rewrites
- [x] **AI Schema Insights** - Natural language explanations of database structure
- [x] **AI Smart Filters** - Data-driven filter suggestions
- [ ] PostgreSQL & MySQL support
- [ ] Parameter dialog for dynamic values
- [ ] Domain dictionary editor
- [x] SQL explanation feature
- [x] Iterative query refinement
- [ ] Favorites management with descriptions
- [ ] Export scheduler
- [ ] Dark/Light theme
- [ ] Multi-language support
- [ ] AI-powered anomaly detection in query results
- [ ] Automated query performance benchmarking

## Support & Contributions

For questions or issues, please open an issue in the repository.

Contributions are welcome! Please create a pull request.

## Disclaimer

This software is provided "as is". The author assumes no liability for damages or data loss through use of this software. Use this software at your own risk and only with appropriate permissions and backups.

**OpenAI Costs**: Using the OpenAI API incurs costs. Please check OpenAI's pricing.
