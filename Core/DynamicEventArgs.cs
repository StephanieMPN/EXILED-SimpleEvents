// -----------------------------------------------------------------------
// <copyright file="DynamicEventArgs.cs" company="SimpleEvents">
// SimpleEvents Framework for EXILED.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleEvents.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A flexible event args type for events that don't have a dedicated typed class.
    /// Stores data as a key/value bag, enabling dynamic property access.
    /// </summary>
    public class DynamicEventArgs : SimpleEventArgs
    {
        private readonly Dictionary<string, object> data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets a data value by key. Returns null if the key is not found.
        /// </summary>
        /// <param name="key">The data key (case-insensitive).</param>
        public object this[string key]
        {
            get => data.TryGetValue(key, out object val) ? val : null;
            set => data[key] = value;
        }

        /// <summary>
        /// Returns whether a key exists in the data bag.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key exists.</returns>
        public bool Has(string key) => data.ContainsKey(key);

        /// <summary>
        /// Gets a typed value from the data bag. Returns default if not found or type mismatch.
        /// </summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="key">The data key.</param>
        /// <returns>The typed value or default.</returns>
        public T Get<T>(string key)
        {
            if (data.TryGetValue(key, out object val) && val is T typed)
                return typed;
            return default;
        }

        /// <summary>
        /// Sets a typed value in the data bag.
        /// </summary>
        /// <param name="key">The data key.</param>
        /// <param name="value">The value to store.</param>
        /// <returns>This instance for chaining.</returns>
        public DynamicEventArgs Set(string key, object value)
        {
            data[key] = value;
            return this;
        }

        /// <summary>
        /// Returns all keys in the data bag.
        /// </summary>
        public IEnumerable<string> Keys => data.Keys;

        /// <summary>
        /// Gets the number of entries in the data bag.
        /// </summary>
        public int Count => data.Count;
    }
}
