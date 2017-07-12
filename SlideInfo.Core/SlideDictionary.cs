using System;
using System.Collections;
using System.Collections.Generic;

namespace SlideInfo.Core
{
	public abstract class SlideDictionary<T> : IDictionary<string, T>
	{
		protected AbstractSlide Osr;
		protected Dictionary<string, T> InternalDict = new Dictionary<string, T>();

		protected SlideDictionary(AbstractSlide osr)
		{
			Osr = osr;
		}

	    public IDictionary<string, T> ToDictionary()
	    {
	        return InternalDict;
	    }

		public bool ContainsKey(string key)
		{
			return InternalDict.ContainsKey(key);
		}

		public void Add(string key, T value)
		{
			InternalDict.Add(key, value);
		}

		public bool Remove(string key)
		{
			return InternalDict.Remove(key);
		}

		public bool TryGetValue(string key, out T value)
		{
			return InternalDict.TryGetValue(key, out value);
		}

		public virtual T this[string key]
		{
			get => InternalDict[key];
			set => InternalDict[key] = value;
		}
	
		public virtual ICollection<string> Keys => InternalDict.Keys;
		public ICollection<T> Values => InternalDict.Values;

		public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
		{
			return InternalDict.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(KeyValuePair<string, T> item)
		{
			InternalDict.Add(item.Key, item.Value);
		}

		public void Clear()
		{
			InternalDict.Clear();
		}

		public bool Contains(KeyValuePair<string, T> item)
		{
			return InternalDict.ContainsKey(item.Key) || InternalDict.ContainsValue(item.Value);
		}

		public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(KeyValuePair<string, T> item)
		{
			return InternalDict.Remove(item.Key);
		}

		public int Count => InternalDict.Count;
		public bool IsReadOnly => false;
	}
}
