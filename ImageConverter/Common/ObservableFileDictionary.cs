using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation.Collections;

namespace ImageConverter.Common
{
    public class ObservableFileDictionary : IObservableMap<string, ImageViewModel>
    {
        private class ObservableDictionaryChangedEventArgs : IMapChangedEventArgs<string>
        {
            public ObservableDictionaryChangedEventArgs(CollectionChange change, string key)
            {
                this.CollectionChange = change;
                this.Key = key;
            }

            public CollectionChange CollectionChange { get; private set; }
            public string Key { get; private set; }
        }

        private Dictionary<string, ImageViewModel> _dictionary = new Dictionary<string, ImageViewModel>();
        public event MapChangedEventHandler<string, ImageViewModel> MapChanged;

        private void InvokeMapChanged(CollectionChange change, string key)
        {
            MapChanged?.Invoke(this, new ObservableDictionaryChangedEventArgs(change, key));
        }

        public void Add(ImageViewModel image)
        {
            this._dictionary.Add(image.File.Path, image);
            this.InvokeMapChanged(CollectionChange.ItemInserted, image.File.Path);
        }

        public void Add(string key, ImageViewModel value) 
            => Add(value);

        public void Add(KeyValuePair<string, ImageViewModel> item)
            => Add(item.Value);

        public bool Remove(string key)
        {
            if (this._dictionary.Remove(key))
            {
                this.InvokeMapChanged(CollectionChange.ItemRemoved, key);
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<string, ImageViewModel> item)
        {
            if (this._dictionary.TryGetValue(item.Value.File.Path, out ImageViewModel currentValue) &&
                Object.Equals(item.Value, currentValue) && this._dictionary.Remove(item.Key))
            {
                this.InvokeMapChanged(CollectionChange.ItemRemoved, item.Key);
                return true;
            }
            return false;
        }

        public ImageViewModel this[string key]
        {
            get
            {
                return this._dictionary[key];
            }
            set
            {
                this._dictionary[key] = value;
                this.InvokeMapChanged(CollectionChange.ItemChanged, key);
            }
        }

        public void Clear()
        {
            var priorKeys = this._dictionary.Keys.ToArray();
            this._dictionary.Clear();
            foreach (var key in priorKeys)
            {
                this.InvokeMapChanged(CollectionChange.ItemRemoved, key);
            }
        }

        public ICollection<string> Keys 
            => _dictionary.Keys;

        public bool ContainsKey(string key) 
            => _dictionary.ContainsKey(key);

        public bool TryGetValue(string key, out ImageViewModel value) 
            => _dictionary.TryGetValue(key, out value);

        public ICollection<ImageViewModel> Values 
            => _dictionary.Values;

        public bool Contains(ImageViewModel file) 
            => ContainsKey(file.File.Path);

        public bool Contains(KeyValuePair<string, ImageViewModel> item) 
            => _dictionary.ContainsKey(item.Value.File.Path);

        public int Count 
            => _dictionary.Count; 

        public bool IsReadOnly 
            => false;

        public IEnumerator<KeyValuePair<string, ImageViewModel>> GetEnumerator() 
            => _dictionary.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => _dictionary.GetEnumerator();

        public void CopyTo(KeyValuePair<string, ImageViewModel>[] array, int arrayIndex)
        {
            int arraySize = array.Length;
            foreach (var pair in this._dictionary)
            {
                if (arrayIndex >= arraySize) break;
                array[arrayIndex++] = pair;
            }
        }

        
    }
}
