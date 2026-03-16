// -----------------------------------------------------------------------
// <copyright file="SimpleEventsConfig.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents
{
    using Exiled.API.Interfaces;

    /// <summary>
    /// Configuration for the SimpleEvents plugin.
    /// </summary>
    public class SimpleEventsConfig : IConfig
    {
        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <inheritdoc/>
        public bool Debug { get; set; } = false;
    }
}
