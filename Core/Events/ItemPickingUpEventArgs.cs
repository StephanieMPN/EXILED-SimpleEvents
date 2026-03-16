// -----------------------------------------------------------------------
// <copyright file="ItemPickingUpEventArgs.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.Core.Events
{
    using Exiled.API.Features;
    using Exiled.API.Features.Pickups;

    /// <summary>
    /// Fired when a player is about to pick up an item (EXILED: Player.PickingUpItem).
    /// EventName: "item.pickup"
    /// </summary>
    public class ItemPickingUpEventArgs : SimpleEventArgs
    {
        /// <summary>Gets the player picking up the item.</summary>
        public Player Player { get; set; }

        /// <summary>Gets the pickup being collected.</summary>
        public Pickup Pickup { get; set; }
    }
}
