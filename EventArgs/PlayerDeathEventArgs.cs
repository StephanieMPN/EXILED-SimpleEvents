// -----------------------------------------------------------------------
// <copyright file="PlayerDeathEventArgs.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.EventArgs
{
    using System.Collections.Generic;

    using SimpleEvents.Core;

    /// <summary>
    /// Fired when a player is about to die. Maps from EXILED's DyingEventArgs.
    /// </summary>
    public class PlayerDeathEventArgs : SimpleEventArgs
    {
        /// <summary>
        /// Gets or sets the player who is dying.
        /// </summary>
        public Exiled.API.Features.Player Target { get; set; }

        /// <summary>
        /// Gets or sets the player who killed the target (null if environmental).
        /// </summary>
        public Exiled.API.Features.Player Attacker { get; set; }

        /// <summary>
        /// Gets or sets the damage handler from EXILED.
        /// </summary>
        public object DamageHandler { get; set; }
    }
}
