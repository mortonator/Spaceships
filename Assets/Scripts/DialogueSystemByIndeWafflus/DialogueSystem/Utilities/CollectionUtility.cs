using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem
{
    public static class CollectionUtility
    {
        public static void AddItem<K, V>(this SerializableDictionary<K, List<V>> serialiseableDictionary, K key, V value)
        {
            if (serialiseableDictionary.ContainsKey(key))
            {
                serialiseableDictionary[key].Add(value);

                return;
            }

            serialiseableDictionary.Add(key, new List<V>() { value } );
        }
    }
}