module TicTacToe.Web.Tests.PlayerAssignmentTests

open Expecto
open TicTacToe.Web.Model

/// Unit tests for PlayerAssignment types and PlayerAssignmentManager
/// These tests verify the core player assignment logic independent of HTTP handlers

// =============================================================================
// T004: Tests for PlayerAssignment record type
// =============================================================================
[<Tests>]
let playerAssignmentRecordTests =
    testList
        "PlayerAssignment Record Type"
        [ test "createAssignment creates empty assignment with correct gameId" {
              let assignment = createAssignment "game-123"
              Expect.equal assignment.GameId "game-123" "GameId should match"
              Expect.isNone assignment.PlayerXId "PlayerXId should be None"
              Expect.isNone assignment.PlayerOId "PlayerOId should be None"
          }

          test "PlayerAssignment can have PlayerXId assigned" {
              let assignment =
                  { createAssignment "game-1" with
                      PlayerXId = Some "user-x" }

              Expect.equal assignment.PlayerXId (Some "user-x") "PlayerXId should be set"
              Expect.isNone assignment.PlayerOId "PlayerOId should still be None"
          }

          test "PlayerAssignment can have both players assigned" {
              let assignment =
                  { createAssignment "game-1" with
                      PlayerXId = Some "user-x"
                      PlayerOId = Some "user-o" }

              Expect.equal assignment.PlayerXId (Some "user-x") "PlayerXId should be set"
              Expect.equal assignment.PlayerOId (Some "user-o") "PlayerOId should be set"
          } ]

// =============================================================================
// T005: Tests for PlayerRole discriminated union
// =============================================================================
[<Tests>]
let playerRoleTests =
    testList
        "PlayerRole Discriminated Union"
        [ test "PlayerRole has all expected cases" {
              // Verify all cases exist and are distinct
              let roles = [ PlayerX; PlayerO; Spectator; UnassignedX; UnassignedO ]
              Expect.equal (List.length roles) 5 "Should have 5 role types"
              Expect.equal (List.distinct roles |> List.length) 5 "All roles should be distinct"
          }

          test "PlayerRole pattern matching works correctly" {
              let describe role =
                  match role with
                  | PlayerX -> "X"
                  | PlayerO -> "O"
                  | Spectator -> "spectator"
                  | UnassignedX -> "unassigned-x"
                  | UnassignedO -> "unassigned-o"

              Expect.equal (describe PlayerX) "X" "PlayerX description"
              Expect.equal (describe PlayerO) "O" "PlayerO description"
              Expect.equal (describe Spectator) "spectator" "Spectator description"
              Expect.equal (describe UnassignedX) "unassigned-x" "UnassignedX description"
              Expect.equal (describe UnassignedO) "unassigned-o" "UnassignedO description"
          } ]

// =============================================================================
// T006: Tests for MoveValidationResult and RejectionReason types
// =============================================================================
[<Tests>]
let moveValidationResultTests =
    testList
        "MoveValidationResult Type"
        [ test "Allowed wraps PlayerRole correctly" {
              let result = Allowed PlayerX

              match result with
              | Allowed role -> Expect.equal role PlayerX "Should contain PlayerX role"
              | Rejected _ -> failtest "Should not be Rejected"
          }

          test "Rejected wraps RejectionReason correctly" {
              let result = Rejected NotYourTurn

              match result with
              | Allowed _ -> failtest "Should not be Allowed"
              | Rejected reason -> Expect.equal reason NotYourTurn "Should contain NotYourTurn reason"
          }

          test "RejectionReason has all expected cases" {
              let reasons = [ NotYourTurn; NotAPlayer; WrongPlayer; GameOver ]
              Expect.equal (List.length reasons) 4 "Should have 4 rejection reasons"
              Expect.equal (List.distinct reasons |> List.length) 4 "All reasons should be distinct"
          }

          test "RejectionReason pattern matching works correctly" {
              let describe reason =
                  match reason with
                  | NotYourTurn -> "not-your-turn"
                  | NotAPlayer -> "not-a-player"
                  | WrongPlayer -> "wrong-player"
                  | GameOver -> "game-over"

              Expect.equal (describe NotYourTurn) "not-your-turn" "NotYourTurn description"
              Expect.equal (describe NotAPlayer) "not-a-player" "NotAPlayer description"
              Expect.equal (describe WrongPlayer) "wrong-player" "WrongPlayer description"
              Expect.equal (describe GameOver) "game-over" "GameOver description"
          } ]

// =============================================================================
// T011: Tests for PlayerAssignmentManager
// =============================================================================
[<Tests>]
let playerAssignmentManagerTests =
    testList
        "PlayerAssignmentManager"
        [ test "GetRole returns UnassignedX for new game" {
              let manager = PlayerAssignmentManager()
              let role = manager.GetRole("new-game", "any-user")
              Expect.equal role UnassignedX "New game should have UnassignedX role"
          }

          test "GetAssignment returns None for non-existent game" {
              let manager = PlayerAssignmentManager()
              let assignment = manager.GetAssignment("non-existent")
              Expect.isNone assignment "Should return None for non-existent game"
          }

          test "TryAssignAndValidate assigns X on first move" {
              let manager = PlayerAssignmentManager()
              let (result, assignment) = manager.TryAssignAndValidate("game-1", "user-x", true)

              match result with
              | Allowed PlayerX -> ()
              | _ -> failtest $"Expected Allowed PlayerX, got {result}"

              Expect.equal assignment.PlayerXId (Some "user-x") "User should be assigned as X"
              Expect.isNone assignment.PlayerOId "O should still be unassigned"
          }

          test "RemoveGame clears assignment" {
              let manager = PlayerAssignmentManager()

              // First assign a player
              let _ = manager.TryAssignAndValidate("game-1", "user-x", true)

              // Verify assignment exists
              let assignmentBefore = manager.GetAssignment("game-1")
              Expect.isSome assignmentBefore "Assignment should exist before removal"

              // Remove game
              manager.RemoveGame("game-1")

              // Give mailbox time to process
              System.Threading.Thread.Sleep(50)

              // Verify assignment is gone
              let assignmentAfter = manager.GetAssignment("game-1")
              Expect.isNone assignmentAfter "Assignment should be None after removal"
          } ]

// =============================================================================
// T011a: Test for assignment persistence across multiple queries
// =============================================================================
[<Tests>]
let playerAssignmentPersistenceTests =
    testList
        "PlayerAssignmentManager Persistence"
        [ test "Assignments are retained across multiple GetRole queries" {
              let manager = PlayerAssignmentManager()

              // Assign both players
              let _ = manager.TryAssignAndValidate("game-1", "user-x", true)
              let _ = manager.TryAssignAndValidate("game-1", "user-o", false)

              // Query multiple times and verify consistency
              for _ in 1..5 do
                  let roleX = manager.GetRole("game-1", "user-x")
                  let roleO = manager.GetRole("game-1", "user-o")
                  let roleSpectator = manager.GetRole("game-1", "user-spectator")

                  Expect.equal roleX PlayerX "User X should remain PlayerX"
                  Expect.equal roleO PlayerO "User O should remain PlayerO"
                  Expect.equal roleSpectator Spectator "Other users should be Spectator"
          } ]

// =============================================================================
// T015: Test first move assigns user as Player X
// =============================================================================
[<Tests>]
let firstMoveAssignmentTests =
    testList
        "First Move Assignment (US1)"
        [ test "First move on new game assigns user as Player X" {
              let manager = PlayerAssignmentManager()
              let (result, _) = manager.TryAssignAndValidate("game-1", "first-user", true)

              match result with
              | Allowed PlayerX -> ()
              | _ -> failtest $"Expected Allowed PlayerX, got {result}"
          }

          test "After first move, user is recognized as PlayerX" {
              let manager = PlayerAssignmentManager()

              // Make first move
              let _ = manager.TryAssignAndValidate("game-1", "user-x", true)

              // Verify role
              let role = manager.GetRole("game-1", "user-x")
              Expect.equal role PlayerX "First mover should be PlayerX"
          } ]

// =============================================================================
// T016: Test getUserRole returns UnassignedX for new game
// =============================================================================
[<Tests>]
let unassignedRoleTests =
    testList
        "Unassigned Role Detection (US1)"
        [ test "GetRole returns UnassignedX for completely new game" {
              let manager = PlayerAssignmentManager()
              let role = manager.GetRole("brand-new-game", "any-user")
              Expect.equal role UnassignedX "New game should show UnassignedX"
          }

          test "GetRole returns UnassignedO after X is assigned" {
              let manager = PlayerAssignmentManager()

              // Assign X
              let _ = manager.TryAssignAndValidate("game-1", "user-x", true)

              // Different user should see UnassignedO
              let role = manager.GetRole("game-1", "user-not-x")
              Expect.equal role UnassignedO "After X assigned, other users should see UnassignedO"
          } ]

// =============================================================================
// T023: Test second move by different user assigns as Player O
// =============================================================================
[<Tests>]
let secondMoveAssignmentTests =
    testList
        "Second Move Assignment (US2)"
        [ test "Second move by different user assigns as Player O" {
              let manager = PlayerAssignmentManager()

              // First user becomes X
              let _ = manager.TryAssignAndValidate("game-1", "user-x", true)

              // Second user (different) becomes O
              let (result, assignment) = manager.TryAssignAndValidate("game-1", "user-o", false)

              match result with
              | Allowed PlayerO -> ()
              | _ -> failtest $"Expected Allowed PlayerO, got {result}"

              Expect.equal assignment.PlayerOId (Some "user-o") "User should be assigned as O"
          }

          test "After second move, user is recognized as PlayerO" {
              let manager = PlayerAssignmentManager()

              // Assign both players
              let _ = manager.TryAssignAndValidate("game-1", "user-x", true)
              let _ = manager.TryAssignAndValidate("game-1", "user-o", false)

              // Verify role
              let role = manager.GetRole("game-1", "user-o")
              Expect.equal role PlayerO "Second mover should be PlayerO"
          } ]

// =============================================================================
// T024: Test same user cannot claim both X and O
// =============================================================================
[<Tests>]
let sameUserCannotClaimBothTests =
    testList
        "Same User Cannot Claim Both (US2)"
        [ test "Same user cannot claim O slot after claiming X" {
              let manager = PlayerAssignmentManager()

              // User becomes X
              let _ = manager.TryAssignAndValidate("game-1", "same-user", true)

              // Same user tries to become O
              let (result, _) = manager.TryAssignAndValidate("game-1", "same-user", false)

              match result with
              | Rejected NotYourTurn -> () // User is X, can't move on O's turn
              | _ -> failtest $"Expected Rejected NotYourTurn, got {result}"
          } ]

// =============================================================================
// T030: Test Player X cannot move on O's turn
// =============================================================================
[<Tests>]
let turnEnforcementXTests =
    testList
        "Turn Enforcement - X on O's Turn (US3)"
        [ test "Player X cannot move when it is O's turn" {
              let manager = PlayerAssignmentManager()

              // Assign both players
              let _ = manager.TryAssignAndValidate("game-1", "user-x", true)
              let _ = manager.TryAssignAndValidate("game-1", "user-o", false)

              // X tries to move on O's turn
              let (result, _) = manager.TryAssignAndValidate("game-1", "user-x", false)

              match result with
              | Rejected NotYourTurn -> ()
              | _ -> failtest $"Expected Rejected NotYourTurn, got {result}"
          } ]

// =============================================================================
// T031: Test Player O cannot move on X's turn
// =============================================================================
[<Tests>]
let turnEnforcementOTests =
    testList
        "Turn Enforcement - O on X's Turn (US3)"
        [ test "Player O cannot move when it is X's turn" {
              let manager = PlayerAssignmentManager()

              // Assign both players
              let _ = manager.TryAssignAndValidate("game-1", "user-x", true)
              let _ = manager.TryAssignAndValidate("game-1", "user-o", false)

              // O tries to move on X's turn
              let (result, _) = manager.TryAssignAndValidate("game-1", "user-o", true)

              match result with
              | Rejected NotYourTurn -> ()
              | _ -> failtest $"Expected Rejected NotYourTurn, got {result}"
          } ]

// =============================================================================
// T037: Test third user rejected as NotAPlayer
// =============================================================================
[<Tests>]
let thirdPartyRejectionTests =
    testList
        "Third Party Rejection (US4)"
        [ test "Third user is rejected as NotAPlayer" {
              let manager = PlayerAssignmentManager()

              // Assign both players
              let _ = manager.TryAssignAndValidate("game-1", "user-x", true)
              let _ = manager.TryAssignAndValidate("game-1", "user-o", false)

              // Third user tries to move
              let (result, _) = manager.TryAssignAndValidate("game-1", "user-third", true)

              match result with
              | Rejected NotAPlayer -> ()
              | _ -> failtest $"Expected Rejected NotAPlayer, got {result}"
          }

          test "Third user rejected on both X and O turns" {
              let manager = PlayerAssignmentManager()

              // Assign both players
              let _ = manager.TryAssignAndValidate("game-1", "user-x", true)
              let _ = manager.TryAssignAndValidate("game-1", "user-o", false)

              // Third user tries on X's turn
              let (resultX, _) = manager.TryAssignAndValidate("game-1", "intruder", true)

              match resultX with
              | Rejected NotAPlayer -> ()
              | _ -> failtest $"Expected NotAPlayer on X's turn, got {resultX}"

              // Third user tries on O's turn
              let (resultO, _) = manager.TryAssignAndValidate("game-1", "intruder", false)

              match resultO with
              | Rejected NotAPlayer -> ()
              | _ -> failtest $"Expected NotAPlayer on O's turn, got {resultO}"
          } ]

// =============================================================================
// T038: Test getUserRole returns Spectator for third user
// =============================================================================
[<Tests>]
let spectatorRoleTests =
    testList
        "Spectator Role Detection (US4)"
        [ test "GetRole returns Spectator for third user when both players assigned" {
              let manager = PlayerAssignmentManager()

              // Assign both players
              let _ = manager.TryAssignAndValidate("game-1", "user-x", true)
              let _ = manager.TryAssignAndValidate("game-1", "user-o", false)

              // Third user should be spectator
              let role = manager.GetRole("game-1", "spectator-user")
              Expect.equal role Spectator "Third user should be Spectator"
          } ]
