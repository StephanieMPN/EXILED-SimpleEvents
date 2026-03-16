// -----------------------------------------------------------------------
// <copyright file="SimpleEventBus.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.Core
{
    using System;
    using System.Collections.Generic;

    using Exiled.API.Features;

    /// <summary>
    /// Central event bus for the SimpleEvents framework.
    /// Supports subscription by concrete type and by event name string.
    /// Thread-safe — handlers may be subscribed/unsubscribed from any thread.
    /// </summary>
    public static class SimpleEventBus
    {
        // Handlers keyed by the exact concrete SimpleEventArgs type.
        private static readonly Dictionary<Type, List<Action<SimpleEventArgs>>> TypeHandlers =
            new Dictionary<Type, List<Action<SimpleEventArgs>>>();

        // Handlers keyed by event name string (case-insensitive).
        private static readonly Dictionary<string, List<Action<SimpleEventArgs>>> NameHandlers =
            new Dictionary<string, List<Action<SimpleEventArgs>>>(StringComparer.OrdinalIgnoreCase);

        // Maps original typed delegates -> wrapped Action<SimpleEventArgs> for unsubscription support.
        private static readonly Dictionary<Delegate, Action<SimpleEventArgs>> WrapperCache =
            new Dictionary<Delegate, Action<SimpleEventArgs>>();

        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Subscribes a typed handler. The handler is invoked whenever an event of type
        /// <typeparamref name="T"/> (or a derived type) is emitted.
        /// </summary>
        /// <typeparam name="T">The SimpleEventArgs subclass to listen for.</typeparam>
        /// <param name="handler">The handler to invoke.</param>
        public static void Subscribe<T>(Action<T> handler) where T : SimpleEventArgs
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Action<SimpleEventArgs> wrapper = e => handler((T)e);

            lock (SyncRoot)
            {
                WrapperCache[handler] = wrapper;

                Type type = typeof(T);
                if (!TypeHandlers.TryGetValue(type, out List<Action<SimpleEventArgs>> list))
                {
                    list = new List<Action<SimpleEventArgs>>();
                    TypeHandlers[type] = list;
                }

                list.Add(wrapper);
            }
        }

        /// <summary>
        /// Unsubscribes a previously subscribed typed handler.
        /// </summary>
        /// <typeparam name="T">The event type the handler was subscribed to.</typeparam>
        /// <param name="handler">The original handler that was passed to <see cref="Subscribe{T}"/>.</param>
        public static void Unsubscribe<T>(Action<T> handler) where T : SimpleEventArgs
        {
            if (handler == null)
                return;

            lock (SyncRoot)
            {
                if (!WrapperCache.TryGetValue(handler, out Action<SimpleEventArgs> wrapper))
                    return;

                WrapperCache.Remove(handler);

                Type type = typeof(T);
                if (TypeHandlers.TryGetValue(type, out List<Action<SimpleEventArgs>> list))
                    list.Remove(wrapper);
            }
        }

        /// <summary>
        /// Subscribes a handler by event name (e.g. "player.damage", "round.start").
        /// Name matching is case-insensitive.
        /// </summary>
        /// <param name="eventName">The event name to listen for.</param>
        /// <param name="handler">The handler to invoke.</param>
        public static void Subscribe(string eventName, Action<SimpleEventArgs> handler)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (SyncRoot)
            {
                if (!NameHandlers.TryGetValue(eventName, out List<Action<SimpleEventArgs>> list))
                {
                    list = new List<Action<SimpleEventArgs>>();
                    NameHandlers[eventName] = list;
                }

                list.Add(handler);
            }
        }

        /// <summary>
        /// Unsubscribes a named handler.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="handler">The handler to remove.</param>
        public static void Unsubscribe(string eventName, Action<SimpleEventArgs> handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null)
                return;

            lock (SyncRoot)
            {
                if (NameHandlers.TryGetValue(eventName, out List<Action<SimpleEventArgs>> list))
                    list.Remove(handler);
            }
        }

        /// <summary>
        /// Emits an event. Dispatches to type handlers (walking the type hierarchy)
        /// and to name handlers.
        /// </summary>
        /// <param name="args">The event arguments to emit.</param>
        public static void Emit(SimpleEventArgs args)
        {
            if (args == null)
                return;

            Action<SimpleEventArgs>[] typedSnapshot = null;
            Action<SimpleEventArgs>[] namedSnapshot = null;

            lock (SyncRoot)
            {
                // Walk the type hierarchy so that subscribing to a base class
                // also catches derived event types.
                List<Action<SimpleEventArgs>> combined = null;
                Type type = args.GetType();
                while (type != null && type != typeof(object))
                {
                    if (TypeHandlers.TryGetValue(type, out List<Action<SimpleEventArgs>> list) && list.Count > 0)
                    {
                        if (combined == null)
                            combined = new List<Action<SimpleEventArgs>>(list);
                        else
                            combined.AddRange(list);
                    }

                    type = type.BaseType;
                }

                if (combined != null)
                    typedSnapshot = combined.ToArray();

                // Name-based handlers
                if (!string.IsNullOrEmpty(args.EventName) &&
                    NameHandlers.TryGetValue(args.EventName, out List<Action<SimpleEventArgs>> nameList) &&
                    nameList.Count > 0)
                {
                    namedSnapshot = nameList.ToArray();
                }
            }

            // Invoke outside of lock for safety and performance.
            if (typedSnapshot != null)
            {
                foreach (Action<SimpleEventArgs> handler in typedSnapshot)
                    InvokeSafely(handler, args);
            }

            if (namedSnapshot != null)
            {
                foreach (Action<SimpleEventArgs> handler in namedSnapshot)
                    InvokeSafely(handler, args);
            }
        }

        /// <summary>
        /// Emits a typed event. Convenience overload.
        /// </summary>
        /// <typeparam name="T">The event args type.</typeparam>
        /// <param name="args">The event arguments.</param>
        public static void Emit<T>(T args) where T : SimpleEventArgs => Emit((SimpleEventArgs)args);

        /// <summary>
        /// Emits an event by name using a <see cref="DynamicEventArgs"/> with the given name.
        /// Useful for fire-and-forget notifications.
        /// </summary>
        /// <param name="eventName">The event name to emit.</param>
        public static void Emit(string eventName)
        {
            Emit(new DynamicEventArgs { EventName = eventName });
        }

        /// <summary>
        /// Removes all registered handlers. Called on plugin disable.
        /// </summary>
        internal static void Clear()
        {
            lock (SyncRoot)
            {
                TypeHandlers.Clear();
                NameHandlers.Clear();
                WrapperCache.Clear();
            }
        }

        private static void InvokeSafely(Action<SimpleEventArgs> handler, SimpleEventArgs args)
        {
            try
            {
                handler(args);
            }
            catch (Exception ex)
            {
                Log.Error(
                    $"[SimpleEvents] Handler \"{handler.Method.Name}\" " +
                    $"from \"{handler.Method.DeclaringType?.FullName}\" " +
                    $"threw an exception for event \"{args.EventName ?? args.GetType().Name}\":\n{ex}");
            }
        }
    }
}
