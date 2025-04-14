# Key Decisions Log

## Architecture Decisions

1. Use .NET with MinimalAPI and RazorSlices [2025-03-26]
   - Reason: Better support for hypermedia approach
   - Alternatives Considered: Node.js, Go
   - Impact: Simplified server-side rendering

2. Immutable Game State [2025-03-26]
   - Decision: Use C# records for game state
   - Rationale: Ensures state consistency
   - Impact: Simplified debugging and testing

3. Progressive Enhancement [2025-03-27]
   - Decision: Render different HTML for valid/invalid moves
   - Rationale: Better user experience and security
   - Impact: Reduced client-side complexity

## Model Decisions

1. Move Record Structure [2025-03-27]
   - Decision: Use byte for row/column
   - Rationale: Type safety and efficiency
   - Impact: Better validation at compile time

2. Game Status Implementation [2025-03-27]
   - Decision: Game record implements IGameStatus
   - Rationale: Unified state management
   - Impact: Cleaner rendering logic

## Technical Decisions

1. SSE for Updates [2025-03-27]
   - Decision: Use SSE over WebSockets
   - Rationale: Simpler, one-way updates
   - Impact: Reduced complexity

2. HTML Fragment Updates [2025-03-27]
   - Decision: Return HTML fragments for moves
   - Rationale: Consistent with hypermedia approach
   - Impact: Simplified client-side logic

## Pending Decisions

1. Game Persistence
   - Options: In-memory, SQL Database, NoSQL
   - Considerations: Scalability, Complexity
   - Timeline: After basic gameplay works

2. Player Management
   - Options: Anonymous, User Accounts
   - Considerations: User Experience, Complexity
   - Timeline: After game persistence
