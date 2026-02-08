# Auth Endpoint Contracts

**Date**: 2026-02-07

## Authentication Behavior Contract

All resources marked with `requireAuth` follow the ASP.NET Core authorization pipeline:

### Unauthenticated Request Flow
1. Request arrives without valid auth cookie
2. ASP.NET Core authorization middleware intercepts (via `[Authorize]` metadata)
3. Cookie auth handler issues challenge → 302 redirect to `/login?returnUrl={original}`
4. User visits `/login` → auto-signs in with new GUID identity
5. 302 redirect back to `returnUrl`

### Protected Resources

| Endpoint | Method | Auth | Notes |
|----------|--------|------|-------|
| `GET /` | GET | Required | Home page |
| `POST /games` | POST | Required | Create game |
| `POST /games/{id}` | POST | Required | Make move |
| `GET /games/{id}` | GET | Required | View game page |
| `DELETE /games/{id}` | DELETE | Required | Delete game |
| `POST /games/{id}/reset` | POST | Required | Reset game |

### Unprotected Resources

| Endpoint | Method | Auth | Notes |
|----------|--------|------|-------|
| `GET /login` | GET | None | Auto-sign-in |
| `GET /logout` | GET | None | Sign out |
| `GET /debug` | GET | None | Debug info |
| `GET /sse` | GET | None | SSE stream (uses userId if available) |

### Cookie Configuration Contract

| Property | Value |
|----------|-------|
| Name | `TicTacToe.User` |
| HttpOnly | `true` |
| SameSite | `Strict` |
| SecurePolicy | `SameAsRequest` |
| ExpireTimeSpan | 30 days |
| SlidingExpiration | `true` |
| LoginPath | `/login` |
