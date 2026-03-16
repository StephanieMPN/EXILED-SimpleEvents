// -----------------------------------------------------------------------
// <copyright file="PlayerSpawningEventArgs.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.Core.Events
{
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;

    using UnityEngine;

    /// <summary>
    /// Fired when a player is spawning (EXILED: Player.Spawning).
    /// EventName: "player.spawn"
    /// </summary>
    public class PlayerSpawningEventArgs : SimpleEventArgs
    {
        /// <summary>Gets the player that is spawning.</summary>
        public Player Player { get; set; }

        /// <summary>Gets or sets the spawn position.</summary>
        public Vector3 Position { get; set; }

        /// <summary>Gets or sets the horizontal spawn rotation.</summary>
        public float HorizontalRotation { get; set; }

        /// <summary>Gets the player's old role before spawning.</summary>
        public Role OldRole { get; set; }
    }
}
