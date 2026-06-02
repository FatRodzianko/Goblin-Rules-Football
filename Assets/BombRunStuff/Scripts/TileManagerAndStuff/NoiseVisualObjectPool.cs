using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class NoiseVisualObjectPool 
{
    private GameObject _prefab;
    private Transform _parent;
    private ObjectPool<GameObject> _pool;
    private int _defaultSize;
    private int _maxSize;

    // Our class's constructor. Takes the prefab to spawn as an argument.
    public NoiseVisualObjectPool(GameObject prefab, Transform parent, int defaultSize = 100, int maxSize = 250)
    {
        this._prefab = prefab;  // The prefab to spawn.
        this._parent = parent;
        this._defaultSize = defaultSize;  // Pool's starting number of objects.
        this._maxSize = maxSize;  // Max size for our pool.
        // Initializing our pool. This won't work; it's missing some arguments.
        _pool = new ObjectPool<GameObject>(
        CreatePooledObject,
            OnGetFromPool,
            OnReturnToPool,
            OnDestroyPooledObject,
            true,
            _defaultSize,
            _maxSize
            );
    }
    // Wrapper function for pool.Get. Gets object and sets position.
    public GameObject GetObject(Vector3 position)
    {
        GameObject obj = _pool.Get();
        obj.transform.position = position;
        return obj;
    }

    // Wrapper function for pool.Release
    public void ReleaseObject(GameObject obj)
    {
        _pool.Release(obj);
    }

    // Return a brand new GameObject instance for our pool to use.
    // We have to specify GameObject.Instantiate because this isn't
    // a Monobehavior.    
    GameObject CreatePooledObject()
    {
        GameObject newObject = GameObject.Instantiate(_prefab, _parent);
        return newObject;
    }
    // When an object is taken from the pool, activate it.
    void OnGetFromPool(GameObject pooledObject)
    {
        pooledObject.SetActive(true);
    }
    // When an object is returned to the pool, deactivate it.
    void OnReturnToPool(GameObject pooledObject)
    {
        pooledObject.SetActive(false);
    }
    // When the pool discards an object, destroy the GameObject.
    void OnDestroyPooledObject(GameObject pooledObject)
    {
        GameObject.Destroy(pooledObject);
    }
}
