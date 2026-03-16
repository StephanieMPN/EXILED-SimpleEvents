// -----------------------------------------------------------------------
// <copyright file="PlayerDyingEventArgs.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.Core.Events
{
    using Exiled.API.Features;

    /// <summary>
    /// Fired when a player is about to die (EXILED: Player.Dying).
    /// EventName: "player.death"
    /// </summary>
    public class PlayerDyingEventArgs : SimpleEventArgs
    {
        /// <summary>Gets the dying player.</summary>
        public Player Player { get; set; }

        /// <summary>Gets the player that caused the death. May be null.</summary>
        public Player Attacker { get; set; }
    }
}
