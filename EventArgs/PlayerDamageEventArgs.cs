// -----------------------------------------------------------------------
// <copyright file="PlayerDamageEventArgs.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.EventArgs
{
    using SimpleEvents.Core;

    /// <summary>
    /// Fired when a player is about to take damage. Maps from EXILED's HurtingEventArgs.
    /// </summary>
    public class PlayerDamageEventArgs : SimpleEventArgs
    {
        /// <summary>
        /// Gets or sets the player who is being damaged.
        /// </summary>
        public Exiled.API.Features.Player Target { get; set; }

        /// <summary>
        /// Gets or sets the player who is attacking (null if environmental damage).
        /// </summary>
        public Exiled.API.Features.Player Attacker { get; set; }

        /// <summary>
        /// Gets or sets the amount of damage being dealt.
        /// Modify this value to change how much damage the target receives.
        /// </summary>
        public float Damage { get; set; }

        /// <summary>
        /// Gets or sets the damage handler from EXILED.
        /// </summary>
        public object DamageHandler { get; set; }
    }
}
