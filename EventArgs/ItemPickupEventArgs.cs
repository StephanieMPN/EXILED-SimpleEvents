// -----------------------------------------------------------------------
// <copyright file="ItemPickupEventArgs.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.EventArgs
{
    using SimpleEvents.Core;

    /// <summary>
    /// Fired when a player is about to pick up an item.
    /// Maps from EXILED's PickingUpItemEventArgs.
    /// </summary>
    public class ItemPickupEventArgs : SimpleEventArgs
    {
        /// <summary>
        /// Gets or sets the player picking up the item.
        /// </summary>
        public Exiled.API.Features.Player Player { get; set; }

        /// <summary>
        /// Gets or sets the pickup being picked up (as object to avoid hard coupling).
        /// </summary>
        public object Pickup { get; set; }
    }
}
