// -----------------------------------------------------------------------
// <copyright file="PlayerDamageEventArgs.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.Core.Events
{
    using Exiled.API.Features;

    /// <summary>
    /// Fired when a player is about to receive damage (EXILED: Player.Hurting).
    /// EventName: "player.damage"
    /// </summary>
    public class PlayerDamageEventArgs : SimpleEventArgs
    {
        /// <summary>Gets the player receiving damage.</summary>
        public Player Player { get; set; }

        /// <summary>Gets the player dealing damage. May be null.</summary>
        public Player Attacker { get; set; }

        /// <summary>Gets or sets the damage amount.</summary>
        public float Damage { get; set; }
    }
}
