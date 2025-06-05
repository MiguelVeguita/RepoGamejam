using System;
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
        if (tipo == TipoCaja.TipoA || tipo == TipoCaja.TipoB)
        {
            if (other.tag == "star")
            {
                SetWeight(3, "estrellita donde estas");
            }
            else if (other.tag == "ship")
            {
                SetWeight(1, "nave");
            }
            else if (other.tag == "moon")
            {
                SetWeight(10, "luna");
            }
            else if (other.tag == "alien")
            {
                SetWeight(4, "alien");
            }
            else if (other.tag == "saturn")
            {
                SetWeight(8, "saturno");
            }
            else
            {
                Debug.Log("xd");
            }
        }
    }

    private void SetWeight(int weight, string messageDebug = "")
    {
        Debug.Log(messageDebug);
        this.weight = weight;
        OnACollisionWeight?.Invoke(this.weight);
    }
}
