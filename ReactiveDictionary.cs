    [Serializable]
    public class ReactiveDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> inner;

        public event Action<TKey, TValue> OnAddedOrUpdated;
        public event Action<TKey, TValue> OnRemoved;

        public ReactiveDictionary() => 
            inner = new Dictionary<TKey, TValue>();

        public ReactiveDictionary(Dictionary<TKey, TValue> initialData) => 
            inner = new Dictionary<TKey, TValue>(initialData);

        public IReadOnlyDictionary<TKey, TValue> Inner => inner;

        public void AddOrUpdate(TKey key, TValue value)
        {
            inner[key] = value;
            OnAddedOrUpdated?.Invoke(key, value);
        }

        public bool RemoveKey(TKey key)
        {
            if (inner.TryGetValue(key, out var oldValue))
            {
                inner.Remove(key);
                OnRemoved?.Invoke(key, oldValue);
                
                return true;
            }

            return false;
        }

        public bool ContainsKey(TKey key) 
            => inner.ContainsKey(key);

        public bool TryGetValue(TKey key, out TValue val) 
            => inner.TryGetValue(key, out val);

        public IDisposable SubscribeOnAddedOrUpdated(Action<TKey, TValue> callback)
        {
            OnAddedOrUpdated += callback;

            // foreach (var kv in inner)
            //   callback?.Invoke(kv.Key, kv.Value);

            return new DisposableAction(() => OnAddedOrUpdated -= callback);
        }

        public IDisposable SubscribeOnRemoved(Action<TKey, TValue> callback)
        {
            OnRemoved += callback;
            return new DisposableAction(() => OnRemoved -= callback);
        }

        private class DisposableAction : IDisposable
        {
            private readonly Action disposeAction;
            private bool isDisposed;

            public DisposableAction(Action disposeAction) => 
                this.disposeAction = disposeAction;

            public void Dispose()
            {
                if (!isDisposed)
                {
                    disposeAction?.Invoke();
                    isDisposed = true;
                }
            }
        }
    }
