using UnityEngine;
using System.Collections;
using System;
public class Pelotita : MonoBehaviour
{
    public static event Action OnLosem;

   
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("lose")) // Usar CompareTag es más eficiente
        {
            Debug.Log("perderXD");
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            OnLosem?.Invoke();
        }
    }
}
