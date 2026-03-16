// -----------------------------------------------------------------------
// <copyright file="EventHooks.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.Hooking
{
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Server;

    using SimpleEvents.Bridge;

    /// <summary>
    /// Subscribes to EXILED events and forwards them to <see cref="ExiledEventBridge"/>.
    /// This keeps the bridge clean of EXILED API references and makes hooking/unhooking explicit.
    /// </summary>
    internal static class EventHooks
    {
        private static readonly object SubscribeLock = new object();
        private static bool subscribed;

        /// <summary>
        /// Subscribe to all tracked EXILED events.
        /// Thread-safe. Subsequent calls while already subscribed are no-ops.
        /// </summary>
        internal static void Subscribe()
        {
            lock (SubscribeLock)
            {
                if (subscribed)
                    return;

                subscribed = true;
            }

            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;

            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            Exiled.Events.Handlers.Player.Dying += OnDying;
            Exiled.Events.Handlers.Player.Spawning += OnSpawning;
            Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUpItem;
        }

        /// <summary>
        /// Unsubscribe from all tracked EXILED events.
        /// Thread-safe.
        /// </summary>
        internal static void Unsubscribe()
        {
            lock (SubscribeLock)
            {
                if (!subscribed)
                    return;

                subscribed = false;
            }

            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;

            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            Exiled.Events.Handlers.Player.Dying -= OnDying;
            Exiled.Events.Handlers.Player.Spawning -= OnSpawning;
            Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUpItem;
        }

        private static void OnWaitingForPlayers() =>
            ExiledEventBridge.DispatchWaitingForPlayers();

        private static void OnRoundStarted() =>
            ExiledEventBridge.DispatchRoundStarted();

        private static void OnRoundEnded(RoundEndedEventArgs ev) =>
            ExiledEventBridge.DispatchRoundEnded(ev);

        private static void OnHurting(HurtingEventArgs ev) =>
            ExiledEventBridge.DispatchHurting(ev);

        private static void OnDying(DyingEventArgs ev) =>
            ExiledEventBridge.DispatchDying(ev);

        private static void OnSpawning(SpawningEventArgs ev) =>
            ExiledEventBridge.DispatchSpawning(ev);

        private static void OnPickingUpItem(PickingUpItemEventArgs ev) =>
            ExiledEventBridge.DispatchPickingUpItem(ev);
    }
}
