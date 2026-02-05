# Quickstart: GitHub Actions CI Workflow

**Branch**: `004-github-actions-ci` | **Date**: 2026-02-04

## What This Feature Does

Creates a GitHub Actions workflow that automatically:
- Builds the .NET 10.0 F# solution on every PR and push to main
- Runs unit tests (TicTacToe.Engine.Tests with Expecto)
- Runs Playwright browser tests (TicTacToe.Web.Tests)
- Reports pass/fail status back to the PR
- Uploads Playwright traces on test failure (for debugging)

## Files Created/Modified

| File | Action | Description |
|------|--------|-------------|
| `.github/workflows/ci.yml` | CREATE | Main CI workflow definition |

## How to Verify

### 1. Push the Branch
```bash
git push -u origin 004-github-actions-ci
```

### 2. Open a Pull Request
- Go to GitHub repository
- Create PR from `004-github-actions-ci` to `main`
- Observe the CI workflow trigger automatically

### 3. Check Workflow Run
- Navigate to Actions tab in GitHub
- Verify workflow shows:
  - ✅ Checkout
  - ✅ Setup .NET 10.0 Preview
  - ✅ Build
  - ✅ Run unit tests
  - ✅ Install Playwright browsers
  - ✅ Start web application
  - ✅ Run Playwright tests

### 4. Verify Failure Handling
To test failure artifact upload:
1. Temporarily break a test
2. Push and observe workflow failure
3. Check "Artifacts" section for `playwright-traces` (if tracing implemented)

## Local Development

Run the same steps locally before pushing:

```bash
# Build
dotnet build

# Run unit tests
dotnet test test/TicTacToe.Engine.Tests/

# Start web app (in separate terminal)
dotnet run --project src/TicTacToe.Web/ --urls "http://localhost:5000"

# Run Playwright tests (with app running)
dotnet test test/TicTacToe.Web.Tests/
```

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `TEST_BASE_URL` | `http://localhost:5000` | Base URL for Playwright tests |
| `TEST_TIMEOUT_MS` | `5000` | Timeout for page navigation |
| `HEADED` | `false` | Run browser in headed mode (debugging) |

## Troubleshooting

### Workflow Fails at .NET Setup
- .NET 10.0 is a preview; ensure `dotnet-quality: 'preview'` is set
- Check Microsoft's release schedule for SDK availability

### Playwright Tests Fail to Connect
- Ensure web app start step runs before Playwright tests
- Check health check curl loop succeeds before tests run
- Verify `TEST_BASE_URL` matches the `--urls` parameter

### Browser Installation Fails
- Verify `pwsh` is available (included in ubuntu-latest)
- Check the path to `playwright.ps1` matches build output directory
- Ensure build completes before browser install step
