// -----------------------------------------------------------------------
// <copyright file="RoundEndEventArgs.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.EventArgs
{
    using SimpleEvents.Core;

    /// <summary>
    /// Fired when a round ends. Maps from EXILED's RoundEndedEventArgs.
    /// </summary>
    public class RoundEndEventArgs : SimpleEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoundEndEventArgs"/> class.
        /// </summary>
        public RoundEndEventArgs()
        {
            EventName = "round.end";
        }

        /// <summary>
        /// Gets or sets the leading team at round end (stored as int to avoid game-specific enum dependency).
        /// </summary>
        public int LeadingTeam { get; set; }
    }
}
