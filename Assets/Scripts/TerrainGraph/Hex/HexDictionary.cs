using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Core;

namespace WanderingRoad.Procgen.RecursiveHex
{
    public class HexDictionary: IEnumerable<KeyValuePair<Vector3Int,Hex>>
    {
        private Dictionary<Vector3Int, Hex> _wrappedDictionary = new Dictionary<Vector3Int, Hex>();
        public Rect Bounds { get; private set; }
        private Rect _bufferedBounds;

        /// <summary>
        /// Initializes a new instance of the <see cref="HexDictionary{TKey,TValue}"/> class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        public HexDictionary()
        {
            Bounds = new Rect();
            _wrappedDictionary = new Dictionary<Vector3Int, Hex>();
        }

        public Dictionary<Vector3Int, Hex> GetDictionary()
        {
            return this._wrappedDictionary;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HexDictionary{TKey,TValue}"/> class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.VirtualDictionary`2"/> can contain.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0.</exception>
        public HexDictionary(int capacity)
        {
            _wrappedDictionary = new Dictionary<Vector3Int, Hex>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HexDictionary{TKey,TValue}"/> class that is empty, has the default initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"/>.
        /// </summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"/> for the type of the key.</param>
        public HexDictionary(IEqualityComparer<Vector3Int> comparer)
        {
            _wrappedDictionary = new Dictionary<Vector3Int, Hex>(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HexDictionary{TKey,TValue}"/> class that is empty, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"/>.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="HexDictionary{TKey,TValue}"/> can contain.</param><param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"/> for the type of the key.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0.</exception>
        public HexDictionary(int capacity, IEqualityComparer<Vector3Int> comparer)
        {
            _wrappedDictionary = new Dictionary<Vector3Int, Hex>(capacity, comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HexDictionary{TKey,TValue}"/> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2"/> and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2"/> whose elements are copied to the new <see cref="HexDictionary{TKey,TValue}"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="dictionary"/> is null.</exception><exception cref="T:System.ArgumentException"><paramref name="dictionary"/> contains one or more duplicate keys.</exception>
        public HexDictionary(IDictionary<Vector3Int, Hex> dictionary)
        {
            _wrappedDictionary = new Dictionary<Vector3Int, Hex>(dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HexDictionary{TKey,TValue}"/> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2"/> and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"/>.
        /// </summary>
        /// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2"/> whose elements are copied to the new <see cref="HexDictionary{TKey,TValue}"/>.</param><param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"/> for the type of the key.</param><exception cref="T:System.ArgumentNullException"><paramref name="dictionary"/> is null.</exception><exception cref="T:System.ArgumentException"><paramref name="dictionary"/> contains one or more duplicate keys.</exception>
        public HexDictionary(IDictionary<Vector3Int, Hex> dictionary, IEqualityComparer<Vector3Int> comparer)
        {
            _wrappedDictionary = new Dictionary<Vector3Int, Hex>(dictionary, comparer);
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param><param name="value">The object to use as the value of the element to add.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public void Add(Vector3Int key, Hex value)
        {
            if(_wrappedDictionary.Count == 0)
            {
                Bounds = value.Index.Bounds;
            }
            else
            {
                var b = Bounds;
                b.Encapsulate(value.Index.Bounds);
                Bounds = b;
            }
            
            var size = Bounds.size;
            _bufferedBounds = new Rect(Bounds.position - (Vector2.one*2), Bounds.size+(Vector2.one*4));
            _wrappedDictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool ContainsKey(Vector3Int key)
        {
            return _wrappedDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<Vector3Int> Keys
        {
            get
            {
                return _wrappedDictionary.Keys;
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key whose value to get.</param><param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetValue(Vector3Int key, out Hex value)
        {
            return _wrappedDictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<Hex> Values
        {
            get
            {
                return _wrappedDictionary.Values;
            }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="key">The key of the element to get or set.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public Hex this[Vector3Int key]
        {
            get
            {
                return _wrappedDictionary[key];
            }
            set
            {
                _wrappedDictionary[key] = value;
            }
        }



        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear()
        {
            _wrappedDictionary.Clear();
        }

        public IEnumerator<KeyValuePair<Vector3Int, Hex>> GetEnumerator()
        {
            return _wrappedDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _wrappedDictionary.GetEnumerator();
        }




        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get
            {
                return _wrappedDictionary.Count;
            }
        }
    }
}