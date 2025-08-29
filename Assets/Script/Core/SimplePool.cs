using System.Collections.Generic;
using UnityEngine;

public class SimplePool<T> where T: Component
{
    private readonly Queue<T> _pool = new Queue<T>();
    private readonly T _prefab;
    private readonly Transform _parent;

    public SimplePool(T prefab, int initial, Transform parent) {
        _prefab = prefab;
        _parent = parent;
        for (int i = 0; i < initial; i++) {
            var inst = GameObject.Instantiate(_prefab, _parent);
            inst.gameObject.SetActive(false);
            _pool.Enqueue(inst);
        }
    }

    public T Get() {
        if (_pool.Count > 0) {
            var x = _pool.Dequeue();
            x.gameObject.SetActive(true);
            return x;
        }
        return GameObject.Instantiate(_prefab, _parent);
    }

    public void Return(T obj) {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}
