# DESIGN.md

This file shows a general design of this code, and provides general guidance to AI code agents when working with code in this repository.

## Project overview

C# SDK for the Qarnot cloud computing platform. Published as a NuGet package (`QarnotSDK`). Multi-targets `netstandard2.0` and `net45`. Uses Newtonsoft.Json for serialization and AWS SDK for S3-compatible storage.

## Build and test commands

```bash
# Restore and build
dotnet restore
dotnet build -c Debug
dotnet build -c Release

# Run all unit tests
dotnet test -c Debug QarnotSDK.UnitTests/QarnotSDK.UnitTests.csproj

# Run a single test class
dotnet test -c Debug QarnotSDK.UnitTests/QarnotSDK.UnitTests.csproj --filter "FullyQualifiedName~TaskTests"

# Run a single test method
dotnet test -c Debug QarnotSDK.UnitTests/QarnotSDK.UnitTests.csproj --filter "FullyQualifiedName~TaskTests.MethodName"

# Integration tests (require Ceph S3 service — normally CI-only)
dotnet test -c Debug QarnotSDK.IntegrationTests/QarnotSDK.IntegrationTests.csproj
```

CI image: `mcr.microsoft.com/dotnet/sdk:8.0`. CI is GitLab (`.gitlab-ci.yml`).

## Architecture

### Three-layer design inside `QarnotSDK/`

**`Api/`** — Internal DTOs mapping 1:1 to REST API JSON (`TaskApi`, `PoolApi`, `JobApi`, `ConnectionApi`). These are plain POCOs for Newtonsoft.Json deserialization.

**`Sdk/`** — Public classes users interact with. Each wraps an internal `*Api` object:
- `Connection` — entry point; holds `HttpClient`, auth token, creates/retrieves all other objects
- `AQTask` (abstract) → `QTask`, `QTaskSummary` — task lifecycle
- `AQPool` (abstract) → `QPool`, `QPoolSummary` — pool lifecycle
- `QAbstractStorage` (abstract) → `QBucket` — S3-compatible storage (wraps AWS SDK)
- `QJob` — groups tasks

**`Tools/`** — Infrastructure: `RetryHandler`, HTTP client handlers/factories, custom JSON converters (`ScalingPolicyConverter`, `HardwareConstraintsJsonConverter`, `TimePeriodSpecificationConverter`), `Utils.cs`.

**`InternalAPI/`** — Extension methods on `Connection` for internal use.

### Partial class pattern

Each SDK class is split across multiple files:
- Main file (`Task.cs`): properties and state
- `*AsyncWrappers.cs` (`TaskAsyncWrappers.cs`): async methods (`SubmitAsync`, `RunAsync`, etc.)
- `Abstract*AsyncWrapper.cs`: deprecated synchronous wrappers marked `[Obsolete]`

### Data flow

```
API JSON → Newtonsoft.Json + custom converters → *Api DTO → Q* public wrapper
```

Public classes expose `*Api` properties through virtual getters and add logic (storage conversion, null checks, computed values). Properties have `[InternalDataApiName]` attributes for filtering/selection.

### Summary vs Full classes

`QTaskSummary` / `QPoolSummary` are lightweight versions of `QTask` / `QPool`, used in list/paginated operations. Both inherit from the same abstract base (`AQTask` / `AQPool`).

## Testing patterns

- **Framework**: NUnit 3 with Moq
- **HTTP mocking**: Custom handlers in `QarnotSDK.UnitTests/Mocks/` that extend `HttpClientHandler`:
  - `FakeHTTPHandler` — returns configurable static responses
  - `InterceptingFakeHttpHandler` — records all requests in `ParsedRequests` for assertion
  - `RetryFakeHTTPHandler` — returns N failures then success (retry testing)
  - `WaitHTTPHandler` — simulates state progression over time

Typical test setup creates a `Connection` with a fake handler:
```csharp
HttpHandler = new InterceptingFakeHttpHandler() { ResponseBody = TaskTestsData.TaskResponseFullBody };
Connect = new Connection(ApiUrl, StorageUrl, Token, HttpHandler);
```

Test data lives in `*TestData.cs` static classes with JSON string constants alongside each test class.

Verify HTTP calls via: `HttpHandler.ParsedRequests.Any(req => req.Method == "POST" && req.Uri.Contains("tasks"))`.

## Integration tests

Require a Ceph (S3-compatible) storage service. Controlled by env vars:
- `QARNOT_SDK_CSHARP_TESTS_USE_REAL_REMOTE_STORAGE=true`
- `QARNOT_SDK_CSHARP_TESTS_STORAGE_IP`
- `QARNOT_SDK_CSHARP_TESTS_STORAGE_ADMIN_ACCESS_KEY` / `SECRET_KEY`

In CI, a `ceph-nano` Docker service provides the storage backend.

## Code style

- StyleCop Analyzers and FxCop analyzers are configured on the unit test project
- `.gitattributes` enforces CRLF line endings (except shell scripts which use LF)
- XML doc comments are generated for all builds (`QarnotSDK.XML`)

### Language Settings
- C# 10.0
- 4 spaces indentation (no tabs)
- File-scoped namespaces: `namespace Qarnot.Module;`

### Naming Conventions
- **PascalCase**: Classes, interfaces, methods, properties, enums, constants
- **camelCase**: Local variables, parameters
- **_camelCase**: Private fields
- **I prefix**: Interfaces (e.g., `ILeaderInfoService`)
- **A prefix**: Abstract base classes (e.g., `ABaseClass`)

### Import Organization
- Place `using` statements outside namespace
- Order: .NET libraries (alphabetical) → blank line → third-party → blank line → internal
- when you need a new namespace in scope, add a `using`, don't use fully qualified names inline in code


### Bracing, indentation, and other coding style

First of all, this is for new or edited code. **Do not modify code just for bracing or indentation style**.

You'll follow mostly an Allman style:
- braces are alone on a new line, at the indentation level of the above line
- **DO NOT** omit braces for one-line bodies of loops or conditions
- every function declaration with 3 arguments or more have their arguments wrapped and aligned. Same for call sites, unless arguments are very short (like 3 integers or something like that)
- be agressive on extracting methods with meaningful names, even if they're called only once
- also use enough intermediate variables and use explicit names. It's OK if the names are a bit long. Not ridiculously long, but don't shorten them agressively.

As a general rule, let code breathe. Group lines that go together but feel free to insert a blank like to separate logical groups of lines.

