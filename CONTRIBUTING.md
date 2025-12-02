# Contributing to VeritasSQL

Thank you for your interest in contributing to VeritasSQL! This document provides guidelines and instructions for contributing.

---

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [Getting Started](#getting-started)
3. [How to Contribute](#how-to-contribute)
4. [Development Workflow](#development-workflow)
5. [Code Standards](#code-standards)
6. [Pull Request Process](#pull-request-process)
7. [Reporting Issues](#reporting-issues)

---

## Code of Conduct

This project follows a standard code of conduct. Please be respectful and constructive in all interactions.

- Be welcoming and inclusive
- Be respectful of differing viewpoints
- Accept constructive criticism gracefully
- Focus on what is best for the community

---

## Getting Started

### Prerequisites

- Windows 10/11
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Git
- SQL Server (LocalDB or Express for testing)
- OpenAI API Key (for testing AI features)

### Setup

```bash
# Fork the repository on GitHub

# Clone your fork
git clone https://github.com/YOUR-USERNAME/VeritasSQL.git
cd VeritasSQL

# Add upstream remote
git remote add upstream https://github.com/ORIGINAL-OWNER/VeritasSQL.git

# Install dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test

# Run application
dotnet run --project VeritasSQL.WPF
```

---

## How to Contribute

### Types of Contributions

- **Bug Fixes**: Fix issues reported in GitHub Issues
- **Features**: Implement new functionality
- **Documentation**: Improve docs, README, comments
- **Tests**: Add or improve test coverage
- **Performance**: Optimize existing code
- **Refactoring**: Improve code quality without changing behavior

### Finding Work

1. Check [GitHub Issues](../../issues) for open issues
2. Look for issues labeled `good first issue` or `help wanted`
3. Comment on an issue before starting work
4. Create an issue for new features before implementing

---

## Development Workflow

### Branch Strategy

```
main                    # Stable release branch
├── develop             # Integration branch
│   ├── feature/xxx     # New features
│   ├── bugfix/xxx      # Bug fixes
│   └── docs/xxx        # Documentation updates
```

### Creating a Branch

```bash
# Update your local main
git checkout main
git pull upstream main

# Create feature branch
git checkout -b feature/my-new-feature

# Or for bug fixes
git checkout -b bugfix/issue-123
```

### Making Changes

1. Write code following the [Code Standards](#code-standards)
2. Add or update tests as needed
3. Update documentation if applicable
4. Commit with clear messages

### Commit Messages

Use conventional commit format:

```
type(scope): description

[optional body]

[optional footer]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `style`: Formatting, no code change
- `refactor`: Code change that neither fixes nor adds
- `test`: Adding or updating tests
- `chore`: Build process or auxiliary tools

Examples:

```
feat(ai): add correlation analysis feature

fix(validation): handle null schema in validator

docs(readme): update installation instructions

test(executor): add timeout handling tests
```

---

## Code Standards

### C# Conventions

```csharp
// Naming
public class QueryValidator { }           // PascalCase for classes
public void ValidateQuery() { }           // PascalCase for methods
public string ConnectionString { get; }   // PascalCase for properties
private readonly IService _service;       // _camelCase for private fields
public void Process(string inputData) { } // camelCase for parameters

// Async methods
public async Task<Result> ProcessAsync()  // Suffix with Async

// Interfaces
public interface IQueryValidator { }      // Prefix with I
```

### File Organization

- One class per file
- File name matches class name
- Group related files in folders
- Keep files under 500 lines when possible

### MVVM Pattern

```csharp
// ViewModel properties
[ObservableProperty]
private string _myProperty;

// Commands
[RelayCommand]
private async Task DoSomethingAsync()
{
    // Implementation
}

// CanExecute
private bool CanDoSomething() => IsConnected && !IsBusy;
```

### Error Handling

```csharp
try
{
    await ProcessAsync();
}
catch (SpecificException ex)
{
    // Handle specific case
    Logger.LogWarning(ex, "Specific error occurred");
}
catch (Exception ex)
{
    // Log and rethrow or handle
    Logger.LogError(ex, "Unexpected error");
    throw;
}
```

### Documentation

```csharp
/// <summary>
/// Validates the SQL query against security rules.
/// </summary>
/// <param name="sql">The SQL query to validate.</param>
/// <param name="schema">The database schema for reference.</param>
/// <returns>Validation result with any errors or warnings.</returns>
/// <exception cref="ArgumentNullException">Thrown when sql is null.</exception>
public ValidationResult Validate(string sql, SchemaInfo schema)
```

---

## Pull Request Process

### Before Submitting

1. **Update from upstream**
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Run all tests**
   ```bash
   dotnet test
   ```

3. **Check for build warnings**
   ```bash
   dotnet build --warnaserror
   ```

4. **Update documentation** if needed

### Submitting

1. Push your branch to your fork
   ```bash
   git push origin feature/my-new-feature
   ```

2. Create Pull Request on GitHub

3. Fill out the PR template:
   - Description of changes
   - Related issue number
   - Type of change
   - Testing performed
   - Screenshots (for UI changes)

### PR Requirements

- [ ] Code follows project style guidelines
- [ ] Tests pass locally
- [ ] New code has appropriate tests
- [ ] Documentation updated if needed
- [ ] No merge conflicts with main
- [ ] PR has clear description

### Review Process

1. Maintainers will review within 1-2 weeks
2. Address any requested changes
3. Once approved, maintainer will merge
4. Delete your feature branch after merge

---

## Reporting Issues

### Bug Reports

Include:

1. **Description**: Clear description of the bug
2. **Steps to Reproduce**: Numbered steps to reproduce
3. **Expected Behavior**: What should happen
4. **Actual Behavior**: What actually happens
5. **Environment**: Windows version, .NET version
6. **Screenshots**: If applicable
7. **Logs**: Relevant error messages

### Feature Requests

Include:

1. **Problem**: What problem does this solve?
2. **Solution**: Proposed solution
3. **Alternatives**: Other solutions considered
4. **Context**: Why is this important?

### Security Issues

For security vulnerabilities, please email directly instead of creating a public issue.

---

## Questions?

- Check existing [Issues](../../issues) and [Discussions](../../discussions)
- Read the [Documentation](./docs/index.md)
- Open a new Discussion for questions

---

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to VeritasSQL! 🎉
