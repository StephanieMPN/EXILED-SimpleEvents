// -----------------------------------------------------------------------
// <copyright file="RoundStartEventArgs.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.EventArgs
{
    using SimpleEvents.Core;

    /// <summary>
    /// Fired when a round starts. This is a parameterless event in EXILED,
    /// but wrapped here for consistency with the typed subscription model.
    /// </summary>
    public class RoundStartEventArgs : SimpleEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoundStartEventArgs"/> class.
        /// </summary>
        public RoundStartEventArgs()
        {
            EventName = "round.start";
        }
    }
}
