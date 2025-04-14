namespace TicTacToe.Engine
{
    /// <summary>
    /// Represents the state of a square on the game board.
    /// </summary>
    public abstract record Square
    {
        private Square() { }

        /// <summary>
        /// Represents a square that has been claimed by a player.
        /// </summary>
        /// <param name="Marker">The marker placed in this square.</param>
        public sealed record Taken(Marker Marker) : Square;

        /// <summary>
        /// Represents an available square that can be claimed.
        /// </summary>
        /// <param name="NextMarker">The marker that would be placed if this square is chosen.</param>
        public sealed record Available(Marker NextMarker) : Square;

        /// <summary>
        /// Represents a square that is no longer available because the game has ended.
        /// </summary>
        public sealed record Unavailable : Square;
    }
}
