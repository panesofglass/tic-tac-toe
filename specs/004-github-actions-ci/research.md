# Research: GitHub Actions CI Workflow

**Branch**: `004-github-actions-ci` | **Date**: 2026-02-04

## 1. .NET 10.0 Preview Setup in GitHub Actions

### Decision
Use `actions/setup-dotnet@v5` with `dotnet-version: '10.0.x'` and `dotnet-quality: 'preview'` parameter.

### Rationale
- The `actions/setup-dotnet` action supports preview versions through the `dotnet-quality` parameter
- Using `10.0.x` with `preview` quality automatically gets the latest .NET 10 preview SDK
- Cleaner than hardcoding specific preview versions, which require manual updates

### Alternatives Considered
- **Exact version specification** - More deterministic but requires manual updates for each preview
- **Using `global.json`** - Project doesn't have one; adds maintenance overhead
- **Third-party `fast-actions/setup-dotnet`** - Less stable than official action

---

## 2. Playwright Browser Installation

### Decision
Install Playwright browsers using `playwright.ps1 install chromium --with-deps` from the build output directory.

### Rationale
- Project uses `Microsoft.Playwright.NUnit` which generates `playwright.ps1` script during build
- Tests only use Chromium (`TestBase.fs` line 52: `playwright.Chromium.LaunchAsync`)
- Installing only Chromium saves time compared to all browsers
- `--with-deps` installs required OS dependencies on Ubuntu runners

### Alternatives Considered
- **Install all browsers** - Unnecessary since tests only use Chromium
- **`microsoft/playwright-github-action`** - Deprecated; doesn't match installed Playwright version
- **Manual dependency installation** - Higher maintenance; `--with-deps` handles this automatically

---

## 3. Playwright Artifact Upload on Failure

### Decision
Use `actions/upload-artifact@v4` with `if: failure()` condition to upload traces only when tests fail.

### Rationale
- `if: failure()` ensures artifacts only uploaded when tests fail, saving storage
- `if-no-files-found: ignore` prevents upload step from failing when tests pass
- Traces provide the most useful debugging information for Playwright failures

### Configuration
```yaml
- name: Upload Playwright traces
  uses: actions/upload-artifact@v4
  if: failure()
  with:
    name: playwright-traces
    path: test/TicTacToe.Web.Tests/playwright-traces/
    if-no-files-found: ignore
    retention-days: 7
```

### Implementation Note
The `TestBase.fs` class needs enhancement to capture traces on failure. Add tracing start/stop in test setup/teardown, saving trace file only when test fails.

### Alternatives Considered
- **`if: always()`** - Uploads empty artifacts on success, wastes storage
- **Video recording** - Heavier than traces; overkill for most debugging

---

## 4. Web Application Startup for Tests

### Decision
Start the web application as a background process before running tests, using shell backgrounding with a health check wait.

### Rationale
- Current tests expect app running at `http://localhost:5000` (configurable via `TEST_BASE_URL`)
- Tests do **not** self-host the application; they connect to an externally running server
- Test setup throws exception if it cannot connect (TestBase.fs lines 70-71)
- Background process with health check is simpler than modifying test infrastructure

### Configuration
```yaml
- name: Start web application
  run: |
    dotnet run --project src/TicTacToe.Web/ --urls "http://localhost:5000" &
    for i in {1..30}; do
      curl -s http://localhost:5000 > /dev/null 2>&1 && break
      sleep 1
    done
```

### Alternatives Considered
- **WebApplicationFactory** - Requires significant refactoring of test infrastructure
- **Docker container** - Overkill for simple F# web app
- **No health check** - Risk of race condition where tests start before server ready

---

## Summary of Decisions

| Area | Decision | Key Benefit |
|------|----------|-------------|
| .NET Setup | `setup-dotnet@v5` with `dotnet-quality: preview` | Auto-updates to latest .NET 10 preview |
| Browser Install | `playwright.ps1 install chromium --with-deps` | Fast, installs only what's needed |
| Artifact Upload | `upload-artifact@v4` with `if: failure()` | Saves storage, artifacts only when useful |
| Web App Startup | Background process with curl health check | Works with existing test infrastructure |

---

## Outstanding Consideration: Test Tracing

The current `TestBase.fs` does not implement Playwright tracing. To capture screenshots and traces on failure:

1. **Option A (Recommended)**: Modify `TestBase.fs` to start/stop tracing conditionally
2. **Option B**: Accept log-only debugging for initial implementation, add tracing later

**Recommendation**: Proceed with Option B for initial workflow. Tracing enhancement can be a follow-up task if debugging proves difficult.
