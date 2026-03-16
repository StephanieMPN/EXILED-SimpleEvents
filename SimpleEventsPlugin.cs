// -----------------------------------------------------------------------
// <copyright file="SimpleEventsPlugin.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Interfaces;

    using SimpleEvents.Hooking;

    /// <summary>
    /// EXILED plugin entry point for the SimpleEvents framework.
    /// Initialises the event bus and all EXILED bridges when the plugin is enabled.
    /// </summary>
    public class SimpleEventsPlugin : Plugin<SimpleEventsConfig>
    {
        private static readonly SimpleEventsPlugin Singleton = new SimpleEventsPlugin();

        private SimpleEventsPlugin()
        {
        }

        /// <summary>Gets the singleton instance.</summary>
        public static SimpleEventsPlugin Instance => Singleton;

        /// <inheritdoc/>
        public override string Name => "SimpleEvents";

        /// <inheritdoc/>
        public override string Author => "SimpleEvents Team";

        /// <inheritdoc/>
        public override string Prefix => "simple_events";

        /// <inheritdoc/>
        public override Version Version => new Version(1, 0, 0);

        /// <inheritdoc/>
        public override Version RequiredExiledVersion => new Version(8, 0, 0);

        /// <summary>
        /// Highest priority so the bridge is active before any other plugin subscribes to EXILED events.
        /// </summary>
        public override PluginPriority Priority => PluginPriority.Highest;

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            HarmonyBootstrap.Initialize();
            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            HarmonyBootstrap.Shutdown();
            base.OnDisabled();
        }
    }
}
