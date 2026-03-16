// -----------------------------------------------------------------------
// <copyright file="EventArgsMapper.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.Bridge
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Server;

    using SimpleEvents.Core;
    using SimpleEvents.Core.Events;

    /// <summary>
    /// Converts EXILED EventArgs into SimpleEventArgs.
    /// Known types are mapped via pre-registered factory functions (no reflection, full performance).
    /// Unknown types fall back to a <see cref="DynamicEventArgs"/> built with cached property reflection.
    /// </summary>
    public static class EventArgsMapper
    {
        // Factory functions for known EXILED EventArgs types.
        private static readonly Dictionary<Type, Func<object, SimpleEventArgs>> Factories =
            new Dictionary<Type, Func<object, SimpleEventArgs>>();

        // Cache of PropertyInfo arrays per type for the dynamic fallback path.
        private static readonly Dictionary<Type, PropertyInfo[]> PropertyCache =
            new Dictionary<Type, PropertyInfo[]>();

        private static readonly object CacheLock = new object();

        static EventArgsMapper()
        {
            RegisterKnownTypes();
        }

        /// <summary>
        /// Maps any EXILED EventArgs object to a <see cref="SimpleEventArgs"/>.
        /// Returns a typed subclass for known types, or a <see cref="DynamicEventArgs"/> for unknown ones.
        /// </summary>
        public static SimpleEventArgs Map(object exiledArgs)
        {
            if (exiledArgs == null)
                return null;

            Type type = exiledArgs.GetType();

            if (Factories.TryGetValue(type, out Func<object, SimpleEventArgs> factory))
                return factory(exiledArgs);

            return BuildDynamic(exiledArgs, type);
        }

        /// <summary>
        /// Registers a custom factory for a specific EXILED EventArgs type.
        /// This can be called by external code to extend the mapper.
        /// </summary>
        public static void Register<TExiled>(string eventName, Func<TExiled, SimpleEventArgs> factory)
        {
            Factories[typeof(TExiled)] = raw => factory((TExiled)raw);
        }

        private static void RegisterKnownTypes()
        {
            // player.damage
            Factories[typeof(HurtingEventArgs)] = raw =>
            {
                var e = (HurtingEventArgs)raw;
                return new PlayerDamageEventArgs
                {
                    EventName = "player.damage",
                    IsAllowed = e.IsAllowed,
                    Player = e.Player,
                    Attacker = e.Attacker,
                    Damage = e.Amount,
                };
            };

            // player.death
            Factories[typeof(DyingEventArgs)] = raw =>
            {
                var e = (DyingEventArgs)raw;
                return new PlayerDyingEventArgs
                {
                    EventName = "player.death",
                    IsAllowed = e.IsAllowed,
                    Player = e.Player,
                    Attacker = e.Attacker,
                };
            };

            // player.spawn
            Factories[typeof(SpawningEventArgs)] = raw =>
            {
                var e = (SpawningEventArgs)raw;
                return new PlayerSpawningEventArgs
                {
                    EventName = "player.spawn",
                    IsAllowed = true,
                    Player = e.Player,
                    Position = e.Position,
                    HorizontalRotation = e.HorizontalRotation,
                    OldRole = e.OldRole,
                };
            };

            // round.end
            Factories[typeof(RoundEndedEventArgs)] = raw =>
            {
                var e = (RoundEndedEventArgs)raw;
                return new Core.Events.RoundEndedEventArgs
                {
                    EventName = "round.end",
                    IsAllowed = true,
                    LeadingTeam = e.LeadingTeam,
                    TimeToRestart = e.TimeToRestart,
                };
            };

            // item.pickup
            Factories[typeof(PickingUpItemEventArgs)] = raw =>
            {
                var e = (PickingUpItemEventArgs)raw;
                return new ItemPickingUpEventArgs
                {
                    EventName = "item.pickup",
                    IsAllowed = e.IsAllowed,
                    Player = e.Player,
                    Pickup = e.Pickup,
                };
            };
        }

        private static DynamicEventArgs BuildDynamic(object exiledArgs, Type type)
        {
            PropertyInfo[] props;

            // Lock around cache access to prevent race conditions when multiple
            // game events fire concurrently for an unknown EventArgs type.
            lock (CacheLock)
            {
                if (!PropertyCache.TryGetValue(type, out props))
                {
                    props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    PropertyCache[type] = props;
                }
            }

            var args = new DynamicEventArgs
            {
                EventName = "dynamic." + type.Name.Replace("EventArgs", string.Empty).ToLowerInvariant(),
                IsAllowed = true,
                OriginalArgs = exiledArgs,
            };

            foreach (PropertyInfo prop in props)
            {
                // Always check CanRead first — before any GetValue call.
                if (!prop.CanRead)
                    continue;

                try
                {
                    if (prop.Name == "IsAllowed" && prop.PropertyType == typeof(bool))
                    {
                        args.IsAllowed = (bool)prop.GetValue(exiledArgs);
                        continue;
                    }

                    args[prop.Name] = prop.GetValue(exiledArgs);
                }
                catch
                {
                    // Ignore properties that throw on access (indexers, etc.).
                }
            }

            return args;
        }
    }
}
