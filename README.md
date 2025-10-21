# VeritasSQL - AI-Powered Data Analysis Assistant

A WPF desktop application that translates natural language questions into secure SQL SELECT queries and executes them - enhanced with **5 innovative AI features** that go far beyond simple translation.

## Overview

VeritasSQL transforms natural language questions into secure SQL queries with **enterprise-grade security**, **AI-powered insights**, and **intelligent automation**. More than just a translator - it's your complete data analysis assistant.

### 🌟 Key Highlights

- **🔤 Natural Language to SQL**: English & German query translation with GPT-4
- **🤖 5 AI Features**: Query Suggestions, Data Insights, Optimization, Schema Insights, Smart Filters
- **🔒 Enterprise Security**: 6-layer validation, encryption, audit trail, read-only enforcement
- **📊 Complete Workflow**: Schema exploration → Query generation → Execution → Export → History
- **🎨 Beautiful UI**: 3-panel layout with tabbed navigation and real-time validation
- **📤 Export Ready**: CSV & Excel with metadata for compliance and reporting
- **🏢 Enterprise-Grade**: Audit logging, encrypted credentials, SOX/GDPR/HIPAA ready

---

## ✨ All Features

### 🔤 Natural Language to SQL

- **Multi-Language Support**: English and German natural language queries
- **GPT-4 Translation**: Intelligent SQL generation with context awareness
- **Domain Dictionary**: Custom business term mapping (e.g., "customers" → "Customers" table)
- **Iterative Refinement**: Ask AI to improve or modify generated SQL
- **SQL Explanation**: Get plain-language explanations of complex SQL

### 🤖 AI-Powered Intelligence (NEW!)

#### 1. Smart Query Suggestions
- **Auto-Generated Templates**: AI analyzes your schema and suggests useful queries
- **Categorized Suggestions**: Analytics, Reporting, Data Quality, Relationships
- **Complexity Ratings**: Low, Medium, High difficulty indicators
- **One-Click Application**: Instantly populate query field
- **Business-Focused**: Queries designed for real business value

#### 2. AI Data Insights
- **Automated Analysis**: AI analyzes query results for patterns and anomalies
- **Data Quality Monitoring**: Null detection, duplicate identification, outlier detection
- **Statistical Analysis**: Null percentage, unique values, row counts
- **Business Insights**: Trends, distributions, and actionable findings
- **Recommendations**: Suggestions for data cleanup and further investigation

#### 3. Query Performance Optimization
- **AI Performance Rating**: Excellent, Good, Fair, Poor classifications
- **Optimized SQL Generation**: AI rewrites queries for better performance
- **Detailed Recommendations**:
  - Index suggestions with CREATE INDEX statements
  - JOIN optimization (INNER vs LEFT, order optimization)
  - Query rewriting (avoiding SELECT *, subquery optimization)
  - WHERE clause improvements
- **Impact Assessment**: Estimated performance gains
- **One-Click Application**: Apply optimized SQL instantly

#### 4. Schema Relationship Insights
- **Natural Language Explanations**: Business-friendly database structure descriptions
- **Relationship Mapping**: How tables connect via foreign keys
- **Common Query Patterns**: Suggested JOINs and typical queries
- **Data Model Analysis**: Fact table vs dimension table identification
- **Table-Specific Insights**: Focus on individual table usage and connections

#### 5. Smart Filters
- **Data-Driven Suggestions**: Based on actual data distribution (samples top 100 rows)
- **Filter Types**: equals, range, in, like, date_range
- **Contextual Reasoning**: Explains why each filter is useful
- **Common Values**: Most frequent categories, statuses, date ranges
- **Outlier Detection**: Min/max values worth filtering
- **One-Click Application**: Add filters to query automatically

### 🗄️ Database & Schema

- **Connection Management**: Save and manage multiple database connections
- **Connection Profiles**: Named profiles with encrypted credentials
- **Connection Testing**: Test before use to ensure validity
- **Automatic Schema Loading**: Tables, views, columns, data types, primary keys, foreign keys
- **Schema Tree Visualization**: Hierarchical display with search/filter
- **Schema Metadata**: Column data types, nullability, max length, relationships
- **Multi-Database Support**: SQL Server (PostgreSQL/MySQL planned)

### 🔒 Enterprise Security

#### Multi-Layer Validation (6 Stages)
1. **Whitelist Validation**: Only SELECT statements allowed
2. **Blacklist Validation**: Dangerous keywords blocked (INSERT, UPDATE, DELETE, DROP, ALTER, EXEC, xp_cmdshell, etc.)
3. **Single Statement Check**: Prevents multiple statements (no semicolons)
4. **Schema Gate Validation**: Only approved tables/views/columns can be referenced
5. **TOP Injection**: Automatic addition of row limits (prevents runaway queries)
6. **Performance Warnings**: Missing WHERE clauses, SELECT * usage

#### Data Protection
- **Read-Only Enforcement**: No data modifications possible
- **Encrypted Credentials**: Windows DPAPI encryption for passwords and API keys
- **CurrentUser Scope**: Only logged-in user can decrypt secrets
- **No Plaintext Storage**: All sensitive data encrypted at rest

#### Audit & Compliance
- **Complete Audit Trail**: Every action logged with timestamp, user, action type
- **Forensic Details**: Natural language query, generated SQL, validation status, execution status
- **Performance Metrics**: Row counts, execution times
- **Error Logging**: Full error messages for troubleshooting
- **SQLite Storage**: Tamper-proof append-only audit database
- **Export Capability**: Audit logs exportable for compliance reporting

### 📊 Query Execution & Results

- **Syntax Highlighting**: SQL editor with color-coded syntax (AvalonEdit)
- **Line Numbers**: Easy reference and debugging
- **Real-Time Validation**: Instant feedback on query safety
- **Validation Severity Levels**: Error (red), Warning (orange), Info (blue)
- **Row Count Estimation**: Pre-execution estimate to prevent large result sets
- **Execution Timeout**: Configurable timeout protection (default: 30s)
- **Performance Metrics**: Execution time tracking (milliseconds)
- **Sortable DataGrid**: Click column headers to sort
- **Filterable Results**: Built-in DataGrid filtering
- **Auto-Column Generation**: Automatic column detection from results

### 📜 History & Favorites

- **Query History**: SQLite-based storage of all executed queries
- **Rich Metadata**: Natural language query, SQL, connection, timestamp, row count, execution time
- **Success/Error Tracking**: Know which queries worked or failed
- **Searchable History**: Filter by query text, SQL, or connection profile
- **Favorites System**: Star queries with custom names and descriptions
- **Reusability**: Double-click to reload any historical query
- **Timestamp Sorting**: Most recent queries first
- **Connection Association**: Know which database each query ran against

### 📤 Export Capabilities

#### CSV Export
- **Delimiter Options**: Comma or semicolon (European format)
- **UTF-8 Encoding**: International character support
- **Metadata Comments**: Embedded query, data source, row count
- **Timestamp Headers**: Export date/time in file
- **SQL Preservation**: Original query embedded as comments

#### Excel Export (.xlsx)
- **Professional Formatting**: Auto-fitted columns, headers
- **Type-Aware**: Proper formatting for dates, numbers, text
- **Metadata Sheet**: Separate info section with branding
- **Query Documentation**: SQL embedded in worksheet
- **EPPlus Library**: Industry-standard Excel generation

### 🎨 User Interface

- **3-Panel Layout**: Schema (left) | Query/Results (center) | Validation (right)
- **Tabbed Navigation**: Results, History, Favorites tabs
- **AI Features Tabs**: Query Suggestions, Schema Insights, Smart Filters
- **Responsive Design**: No UI freezing during operations (async/await)
- **Progress Indicators**: Status bar with progress animation
- **Color-Coded Feedback**: Visual severity indicators
- **Tooltips Everywhere**: Helpful descriptions on all buttons
- **Emoji Icons**: Visual recognition for AI features
- **Search/Filter**: Schema tree filtering, history search
- **Keyboard Shortcuts**: Efficient navigation

### ⚙️ Configuration & Settings

- **OpenAI Model Selection**: gpt-4, gpt-3.5-turbo, etc.
- **Row Limits**: Default and maximum configurable
- **Query Timeout**: Customizable timeout duration
- **Dry Run Mode**: Preview without execution
- **Show Explanations**: Toggle SQL explanations
- **AppData Storage**: All configs in `%AppData%\VeritasSQL`
- **JSON Configuration**: Human-readable settings files
- **Encrypted Secrets**: DPAPI-protected sensitive data

### 🏗️ Technical Features

- **MVVM Architecture**: Clean separation of concerns
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Async/Await**: Non-blocking I/O operations
- **Observable Properties**: CommunityToolkit.Mvvm source generators
- **Command Pattern**: RelayCommand with CanExecute validation
- **Repository Pattern**: Service-based data access
- **SQLite Storage**: Local history and audit databases
- **JSON Serialization**: Newtonsoft.Json for configs
- **Error Handling**: Graceful degradation with user-friendly messages
- **Logging**: Comprehensive audit and error logging

---

## 📈 Feature Statistics

| Category | Feature Count |
|----------|--------------|
| **AI Features** | 5 major features (20+ sub-features) |
| **Security Layers** | 6-stage validation pipeline |
| **Export Formats** | 2 (CSV, Excel) |
| **Database Support** | SQL Server (PostgreSQL/MySQL planned) |
| **UI Panels** | 3-panel responsive layout |
| **Tab Views** | 7 tabs (Schema, Suggestions, Insights, Filters, Results, History, Favorites) |
| **Validation Severity Levels** | 3 (Error, Warning, Info) |
| **Encryption Methods** | Windows DPAPI |
| **Storage Databases** | 2 SQLite (history.db, audit.db) |
| **NuGet Packages** | 12 |
| **ViewModel Commands** | 20+ commands |
| **Model Classes** | 15+ models |

### 🆚 VeritasSQL vs. Competitors

| Feature | VeritasSQL | Other NL-to-SQL Tools |
|---------|------------|----------------------|
| Natural Language Translation | ✅ | ✅ |
| AI Query Suggestions | ✅ | ❌ |
| AI Data Insights | ✅ | ❌ |
| AI Query Optimization | ✅ | ❌ |
| AI Schema Insights | ✅ | ❌ |
| Smart Filters | ✅ | ❌ |
| 6-Layer Security | ✅ | ⚠️ (basic) |
| Audit Trail | ✅ | ⚠️ (limited) |
| Encrypted Credentials | ✅ | ⚠️ (varies) |
| Export with Metadata | ✅ | ⚠️ (basic) |
| Query History | ✅ | ✅ |
| Favorites | ✅ | ⚠️ (varies) |
| Schema Visualization | ✅ | ✅ |
| Domain Dictionary | ✅ | ❌ |
| Iterative Refinement | ✅ | ❌ |

**Legend**: ✅ Full Support | ⚠️ Partial/Limited | ❌ Not Available

---

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
