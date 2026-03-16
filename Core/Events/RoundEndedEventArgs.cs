// -----------------------------------------------------------------------
// <copyright file="RoundEndedEventArgs.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.Core.Events
{
    /// <summary>
    /// Fired after a round ends (EXILED: Server.RoundEnded).
    /// EventName: "round.end"
    /// </summary>
    public class RoundEndedEventArgs : SimpleEventArgs
    {
        /// <summary>Gets the winning team name.</summary>
        public string WinningTeam { get; set; }

        /// <summary>Gets the lead team value.</summary>
        public int LeadingTeam { get; set; }
    }
}
