// -----------------------------------------------------------------------
// <copyright file="RoundEndedEventArgs.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.Core.Events
{
    using Exiled.API.Enums;

    /// <summary>
    /// Fired after a round ends (EXILED: Server.RoundEnded).
    /// EventName: "round.end"
    /// </summary>
    public class RoundEndedEventArgs : SimpleEventArgs
    {
        /// <summary>Gets or sets the team that won the round.</summary>
        public LeadingTeam LeadingTeam { get; set; }

        /// <summary>Gets or sets the time in seconds until the next round restarts.</summary>
        public int TimeToRestart { get; set; }

        /// <summary>
        /// Gets the winning team as a human-readable string.
        /// Convenience wrapper around <see cref="LeadingTeam"/>.
        /// </summary>
        public string WinningTeamName => LeadingTeam.ToString();
    }
}
