using System;
using Unity.VisualScripting;
using UnityEngine;

public class Boxes : MonoBehaviour
{
    public static event Action <int> OnACollisionWeight;
    public static event Action<int> OnBCollisionWeight;
    public enum TipoCaja
    {
        TipoA,
        TipoB
    }

    public TipoCaja tipo;
    public int weight;
    void Start()
    {
        switch (tipo)
        {
            case TipoCaja.TipoA:
                Debug.Log("Esta es una caja tipo A");
                
                break;

            case TipoCaja.TipoB:
                Debug.Log("Esta es una caja tipo b");
               
                break;
      
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (tipo == TipoCaja.TipoA)
        {
            if (other.tag == "star")
            {
                Debug.Log("estrellita donde estas");
                OnACollisionWeight?.Invoke(2);
                weight = 2;

            }
            else if (other.tag == "ship")
            {
                Debug.Log("navecita");
                OnACollisionWeight?.Invoke(1);
                weight = 1;
            }
            else if (other.tag == "moon")
            {
                Debug.Log("luna");
                OnACollisionWeight?.Invoke(3);
                weight = 3;
            }
            else if (other.tag == "alien")
            {
                Debug.Log("alien"); OnACollisionWeight?.Invoke(4);
                weight = 4;
            }
            else if (other.tag == "saturn")
            {
                Debug.Log("en saturno"); OnACollisionWeight?.Invoke(5);
                weight = 5;
            }
            else
            {
                Debug.Log("no hijito");
            }
            Destroy(other.gameObject);
        }
        if (tipo == TipoCaja.TipoB)
        {
            if (other.tag == "star")
            {
                Debug.Log("estrellita donde estas");
                OnBCollisionWeight?.Invoke(2);
                weight = 2;
            }
            else if (other.tag == "ship")
            {
                Debug.Log("navecita");
                OnBCollisionWeight?.Invoke(1);
                weight = 1;
            }
            else if (other.tag == "moon")
            {
                Debug.Log("luna");
                OnBCollisionWeight?.Invoke(3);
                weight = 3;
            }
            else if (other.tag == "alien")
            {
                Debug.Log("alien"); OnBCollisionWeight?.Invoke(4);
                weight = 4;
            }
            else if (other.tag == "saturn")
            {
                Debug.Log("en saturno"); OnBCollisionWeight?.Invoke(5);
                weight = 5;
            }
            else
            {
                Debug.Log("no hijito");
            }
            Destroy(other.gameObject);
        }
        
    }
}
