using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Aseg�rate de tener esta l�nea para TextMeshPro
using DG.Tweening; // Aseg�rate de tener DOTween importado

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float targetScale = 1.2f;
    public Color targetColor = Color.yellow; // Color al que cambiar� el texto
    public float duration = 0.2f;

    private Vector3 initialScale;
    private Color initialColor; // Color original del texto
    private TextMeshProUGUI tmpText; // Referencia al componente TextMeshPro

    // Variable para controlar si el puntero est� actualmente sobre el bot�n
    private bool isPointerOver = false;

    void Start()
    {
        initialScale = transform.localScale;

        tmpText = GetComponent<TextMeshProUGUI>();
        if (tmpText == null)
        {
            tmpText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (tmpText != null)
        {
            initialColor = tmpText.color;
        }
        else
        {
            Debug.LogWarning("No se encontr� un componente TextMeshProUGUI en este GameObject o sus hijos.", this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        transform.DOKill(); // Detiene animaciones previas de escala en este transform
        transform.DOScale(targetScale, duration);

        if (tmpText != null)
        {
            tmpText.DOKill(); // Detiene animaciones previas de color en este texto
            tmpText.DOColor(targetColor, duration);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        // Solo revertir si no se est� desactivando (OnDisable se encargar� si es el caso)
        // y si el componente sigue activo y habilitado.
        if (this.enabled && gameObject.activeInHierarchy)
        {
            RevertToInitialState();
        }
    }

    // Se llama cuando el componente se desactiva o el GameObject se desactiva
    void OnDisable()
    {
        // Si el puntero estaba sobre el bot�n cuando se desactiv�,
        // o simplemente para asegurar un estado limpio, revertimos.
        // Es importante matar las animaciones primero.
        transform.DOKill();
        transform.localScale = initialScale;

        if (tmpText != null)
        {
            tmpText.DOKill();
            tmpText.color = initialColor;
        }
        isPointerOver = false; // Reseteamos el estado del puntero
    }

    // M�todo para revertir al estado inicial, se puede llamar desde OnPointerExit o OnDisable
    private void RevertToInitialState(bool animate = true)
    {
        // Detenemos cualquier animaci�n en curso para evitar conflictos
        transform.DOKill();
        if (tmpText != null)
        {
            tmpText.DOKill();
        }

        if (animate)
        {
            transform.DOScale(initialScale, duration);
            if (tmpText != null)
            {
                tmpText.DOColor(initialColor, duration);
            }
        }
        else // Revertir instant�neamente
        {
            transform.localScale = initialScale;
            if (tmpText != null)
            {
                tmpText.color = initialColor;
            }
        }
    }

    // Sobreescribimos OnPointerExit para usar el nuevo m�todo con animaci�n
    // OnPointerExit ya estaba definido, as� que ajustamos su l�gica
    // (El c�digo anterior de OnPointerExit ha sido movido y modificado)

    // Para ser m�s expl�citos, ajustemos OnPointerExit para que llame a RevertToInitialState con animaci�n
    // La versi�n anterior de OnPointerExit es:
    /*
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(initialScale, duration);
        if (tmpText != null)
        {
            tmpText.DOColor(initialColor, duration);
        }
    }
    */
    // La nueva l�gica en OnPointerExit ya est� arriba, pero aseg�rate de que est� as�:
    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     isPointerOver = false;
    //     if (this.enabled && gameObject.activeInHierarchy) // Asegura que el objeto sigue activo
    //     {
    //         RevertToInitialState(true); // Llama a revertir con animaci�n
    //     }
    // }

    // Y en OnDisable, revertimos sin animaci�n:
    // void OnDisable()
    // {
    //     RevertToInitialState(false); // Llama a revertir sin animaci�n
    //     isPointerOver = false;
    // }
}