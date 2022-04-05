using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Oqtane.Shared
{
    public class PropertyDictionary : DynamicObject, IDictionary<string, object>, INotifyPropertyChanged
    {
        readonly IDictionary<string, object> _dictionary = new Dictionary<string, object>();

        public void Add(KeyValuePair<string, object> item)
        {
            _dictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _dictionary.Remove(item);
        }

        public int Count => _dictionary.Count;
        public bool IsReadOnly => _dictionary.IsReadOnly;

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!_dictionary.TryGetValue(binder.Name, out result)) result = null;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _dictionary[binder.Name] = value;
            OnPropertyChanged(binder.Name);
            return true;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_dictionary).GetEnumerator();
        }

        public void Add(string key, object value)
        {
            _dictionary.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public object this[string key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public ICollection<string> Keys => _dictionary.Keys;

        public ICollection<object> Values => _dictionary.Values;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
