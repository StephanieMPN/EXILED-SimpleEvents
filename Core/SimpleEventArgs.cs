// -----------------------------------------------------------------------
// <copyright file="SimpleEventArgs.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.Core
{
    using System;

    /// <summary>
    /// Base class for all SimpleEvents events.
    /// Provides common properties shared across all event types.
    /// </summary>
    public abstract class SimpleEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the action is allowed to proceed.
        /// Setting this to false will cancel the event (if the original EXILED event supports cancellation).
        /// </summary>
        public bool IsAllowed { get; set; } = true;

        /// <summary>
        /// Gets or sets the canonical name of the event (e.g. "player.damage", "round.start").
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Gets the UTC timestamp of when the event was created.
        /// </summary>
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the original EXILED EventArgs that this event was mapped from.
        /// Null if the event was created directly (not bridged from EXILED).
        /// </summary>
        public object OriginalArgs { get; set; }
    }
}
