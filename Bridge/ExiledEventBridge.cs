// -----------------------------------------------------------------------
// <copyright file="ExiledEventBridge.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.Bridge
{
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Server;

    using SimpleEvents.Core;
    using SimpleEvents.Core.Events;

    /// <summary>
    /// Receives EXILED EventArgs from event hooks and dispatches them through the SimpleEventBus.
    /// After the bus runs, writes IsAllowed back to the original EXILED EventArgs so cancellable
    /// events are honoured correctly.
    /// </summary>
    public static class ExiledEventBridge
    {
        /// <summary>Dispatches a waiting-for-players notification.</summary>
        public static void DispatchWaitingForPlayers()
        {
            SimpleEventBus.Emit(new DynamicEventArgs { EventName = "server.waiting" });
        }

        /// <summary>Dispatches a round-started notification (no args, no cancellation).</summary>
        public static void DispatchRoundStarted()
        {
            SimpleEventBus.Emit(new RoundStartedEventArgs { EventName = "round.start" });
        }

        /// <summary>Dispatches a round-ended event.</summary>
        public static void DispatchRoundEnded(RoundEndedEventArgs exiledEv)
        {
            SimpleEventArgs mapped = EventArgsMapper.Map(exiledEv);
            mapped.OriginalArgs = exiledEv;
            SimpleEventBus.Emit(mapped);
            // RoundEndedEventArgs has no IsAllowed — nothing to write back.
        }

        /// <summary>Dispatches a player-hurting event and writes back IsAllowed.</summary>
        public static void DispatchHurting(HurtingEventArgs exiledEv)
        {
            SimpleEventArgs mapped = EventArgsMapper.Map(exiledEv);
            mapped.OriginalArgs = exiledEv;
            SimpleEventBus.Emit(mapped);
            exiledEv.IsAllowed = mapped.IsAllowed;
        }

        /// <summary>Dispatches a player-dying event and writes back IsAllowed.</summary>
        public static void DispatchDying(DyingEventArgs exiledEv)
        {
            SimpleEventArgs mapped = EventArgsMapper.Map(exiledEv);
            mapped.OriginalArgs = exiledEv;
            SimpleEventBus.Emit(mapped);
            exiledEv.IsAllowed = mapped.IsAllowed;
        }

        /// <summary>Dispatches a player-spawning event (not cancellable in EXILED).</summary>
        public static void DispatchSpawning(SpawningEventArgs exiledEv)
        {
            SimpleEventArgs mapped = EventArgsMapper.Map(exiledEv);
            mapped.OriginalArgs = exiledEv;
            SimpleEventBus.Emit(mapped);

            // Write back mutable properties that bus handlers may have changed.
            if (mapped is PlayerSpawningEventArgs simple)
            {
                exiledEv.Position = simple.Position;
                exiledEv.HorizontalRotation = simple.HorizontalRotation;
            }
        }

        /// <summary>Dispatches a pick-up-item event and writes back IsAllowed.</summary>
        public static void DispatchPickingUpItem(PickingUpItemEventArgs exiledEv)
        {
            SimpleEventArgs mapped = EventArgsMapper.Map(exiledEv);
            mapped.OriginalArgs = exiledEv;
            SimpleEventBus.Emit(mapped);
            exiledEv.IsAllowed = mapped.IsAllowed;
        }

        /// <summary>
        /// Generic fallback — maps any EXILED EventArgs via reflection and emits a DynamicEventArgs.
        /// Use this for events not explicitly handled above.
        /// </summary>
        public static void Dispatch(object exiledArgs)
        {
            SimpleEventArgs mapped = EventArgsMapper.Map(exiledArgs);
            if (mapped != null)
                SimpleEventBus.Emit(mapped);
        }
    }
}
