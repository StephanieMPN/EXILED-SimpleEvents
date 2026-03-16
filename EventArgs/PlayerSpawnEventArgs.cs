// -----------------------------------------------------------------------
// <copyright file="PlayerSpawnEventArgs.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.EventArgs
{
    using SimpleEvents.Core;

    /// <summary>
    /// Fired when a player is about to spawn. Maps from EXILED's SpawningEventArgs.
    /// </summary>
    public class PlayerSpawnEventArgs : SimpleEventArgs
    {
        /// <summary>
        /// Gets or sets the player who is spawning.
        /// </summary>
        public Exiled.API.Features.Player Player { get; set; }

        /// <summary>
        /// Gets or sets the spawn position as a float array [x, y, z].
        /// Stored as floats to avoid hard dependency on UnityEngine.Vector3 in consuming code.
        /// </summary>
        public float[] Position { get; set; }

        /// <summary>
        /// Gets or sets the horizontal rotation at spawn.
        /// </summary>
        public float HorizontalRotation { get; set; }
    }
}
