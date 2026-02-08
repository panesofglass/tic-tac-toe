# Feature Specification: Frank 7.0 Framework Upgrade with Performance Optimization

**Feature Branch**: `001-frank-upgrade`
**Created**: 2026-02-07
**Status**: Draft
**Input**: User description: "Use Frank 7.0.0, Frank.Datastar 7.1.0, and Frank.Auth 7.0.0. Source code is available at ../frank. Use the example in ../frank/sample/Frank.Datastar.Oxpecker to use the stream overloads to avoid generating HTML strings and instead write directly into the response stream. Replace the Handlers.requireAuth with the Frank.Auth extensions for ResourceBuilder, and move configuration for cookie auth into the WebHostBuilder extensions. Measure performance before and after, and provide a report of the change. Note Oxpecker source code is available at ../Oxpecker."

## Clarifications

### Session 2026-02-07

- Q: Should streaming apply only to direct HTTP responses or also SSE broadcasts? → A: Stream everywhere by changing SseEvent DU to carry `TextWriter -> Task` callbacks instead of strings, using Frank.Datastar 7.1.0's native `Datastar.streamPatchElements (writer: TextWriter -> Task)` overloads combined with Oxpecker's `Render.toTextWriterAsync`. True zero-copy streaming — no intermediate string allocation at any point. Note: no caching benefit is lost — `broadcastPerRole` already renders per-subscriber (per userId), and `broadcast` is used only for identity-independent events (selectors, JSON, small uniform fragments).
- Q: Should Oxpecker.ViewEngine be upgraded alongside Frank? → A: Yes, upgrade Oxpecker.ViewEngine to 2.x. The Frank.Datastar.Oxpecker sample already targets 2.x, and staying on 1.x risks incompatibilities with the streaming patterns.
- Q: Should `requireAuth` scope expand beyond the current 2 resources? → A: Yes, expand to all game endpoints: `/games` POST (create), `/games/{id}` GET (view), POST (move), and DELETE (delete), plus existing `/` (home) and `/games/{id}/reset` POST. SSE and auth endpoints remain unprotected.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Framework Version Upgrade (Priority: P1)

Development team upgrades the application to use the latest Frank framework versions (7.0.0, Frank.Datastar 7.1.0, Frank.Auth 7.0.0) to access new features, bug fixes, and improved APIs.

**Why this priority**: Foundation for all other improvements. Without the framework upgrade, streaming optimizations and auth improvements cannot be implemented.

**Independent Test**: Can be fully tested by verifying application builds successfully, all existing tests pass, and application runs with identical functionality using the new framework versions.

**Acceptance Scenarios**:

1. **Given** the project references Frank 6.5.0, Frank.Datastar 6.5.0, Oxpecker.ViewEngine 1.1.0, **When** package references are updated to Frank 7.0.0, Frank.Datastar 7.1.0, Frank.Auth 7.0.0, Oxpecker.ViewEngine 2.x, **Then** the project builds without errors
2. **Given** the upgraded framework versions, **When** all existing unit and integration tests are executed, **Then** all tests pass with identical results
3. **Given** the application is running with new framework versions, **When** users interact with all existing features, **Then** all functionality works identically to the previous version

---

### User Story 2 - Authentication Modernization (Priority: P2)

Authentication configuration is refactored to use Frank.Auth 7.0.0 extensions, replacing custom `requiresAuth` handler wrapper with declarative `requireAuth` resource builder operations and moving cookie authentication configuration to WebHostBuilder extensions.

**Why this priority**: Simplifies authentication code, improves maintainability, and follows framework best practices. Can be tested independently by verifying all protected resources require authentication.

**Independent Test**: Can be fully tested by accessing protected resources without authentication (should redirect to login), authenticating, accessing protected resources (should succeed), and verifying cookie configuration matches previous behavior.

**Acceptance Scenarios**:

1. **Given** a resource is configured with `requireAuth`, **When** an unauthenticated user attempts to access it, **Then** the user is challenged and redirected to the login page
2. **Given** a user has authenticated successfully, **When** the user accesses a protected resource with `requireAuth`, **Then** the resource is served without additional authentication challenges
3. **Given** cookie authentication is configured via WebHostBuilder extensions, **When** a user signs in, **Then** authentication cookies are set with identical security properties (HttpOnly, SameSite, SecurePolicy, expiration) as the previous implementation
4. **Given** the authentication refactoring is complete, **When** all Handlers code is reviewed, **Then** the custom `requiresAuth` function has been removed and replaced with declarative resource builder operations
5. **Given** an unauthenticated user, **When** they attempt to create a game, make a move, or delete a game, **Then** the request is challenged (redirected to login)

---

### User Story 3 - HTML Streaming Optimization (Priority: P3)

HTML rendering is optimized to stream directly into the HTTP response instead of generating intermediate strings, reducing memory allocations and improving response time for page loads and SSE updates.

**Why this priority**: Performance optimization that builds on the framework upgrade. Measurably improves application performance but doesn't change user-facing functionality.

**Independent Test**: Can be fully tested by measuring memory allocations and response times before and after the change, verifying identical HTML output, and confirming performance metrics show improvement.

**Acceptance Scenarios**:

1. **Given** HTML templates are rendered for SSE broadcasts, **When** the SseEvent DU is refactored from `PatchElements of html: string` to carry `TextWriter -> Task` callbacks, **Then** the rendered HTML output is identical
2. **Given** baseline performance metrics are captured (response time, memory allocations, GC pressure), **When** streaming rendering is implemented across direct responses and SSE broadcasts, **Then** performance metrics show measurable improvement (reduced allocations, faster response times)
3. **Given** the streaming implementation uses Oxpecker's `toTextWriterAsync` combined with Frank.Datastar 7.1.0's `streamPatchElements`, **When** HTML is rendered for Datastar SSE events, **Then** rendering streams directly from view engine to HTTP response with no intermediate string allocations (true zero-copy)
4. **Given** the performance optimization is complete, **When** a performance report is generated, **Then** the report includes before/after metrics for memory allocations, response times, and GC pressure for representative operations (page load, game board updates, SSE broadcasts)

---

### Edge Cases

- What happens when Frank 7.0 APIs have breaking changes from 6.5? Implementation must identify and handle any API changes during upgrade.
- How does streaming handle errors during HTML rendering? Stream errors should not corrupt the response or leave connections in invalid state.
- What if performance degrades instead of improving? Performance measurements before and after must be captured to verify improvement hypothesis; if performance degrades, investigate and potentially revert streaming approach.
- How are concurrent SSE connections affected by streaming changes? Streaming must maintain thread-safety for concurrent broadcasts.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST upgrade to Frank 7.0.0, Frank.Datastar 7.1.0, Frank.Auth 7.0.0, and Oxpecker.ViewEngine 2.x
- **FR-002**: System MUST replace custom `requiresAuth` handler wrapper with Frank.Auth's `requireAuth` resource builder operation, applied to all game endpoints (`/` GET, `/games` POST, `/games/{id}` GET, POST, and DELETE, `/games/{id}/reset` POST). SSE (`/sse`), debug (`/debug`), and auth endpoints (`/login`, `/logout`) MUST remain unprotected
- **FR-003**: System MUST move cookie authentication configuration from `configureServices` function to WebHostBuilder extensions provided by Frank.Auth
- **FR-004**: System MUST use Oxpecker's streaming render functions (`toStreamAsync`, `toTextWriterAsync`) combined with Frank.Datastar 7.1.0's native stream overloads (`Datastar.streamPatchElements`) instead of `Render.toString` for all HTML generation, including SSE broadcasts. The SseEvent DU MUST be refactored to carry `TextWriter -> Task` callbacks instead of pre-rendered strings, enabling true zero-copy streaming from view engine through SSE to HTTP response
- **FR-005**: System MUST maintain identical authentication behavior (redirects, cookie properties, session lifetime) after refactoring
- **FR-006**: System MUST maintain identical HTML output (byte-for-byte where possible) after streaming refactoring
- **FR-007**: System MUST capture baseline performance metrics before implementing streaming changes
- **FR-008**: System MUST capture performance metrics after implementing streaming changes
- **FR-009**: System MUST generate a performance report comparing before and after metrics
- **FR-010**: All existing unit tests MUST continue to pass without modification
- **FR-011**: All existing integration tests MUST continue to pass without modification
- **FR-012**: System MUST maintain thread-safety for concurrent SSE broadcasts when using streaming

### Key Entities

- **Performance Metrics**: Measurements captured before and after optimization including response time (milliseconds), memory allocations (bytes), garbage collection pressure (frequency/duration), throughput (requests per second)
- **Framework Dependencies**: Package references with specific versions (Frank 7.0.0, Frank.Datastar 7.1.0, Frank.Auth 7.0.0, Oxpecker.ViewEngine 2.x)
- **Authentication Configuration**: Cookie settings including name, security properties (HttpOnly, SameSite, SecurePolicy), expiration, login path

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Application builds successfully with Frank 7.0.0, Frank.Datastar 7.1.0, Frank.Auth 7.0.0, and Oxpecker.ViewEngine 2.x without compilation errors
- **SC-002**: 100% of existing unit tests (43 tests) pass after framework upgrade
- **SC-003**: 100% of existing integration tests pass after framework upgrade (when server is running)
- **SC-004**: Memory allocations for rendering game board reduce by at least 40% compared to baseline (measured via BenchmarkDotNet or equivalent)
- **SC-005**: Response time for page loads improves by at least 15% compared to baseline (measured at 50th and 95th percentile)
- **SC-006**: HTML output remains identical for all templates after streaming refactoring (verified via snapshot testing or byte comparison)
- **SC-007**: Authentication behavior remains unchanged (verified by existing authentication tests passing without modification)
- **SC-008**: Performance report is generated documenting before/after metrics for at least: memory allocations, response times (p50, p95, p99), garbage collection events, and throughput

## Assumptions

- Frank 7.0.0, Frank.Datastar 7.1.0, and Frank.Auth 7.0.0 are API-compatible with the existing usage patterns (or any breaking changes are documented and can be addressed)
- Oxpecker 2.x streaming APIs (`toStreamAsync`, `toTextWriterAsync`) are compatible with the upgraded framework versions
- Performance improvements from streaming are measurable with existing tooling (BenchmarkDotNet, profilers, or custom metrics)
- The Frank.Auth WebHostBuilder extensions support the same cookie authentication configuration options as the current manual configuration
- The performance benefits of streaming (reduced allocations, faster rendering) outweigh any additional complexity
- Source code for Frank and Oxpecker at ../frank and ../Oxpecker is available and can be referenced for API usage examples
