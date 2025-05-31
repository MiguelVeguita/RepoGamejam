using System.Collections.Generic;
using UnityEngine;


public class PoolObject : MonoBehaviour
{
   
    public virtual void OnActivate()
    {
        gameObject.SetActive(true);
    }

  
    public virtual void OnDeactivate()
    {
        gameObject.SetActive(false);
    }
}


