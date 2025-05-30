using UnityEngine;
using System.Collections.Generic;

public class DynamicObjectPooling<T> : MonoBehaviour where T : PoolObject
{
    [Header("Pool Settings")]
    [SerializeField] private T prefab;
    [SerializeField] private int initialSize = 5;

    private readonly List<T> pool = new List<T>();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
            AddObjectToPool();
    }

    private T AddObjectToPool()
    {
        T obj = Instantiate(prefab, transform);
        obj.OnDeactivate();
        pool.Add(obj);
        return obj;
    }

    public T GetObject()
    {
        int count = pool.Count;
        for (int i = 0; i < count; i++)
        {
            T obj = pool[i];
            if (!obj.gameObject.activeInHierarchy)
            {
                obj.OnActivate();
                return obj;
            }
        }
        T newObj = AddObjectToPool();
        newObj.OnActivate();
        return newObj;
    }

    
    public void ReturnObject(T obj)
    {
        if (pool.Contains(obj))
            obj.OnDeactivate();
        else
            Debug.LogWarning("Se intentó devolver un objeto que no es de este grupo.");
    }
}
