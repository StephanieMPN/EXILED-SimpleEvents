// -----------------------------------------------------------------------
// <copyright file="HarmonyBootstrap.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.Hooking
{
    using Exiled.API.Features;

    using HarmonyLib;

    using SimpleEvents.Core;

    /// <summary>
    /// Bootstraps the SimpleEvents system.
    /// Initialises the <see cref="SimpleEventBus"/>, creates a Harmony instance for any future
    /// manual patches, registers the bridge, and subscribes to EXILED events via <see cref="EventHooks"/>.
    /// </summary>
    public static class HarmonyBootstrap
    {
        private static Harmony harmonyInstance;

        /// <summary>
        /// Gets the Harmony instance created during initialisation.
        /// Available for external code that wants to apply additional patches.
        /// </summary>
        public static Harmony Harmony => harmonyInstance;

        /// <summary>
        /// Initialises the entire SimpleEvents pipeline:
        /// 1. Clears the event bus (safe restart).
        /// 2. Creates the Harmony instance.
        /// 3. Subscribes to EXILED events through EventHooks.
        /// </summary>
        public static void Initialize()
        {
            // 1 — Reset the bus so previous subscriptions don't leak on hot-reload.
            SimpleEventBus.Clear();

            // 2 — Create Harmony instance (id must be unique per plugin).
            harmonyInstance = new Harmony("com.simpleevents.framework");

            // 3 — Wire up EXILED event subscriptions.
            EventHooks.Subscribe();

            Log.Info("[SimpleEvents] Framework initialised. EventBus ready, hooks active.");
        }

        /// <summary>
        /// Tears down the SimpleEvents pipeline cleanly.
        /// </summary>
        public static void Shutdown()
        {
            EventHooks.Unsubscribe();
            SimpleEventBus.Clear();

            harmonyInstance?.UnpatchSelf();
            harmonyInstance = null;

            Log.Info("[SimpleEvents] Framework shut down.");
        }
    }
}
