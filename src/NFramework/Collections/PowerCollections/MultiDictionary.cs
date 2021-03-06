﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NSoft.NFramework.Collections.PowerCollections {
    /// <summary>
    /// <para>The MultiDictionary class that associates values with a key. Unlike an Dictionary,
    /// each key can have multiple values associated with it. When indexing an MultiDictionary, instead
    /// of a single value associated with a key, you retrieve an enumeration of values.</para>
    /// <para>When constructed, you can chose to allow the same value to be associated with a key multiple
    /// times, or only one time. </para>
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The of values associated with the keys.</typeparam>
    ///<seealso cref="Dictionary{TKey,TValue}"/>
    ///<seealso cref="OrderedMultiDictionary{TKey,TValue}"/>
    [Serializable]
#if !SILVERLIGHT
    [System.Reflection.Obfuscation(Feature = "all", Exclude = true, ApplyToMembers = true)]
#endif
    public class MultiDictionary<TKey, TValue> : MultiDictionaryBase<TKey, TValue> {
        #region << logger >>

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private static readonly bool IsDebugEnabled = log.IsDebugEnabled;

        #endregion

        // The comparer for comparing keys
        private readonly IEqualityComparer<TKey> _keyEqualityComparer;

        // The comparer for comparing values;
        private readonly IEqualityComparer<TValue> _valueEqualityComparer;

        // The comparer for compaing keys and values.
        private readonly IEqualityComparer<KeyAndValues> _equalityComparer;

        // The hash that holds the keys and values.
        private Hash<KeyAndValues> _hash;

        // Whether duplicate values for the same key are allowed.
        private readonly bool _allowDuplicateValues;

        /// <summary>
        /// A structure to hold the key and the values associated with the key.
        /// The number of values must always be 1 or greater in a version that is stored, but 
        /// can be zero in a dummy version used only for lookups.
        /// </summary>
        [Serializable]
#if !SILVERLIGHT
        [System.Reflection.Obfuscation(Feature = "all", Exclude = true, ApplyToMembers = true)]
#endif
        private struct KeyAndValues {
            /// <summary>
            /// The key.
            /// </summary>
            public TKey Key;

            /// <summary>
            /// The number of values. Always at least 1 except in a dummy version for lookups.
            /// </summary>
            public int Count;

            /// <summary>
            /// An array of values. 
            /// </summary>
            public TValue[] Values;

            /// <summary>
            /// Create a dummy KeyAndValues with just the key, for lookups.
            /// </summary>
            /// <param name="key">The key to use.</param>
            public KeyAndValues(TKey key) {
                Key = key;
                Count = 0;
                Values = null;
            }

            /// <summary>
            /// Make a copy of a KeyAndValues, copying the array.
            /// </summary>
            /// <param name="x">KeyAndValues to copy.</param>
            /// <returns>A copied version.</returns>
            public static KeyAndValues Copy(KeyAndValues x) {
                KeyAndValues result;

                result.Key = x.Key;
                result.Count = x.Count;

                result.Values = (x.Values != null) ? (TValue[])x.Values.Clone() : null;

                return result;
            }
        }

        /// <summary>
        /// This class implements IEqualityComparer for KeysAndValues, allowing them to be
        /// compared by their keys. An IEqualityComparer on keys is required.
        /// </summary>
        [Serializable]
#if !SILVERLIGHT
        [System.Reflection.Obfuscation(Feature = "all", Exclude = true, ApplyToMembers = true)]
#endif
        private class KeyAndValuesEqualityComparer : IEqualityComparer<KeyAndValues> {
            private readonly IEqualityComparer<TKey> _keyEqualityComparer;

            public KeyAndValuesEqualityComparer(IEqualityComparer<TKey> keyEqualityComparer) {
                keyEqualityComparer.ShouldNotBeNull("keyEqualityComparer");
                _keyEqualityComparer = keyEqualityComparer;
            }

            public bool Equals(KeyAndValues x, KeyAndValues y) {
                return _keyEqualityComparer.Equals(x.Key, y.Key);
            }

            public int GetHashCode(KeyAndValues obj) {
                return Util.GetHashCode(obj.Key, _keyEqualityComparer);
            }

            public override bool Equals(object obj) {
                return (obj != null) && (obj is KeyAndValuesEqualityComparer) && Equals((KeyAndValuesEqualityComparer)obj);
            }

            public bool Equals(KeyAndValuesEqualityComparer other) {
                if(ReferenceEquals(null, other))
                    return false;
                if(ReferenceEquals(this, other))
                    return true;
                return Equals(other._keyEqualityComparer, _keyEqualityComparer);
            }

            public override int GetHashCode() {
                return (_keyEqualityComparer != null ? _keyEqualityComparer.GetHashCode() : 0);
            }
        }

        #region Constructors

        /// <summary>
        /// Create a new MultiDictionary. The default ordering of keys and values are used. If duplicate values
        /// are allowed, multiple copies of the same value can be associated with the same key. For example, the key "foo"
        /// could have "a", "a", and "b" associated with it. If duplicate values are not allowed, only one copies of a given value can
        /// be associated with the same key, although different keys can have the same value. For example, the key "foo" could
        /// have "a" and "b" associated with it, which key "bar" has values "b" and "c" associated with it.
        /// </summary>
        /// <remarks>The default ordering of keys and values will be used, as defined by TKey and TValue's implementation
        /// of IComparable&lt;T&gt; (or IComparable if IComparable&lt;T&gt; is not implemented). If a different ordering should be
        /// used, other constructors allow a custom Comparer or IComparer to be passed to changed the ordering.</remarks>
        /// <param name="allowDuplicateValues">Can the same value be associated with a key multiple times?</param>
        /// <exception cref="InvalidOperationException">TKey or TValue does not implement either IComparable&lt;T&gt; or IComparable.</exception>
        public MultiDictionary(bool allowDuplicateValues)
            : this(allowDuplicateValues, EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default) {}

        /// <summary>
        /// Create a new MultiDictionary. If duplicate values
        /// are allowed, multiple copies of the same value can be associated with the same key. For example, the key "foo"
        /// could have "a", "a", and "b" associated with it. If duplicate values are not allowed, only one copies of a given value can
        /// be associated with the same key, although different keys can have the same value. For example, the key "foo" could
        /// have "a" and "b" associated with it, which key "bar" has values "b" and "c" associated with it.
        /// </summary>
        /// <param name="allowDuplicateValues">Can the same value be associated with a key multiple times?</param>
        /// <param name="keyEqualityComparer">An IEqualityComparer&lt;TKey&gt; instance that will be used to compare keys.</param>
        /// <exception cref="InvalidOperationException">TValue does not implement either IComparable&lt;TValue&gt; or IComparable.</exception>
        public MultiDictionary(bool allowDuplicateValues, IEqualityComparer<TKey> keyEqualityComparer)
            : this(allowDuplicateValues, keyEqualityComparer, EqualityComparer<TValue>.Default) {}

        /// <summary>
        /// Create a new MultiDictionary. If duplicate values
        /// are allowed, multiple copies of the same value can be associated with the same key. For example, the key "foo"
        /// could have "a", "a", and "b" associated with it. If duplicate values are not allowed, only one copies of a given value can
        /// be associated with the same key, although different keys can have the same value. For example, the key "foo" could
        /// have "a" and "b" associated with it, which key "bar" has values "b" and "c" associated with it.
        /// </summary>
        /// <param name="allowDuplicateValues">Can the same value be associated with a key multiple times?</param>
        /// <param name="keyEqualityComparer">An IEqualityComparer&lt;TKey&gt; instance that will be used to compare keys.</param>
        /// <param name="valueEqualityComparer">An IEqualityComparer&lt;TValue&gt; instance that will be used to compare values.</param>
        public MultiDictionary(bool allowDuplicateValues, IEqualityComparer<TKey> keyEqualityComparer,
                               IEqualityComparer<TValue> valueEqualityComparer) {
            keyEqualityComparer.ShouldNotBeNull("keyEqualityComparer");
            valueEqualityComparer.ShouldNotBeNull("valueEqualityComparer");

            _allowDuplicateValues = allowDuplicateValues;
            _keyEqualityComparer = keyEqualityComparer;
            _valueEqualityComparer = valueEqualityComparer;

            _equalityComparer = new KeyAndValuesEqualityComparer(keyEqualityComparer);
            _hash = new Hash<KeyAndValues>(_equalityComparer);
        }

        /// <summary>
        /// Create a new MultiDictionary. Private constructor, for use by Clone().
        /// </summary>
        private MultiDictionary(bool allowDuplicateValues, IEqualityComparer<TKey> keyEqualityComparer,
                                IEqualityComparer<TValue> valueEqualityComparer, IEqualityComparer<KeyAndValues> equalityComparer,
                                Hash<KeyAndValues> hash) {
            keyEqualityComparer.ShouldNotBeNull("keyEqualityComparer");
            valueEqualityComparer.ShouldNotBeNull("valueEqualityComparer");

            _allowDuplicateValues = allowDuplicateValues;
            _keyEqualityComparer = keyEqualityComparer;
            _valueEqualityComparer = valueEqualityComparer;
            _equalityComparer = equalityComparer;
            _hash = hash;
        }

        #endregion Constructors

        #region Add or remove items

        /// <summary>
        /// <para>Adds a new value to be associated with a key. If duplicate values are permitted, this
        /// method always adds a new key-value pair to the dictionary.</para>
        /// <para>If duplicate values are not permitted, and <paramref name="key"/> already has a value
        /// equal to <paramref name="value"/> associated with it, then that value is replaced with <paramref name="value"/>,
        /// and the number of values associate with <paramref name="key"/> is unchanged.</para>
        /// </summary>
        /// <param name="key">The key to associate with.</param>
        /// <param name="value">The value to associated with <paramref name="key"/>.</param>
        public override sealed void Add(TKey key, TValue value) {
            var keyValues = new KeyAndValues(key);
            KeyAndValues existing;

            if(_hash.Find(keyValues, false, out existing)) {
                // There already is an item in the hash table equal to this key. Add the new value,
                // taking into account duplicates if needed.
                var existingCount = existing.Count;

                if(_allowDuplicateValues == false) {
                    var valueHash = Util.GetHashCode(value, _valueEqualityComparer);

                    for(var i = 0; i < existingCount; ++i) {
                        if(Util.GetHashCode(existing.Values[i], _valueEqualityComparer) == valueHash &&
                           _valueEqualityComparer.Equals(existing.Values[i], value)) {
                            // Found an equal existing value. Replace it and we're done.
                            existing.Values[i] = value;
                            return;
                        }
                    }
                }

                // Add a new value to an existing key.
                if(existingCount == existing.Values.Length) {
                    // Grow the array to make room.
                    var newValues = new TValue[existingCount * 2];
                    Array.Copy(existing.Values, newValues, existingCount);
                    existing.Values = newValues;
                }
                existing.Values[existingCount] = value;
                existing.Count = existingCount + 1;

                // Update the hash table.
                _hash.Find(existing, true, out keyValues);
                return;
            }
            // No item with this key. Add it.
            keyValues.Count = 1;
            keyValues.Values = new TValue[1] { value };

            _hash.Insert(keyValues, true, out existing);
        }

        /// <summary>
        /// Removes a given value from the values associated with a key. If the
        /// last value is removed from a key, the key is removed also.
        /// </summary>
        /// <param name="key">A key to remove a value from.</param>
        /// <param name="value">The value to remove.</param>
        /// <returns>True if <paramref name="value"/> was associated with <paramref name="key"/> (and was
        /// therefore removed). False if <paramref name="value"/> was not associated with <paramref name="key"/>.</returns>
        public override sealed bool Remove(TKey key, TValue value) {
            var keyValues = new KeyAndValues(key);
            KeyAndValues existing;

            if(_hash.Find(keyValues, false, out existing)) {
                // There is an item in the hash table equal to this key. Find the value.
                var existingCount = existing.Count;
                var valueHash = Util.GetHashCode(value, _valueEqualityComparer);
                var indexFound = -1;

                for(var i = 0; i < existingCount; ++i) {
                    if(Util.GetHashCode(existing.Values[i], _valueEqualityComparer) == valueHash &&
                       _valueEqualityComparer.Equals(existing.Values[i], value)) {
                        // Found an equal existing value
                        indexFound = i;
                    }
                }

                if(existingCount == 1) {
                    // Removing the last value. Remove the key.
                    _hash.Delete(existing, out keyValues);
                    return true;
                }

                if(indexFound >= 0) {
                    // Found a value. Remove it.
                    if(indexFound < existingCount - 1)
                        Array.Copy(existing.Values, indexFound + 1, existing.Values, indexFound, existingCount - indexFound - 1);
                    existing.Count = existingCount - 1;

                    // Update the hash.
                    _hash.Find(existing, true, out keyValues);
                    return true;
                }
                // Value was not found.
                return false;
            }

            return false; // key not found.
        }

        /// <summary>
        /// Removes a key and all associated values from the dictionary. If the
        /// key is not present in the dictionary, it is unchanged and false is returned.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True if the key was present and was removed. Returns 
        /// false if the key was not present.</returns>
        public override sealed bool Remove(TKey key) {
            KeyAndValues dummy;
            return _hash.Delete(new KeyAndValues(key), out dummy);
        }

        /// <summary>
        /// Removes all keys and values from the dictionary.
        /// </summary>
        public override sealed void Clear() {
            _hash.StopEnumerations(); // Invalidate any enumerations.

            // The simplest and fastest way is simply to throw away the old hash and create a new one.
            _hash = new Hash<KeyAndValues>(_equalityComparer);
        }

        #endregion Add or remove items

        #region Query items

        /// <summary>
        /// Returns the IEqualityComparer&lt;T&gt; used to compare keys in this dictionary. 
        /// </summary>
        /// <value>If the dictionary was created using a comparer, that comparer is returned. Otherwise
        /// the default comparer for TKey (EqualityComparer&lt;TKey&gt;.Default) is returned.</value>
        public IEqualityComparer<TKey> KeyComparer {
            get { return _keyEqualityComparer; }
        }

        /// <summary>
        /// Returns the IEqualityComparer&lt;T&gt; used to compare values in this dictionary. 
        /// </summary>
        /// <value>If the dictionary was created using a comparer, that comparer is returned. Otherwise
        /// the default comparer for TValue (EqualityComparer&lt;TValue&gt;.Default) is returned.</value>
        public IEqualityComparer<TValue> ValueComparer {
            get { return _valueEqualityComparer; }
        }

        /// <summary>
        /// Determine if two values are equal.
        /// </summary>
        /// <param name="value1">First value to compare.</param>
        /// <param name="value2">Second value to compare.</param>
        /// <returns>True if the values are equal.</returns>
        protected override sealed bool EqualValues(TValue value1, TValue value2) {
            return _valueEqualityComparer.Equals(value1, value2);
        }

        /// <summary>
        /// Gets the number of key-value pairs in the dictionary. Each value associated
        /// with a given key is counted. If duplicate values are permitted, each duplicate
        /// value is included in the count.
        /// </summary>
        /// <value>The number of key-value pairs in the dictionary.</value>
        public override sealed int Count {
            get { return _hash.ElementCount; }
        }

        /// <summary>
        /// Checks to see if <paramref name="value"/> is associated with <paramref name="key"/>
        /// in the dictionary.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <param name="value">The value to check.</param>
        /// <returns>True if <paramref name="value"/> is associated with <paramref name="key"/>.</returns>
        public override sealed bool Contains(TKey key, TValue value) {
            KeyAndValues item;

            if(_hash.Find(new KeyAndValues(key), false, out item)) {
                var existingCount = item.Count;
                var valueHash = Util.GetHashCode(value, _valueEqualityComparer);

                for(var i = 0; i < existingCount; ++i) {
                    if(Util.GetHashCode(item.Values[i], _valueEqualityComparer) == valueHash &&
                       _valueEqualityComparer.Equals(item.Values[i], value)) {
                        // Found an equal existing value. 
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the key is present in the dictionary and has
        /// at least one value associated with it.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if <paramref name="key"/> is present and has at least
        /// one value associated with it. Returns false otherwise.</returns>
        public override sealed bool ContainsKey(TKey key) {
            KeyAndValues temp;
            return _hash.Find(new KeyAndValues(key), false, out temp);
        }

        /// <summary>
        /// Enumerate all the keys in the dictionary. 
        /// </summary>
        /// <returns>An IEnumerator&lt;TKey&gt; that enumerates all of the keys in the dictionary that
        /// have at least one value associated with them.</returns>
        protected override sealed IEnumerator<TKey> EnumerateKeys() {
            return _hash.Select(item => item.Key).GetEnumerator();
        }

        /// <summary>
        ///  Enumerate the values in the a KeyAndValues structure. Can't return
        /// the array directly because:
        ///   a) The array might be larger than the count.
        ///   b) We can't allow clients to down-cast to the array and modify it.
        ///   c) We have to abort enumeration if the hash changes.
        /// </summary>
        /// <param name="keyAndValues">Item with the values to enumerate..</param>
        /// <returns>An enumerable that enumerates the items in the KeyAndValues structure.</returns>
        private IEnumerator<TValue> EnumerateValues(KeyAndValues keyAndValues) {
            var count = keyAndValues.Count;
            var stamp = _hash.GetEnumerationStamp();

            for(var i = 0; i < count; ++i) {
                yield return keyAndValues.Values[i];
                _hash.CheckEnumerationStamp(stamp);
            }
        }

        /// <summary>
        /// Determines if this dictionary contains a key equal to <paramref name="key"/>. If so, all the values
        /// associated with that key are returned through the values parameter. 
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="values">Returns all values associated with key, if true was returned.</param>
        /// <returns>True if the dictionary contains key. False if the dictionary does not contain key.</returns>
        protected override sealed bool TryEnumerateValuesForKey(TKey key, out IEnumerator<TValue> values) {
            KeyAndValues item;

            if(_hash.Find(new KeyAndValues(key), false, out item)) {
                values = EnumerateValues(item);
                return true;
            }
            values = null;
            return false;
        }

        /// <summary>
        /// Gets the number of values associated with a given key.
        /// </summary>
        /// <param name="key">The key to count values of.</param>
        /// <returns>The number of values associated with <paramref name="key"/>. If <paramref name="key"/>
        /// is not present in the dictionary, zero is returned.</returns>
        protected override sealed int CountValues(TKey key) {
            KeyAndValues item;

            return _hash.Find(new KeyAndValues(key), false, out item) ? item.Count : 0;
        }

        #endregion Query items

        #region Cloning

        /// <summary>
        /// Makes a shallow clone of this dictionary; i.e., if keys or values of the
        /// dictionary are reference types, then they are not cloned. If TKey or TValue is a value type,
        /// then each element is copied as if by simple assignment.
        /// </summary>
        /// <remarks>Cloning the dictionary takes time O(N), where N is the number of key-value pairs in the dictionary.</remarks>
        /// <returns>The cloned dictionary.</returns>
        public MultiDictionary<TKey, TValue> Clone() {
            return new MultiDictionary<TKey, TValue>(_allowDuplicateValues, _keyEqualityComparer, _valueEqualityComparer,
                                                     _equalityComparer,
                                                     _hash.Clone(KeyAndValues.Copy));
        }

        /// <summary>
        /// Throw an InvalidOperationException indicating that this type is not cloneable.
        /// </summary>
        /// <param name="t">Type to test.</param>
        private static void NonCloneableType(Type t) {
            throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, t.FullName));
        }

        /// <summary>
        /// Makes a deep clone of this dictionary. A new dictionary is created with a clone of
        /// each entry of this dictionary, by calling ICloneable.Clone on each element. If TKey or TValue is
        /// a value type, then each element is copied as if by simple assignment.
        /// </summary>
        /// <remarks><para>If TKey or TValue is a reference type, it must implement
        /// ICloneable. Otherwise, an InvalidOperationException is thrown.</para>
        /// <para>Cloning the dictionary takes time O(N log N), where N is the number of key-value pairs in the dictionary.</para></remarks>
        /// <returns>The cloned dictionary.</returns>
        /// <exception cref="InvalidOperationException">TKey or TValue is a reference type that does not implement ICloneable.</exception>
        public MultiDictionary<TKey, TValue> CloneContents() {
            bool keyIsValueType, valueIsValueType;

            // Make sure that TKey and TValue can be cloned.
            if(Util.IsCloneableType(typeof(TKey), out keyIsValueType) == false)
                NonCloneableType(typeof(TKey));

            if(Util.IsCloneableType(typeof(TValue), out valueIsValueType) == false)
                NonCloneableType(typeof(TValue));

            // It's tempting to do a more efficient cloning, utilizing the hash.Clone() method. However, we can't know that
            // the cloned version of the key has the same hash value.

            var newDict = new MultiDictionary<TKey, TValue>(_allowDuplicateValues, _keyEqualityComparer, _valueEqualityComparer);

            foreach(KeyAndValues item in _hash) {
                // Clone the key and values parts. Value types can be cloned
                // by just copying them, otherwise, ICloneable is used.

                TKey keyClone = keyIsValueType
                                    ? item.Key
                                    : (Equals(item.Key, null)
                                           ? default(TKey)
                                           : item.Key); //(TKey) (((ICloneable) item.Key).Clone()));


                var valuesClone = new TValue[item.Count];

                if(valueIsValueType)
                    Array.Copy(item.Values, valuesClone, item.Count);
                else {
                    for(int i = 0; i < item.Count; ++i) {
                        valuesClone[i] = Equals(item.Values[i], null)
                                             ? default(TValue)
                                             : item.Values[i]; // (TValue) (((ICloneable) item.Values[i]).Clone());
                    }
                }

                newDict.AddMany(keyClone, valuesClone);
            }

            return newDict;
        }

        #endregion Cloning
    }
}