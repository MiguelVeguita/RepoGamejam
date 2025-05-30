using UnityEngine;
using System.Collections.Generic;

public class StaticObjectPooling<T> : MonoBehaviour where T : PoolObject
{
    [Header("Pool Settings")]
    [SerializeField] private T prefab;
    [SerializeField] private int initialSize = 10;

    private readonly List<T> pool = new List<T>();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            T obj = Instantiate(prefab, transform);
            obj.OnDeactivate();
            pool.Add(obj);
        }
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
        Debug.Log("No hay objetos inactivos disponibles en el grupo estático.");
        return null;
    }

    
    public void ReturnObject(T obj)
    {
        if (pool.Contains(obj))
        {
            obj.OnDeactivate();

        }
        else
            Debug.Log("Se intentó devolver un objeto que no es de este grupo.");
    }
}

