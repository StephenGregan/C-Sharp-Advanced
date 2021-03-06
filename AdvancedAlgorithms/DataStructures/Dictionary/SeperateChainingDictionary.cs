﻿using AdvancedAlgorithms.DataStructures.LinkedList;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AdvancedAlgorithms.DataStructures.Dictionary.Dictionary<K, V>;

namespace AdvancedAlgorithms.DataStructures.Dictionary
{
    /// <summary>
    /// 
    /// </summary>
    internal class SeperateChainingDictionary<K, V> : IDictionary<K, V>
    {
        private const double tolerence = 0.1;

        private DoublyLinkedList<DictionaryNode<K, V>>[] hashArray;
        private int bucketSize => hashArray.Length;
        private readonly int initialBucketSize;
        private int filledBuckets;

        public int Count { get; private set; }

        public SeperateChainingDictionary(int initialBucketSize = 3)
        {
            this.initialBucketSize = initialBucketSize;
            hashArray = new DoublyLinkedList<DictionaryNode<K, V>>[initialBucketSize];
        }

        public V this[K key]
        {
            get => GetValue(key);
            set => SetValue(key, value);
        }

        public bool ContainsKey(K key)
        {
            var index = Math.Abs(key.GetHashCode()) % bucketSize;

            if (hashArray[index] == null)
            {
                return false;
            }
            else
            {
                var current = hashArray[index].Head;

                while (current != null)
                {
                    if(current.Data.Key.Equals(key))
                    {
                        return true;
                    }

                    current = current.Next;
                }
            }

            return false;
        }

        public void Add(K key, V value)
        {
            Grow();

            var index = Math.Abs(key.GetHashCode()) % bucketSize;

            if (hashArray[index] == null)
            {
                hashArray[index] = new DoublyLinkedList<DictionaryNode<K, V>>();
                hashArray[index].InsertFirst(new DictionaryNode<K, V>(key, value));
                filledBuckets++;
            }
            else
            {
                var current = hashArray[index].Head;

                while (current != null)
                {
                    if (current.Data.Key.Equals(key))
                    {
                        throw new Exception("Duplicate key");
                    }

                    current = current.Next;
                }

                hashArray[index].InsertFirst(new DictionaryNode<K, V>(key, value));
            }

            Count++;
        }

        public void Remove(K key)
        {
            var index = Math.Abs(key.GetHashCode()) % bucketSize;

            if (hashArray[index] == null)
            {
                throw new Exception("No suck item for given key");
            }
            else
            {
                var current = hashArray[index].Head;

                // TODO merge both search and remove to a single loop here.
                DoublyLinkedList<DictionaryNode<K, V>> item = null;

                while (current != null)
                {
                    if (current.Data.Key.Equals(key))
                    {
                        item = current;
                        break;
                    }

                    current = current.Next;
                }

                // remove
                if (item == null)
                {
                    throw new Exception("No suck item for given key");
                }
                else
                {
                    hashArray[index].Delete(item);

                    // if the list is empty, mark bucket as null
                    if (hashArray[index].Head == null)
                    {
                        hashArray[index] = null;
                        filledBuckets--;
                    }
                }
            }
            Count--;
            Shrink();
        }

        public void Clear()
        {
            hashArray = new DoublyLinkedList<DictionaryNode<K, V>>[initialBucketSize];
            Count = 0;
            filledBuckets = 0;
        }

        private void SetValue(K key, V value)
        {
            var index = Math.Abs(key.GetHashCode()) % bucketSize;

            if (hashArray[index] == null)
            {
                Add(key, value);
            }
            else
            {
                var current = hashArray[index].Head;

                while (current != null)
                {
                    if(current.Data.Key.Equals(key))
                    {
                        Remove(key);
                        Add(key, value);
                        return;
                    }

                    current = current.Next;
                }
            }

            throw new Exception("Item not found");
        }

        private V GetValue(K key)
        {
            var index = Math.Abs(key.GetHashCode()) % bucketSize;

            if (hashArray[index] == null)
            {
                throw new Exception("Item not found");
            }
            else
            {
                var current = hashArray[index].Head;

                while (current != null)
                {
                    if (current.Data.Key.Equals(key))
                    {
                        return current.Data.Value;
                    }

                    current = current.Next;
                }
            }

            throw new Exception("Item not found");
        }

        private void Grow()
        {
            if (filledBuckets >= bucketSize * 0.7)
            {
                filledBuckets = 0;
                // Increase array size exponentially on demand.
                var newBucketSize = bucketSize * 2;

                var biggerArray = new DoublyLinkedList<DictionaryNode<K, V>>[newBucketSize];

                for (var i = 0; i < bucketSize; i++)
                {
                    var item = hashArray[i];

                    // hashcode changes when bucketSize changes.
                    if (item != null)
                    {
                        if (item.Head != null)
                        {
                            var current = item.Head;

                            // find new location for each item
                            while (current != null)
                            {
                                var next = current.Next;

                                var newIndex = Math.Abs(current.Data.Key.GetHashCode()) % newBucketSize;

                                if (biggerArray[newIndex] == null)
                                {
                                    filledBuckets++;
                                    biggerArray[newIndex] = new DoublyLinkedList<DictionaryNode<K, V>>();
                                }

                                biggerArray[newIndex].InsertFirst(current);

                                current = next;
                            }
                        }
                    }
                }

                hashArray = biggerArray;
            }
        }

        private void Shrink()
        {
            if (Math.Abs(filledBuckets - bucketSize * 0.3) < tolerence && bucketSize / 2 > initialBucketSize)
            {
                filledBuckets = 0;
                // reduce array by half.
                var newBucketSize = bucketSize / 2;

                var smallerArray = new DoublyLinkedList<DictionaryNode<K, V>>[newBucketSize];

                for (var i = 0; i < bucketSize; i++)
                {
                    var item = hashArray[i];

                    // hashcode changes when bucket size changes.
                    if (item?.Head != null)
                    {
                        var current = item.Head;

                        // find new location for each item.
                        while (current != null)
                        {
                            var next = current.Next;

                            var newIndex = Math.Abs(current.Data.Key.GetHashCode % newBucketSize);

                            if(smallerArray[newIndex] == null)
                            {
                                filledBuckets++;
                                smallerArray[newIndex] = new DoublyLinkedList<DictionaryNode<K, V>>();
                            }

                            smallerArray[newIndex].InsertFirst(current);

                            current = next;
                        }
                    }
                }

                hashArray = smallerArray;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return new SeperateChainingDictionaryEnumerator<K, V>(hashArray, bucketSize);
        }

        internal class SeperateChainingDictionaryEnumerator<TK, TV> : IEnumerator<KeyValuePair<TK, TV>>
        {
            internal DoublyLinkedList<DictionaryNode<TK, TV>>[] HashList;

            // Enumerators are positioned before the first element
            // until the first MoveNext() call.
            int position = -1;
            DoublyLinkedList<DictionaryNode<TK, TV>> currentNode = null;

            int length;

            internal SeperateChainingDictionaryEnumerator(DoublyLinkedList<DictionaryNode<TK, TV>>[] hashlist, int length)
            {
                this.length = length;
                this.HashList = hashlist;
            }

            public bool MoveNext()
            {
                if (currentNode?.Next != null)
                {
                    currentNode = currentNode.Next;
                    return true;
                }

                while (currentNode?.Next == null)
                {
                    position++;

                    if (position < length)
                    {
                        if (HashList[position] == null)
                            continue;

                        currentNode = HashList[position].Head;

                        if (currentNode == null)
                            continue;

                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                return false;
            }

            public void Reset()
            {
                position = -1;
                currentNode = null;
            }

            object IEnumerator.Current => Current;

            public KeyValuePair<TK, TV> Current
            {
                get
                {
                    try
                    {
                        return new KeyValuePair<TK, TV>(currentNode.Data.Key, currentNode.Data.Value);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new IndexOutOfRangeException();
                    }
                }
            }

            public void Dispose()
            {
                HashList = null;
            }
        }
    }
}
