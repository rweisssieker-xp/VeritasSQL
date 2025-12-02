# Changelog

All notable changes to VeritasSQL are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2025-12-02

### Added

#### Core Features
- Natural language to SQL translation using OpenAI GPT-4
- Multi-language support (English and German queries)
- SQL syntax highlighting with AvalonEdit editor
- Query preview mode (5 rows before full execution)
- Query history with SQLite storage
- Favorites system for frequently used queries
- CSV export with metadata comments
- Excel export with formatting

#### AI Features (45 Total)
- **Smart Query Suggestions**: AI-generated query templates based on schema
- **AI Data Insights**: Automated pattern and anomaly detection
- **Query Optimization**: Performance recommendations and SQL rewrites
- **Schema Relationship Insights**: Natural language schema explanations
- **Smart Filters**: Data-driven filter suggestions
- **SQL Error Explanation**: AI-powered error analysis and auto-fix
- **Natural Language Summaries**: Business-friendly result descriptions
- **Anomaly Detection**: Proactive data quality monitoring
- **Semantic History Search**: Intent-based query history search
- **Visualization Recommendations**: AI chart type suggestions
- **Query Co-Pilot**: Real-time auto-complete suggestions
- **Predictive Next Queries**: Netflix-style "you might also ask"
- **JOIN Path Finder**: Optimal path discovery between tables
- **Data Profiling & PII Detection**: GDPR compliance scanning
- **Conversational Chat**: Multi-turn context-aware assistant
- **Dashboard Generator**: Instant BI dashboard creation
- **Data Quality Score**: 0-100 rating with recommendations
- **Business Impact Analysis**: Schema change prediction
- **Voice-to-SQL**: Speech recognition with Whisper API
- **Query Cost Estimator**: Cloud cost prediction
- **Correlation Finder**: Hidden relationship discovery
- **Statistical Test Recommender**: Data science assistance
- **Data Storytelling**: Narrative generation from data

#### Security
- 6-layer query validation pipeline
- SELECT-only enforcement (read-only access)
- Windows DPAPI encryption for credentials
- Complete audit trail logging
- Schema gate validation

#### Enterprise Features
- Connection profile management
- Domain dictionary for custom term mapping
- Configurable row limits and timeouts
- Dry run mode for query preview

### Technical
- .NET 8.0 with WPF
- MVVM architecture with CommunityToolkit.Mvvm
- Dependency injection with Microsoft.Extensions.DependencyInjection
- SQLite for local storage
- Async/await throughout

---

## [Unreleased]

### Planned
- PostgreSQL support
- MySQL support
- Dark/Light theme toggle
- Multi-language UI localization
- Export scheduler
- Plugin system

---

## Version History

| Version | Date | Highlights |
|---------|------|------------|
| 1.0.0 | 2025-12-02 | Initial release with 45 AI features |

---

## Migration Notes

### Upgrading to 1.0.0

This is the initial release. No migration required.

### Data Locations

All user data is stored in `%AppData%\VeritasSQL`:
- `connections.json` - Connection profiles
- `settings.json` - Application settings
- `history.db` - Query history
- `audit.db` - Audit log

---

## Deprecation Notices

None at this time.

---

## Security Advisories

None at this time.

---

## Contributors

See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines.
