using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Asegúrate de tener esta línea para TextMeshPro
using DG.Tweening; // Asegúrate de tener DOTween importado

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float targetScale = 1.2f;
    public Color targetColor = Color.yellow; // Color al que cambiará el texto
    public float duration = 0.2f;

    private Vector3 initialScale;
    private Color initialColor; // Color original del texto
    private TextMeshProUGUI tmpText; // Referencia al componente TextMeshPro

    // Variable para controlar si el puntero está actualmente sobre el botón
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
            Debug.LogWarning("No se encontró un componente TextMeshProUGUI en este GameObject o sus hijos.", this);
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
        // Solo revertir si no se está desactivando (OnDisable se encargará si es el caso)
        // y si el componente sigue activo y habilitado.
        if (this.enabled && gameObject.activeInHierarchy)
        {
            RevertToInitialState();
        }
    }

    // Se llama cuando el componente se desactiva o el GameObject se desactiva
    void OnDisable()
    {
        // Si el puntero estaba sobre el botón cuando se desactivó,
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

    // Método para revertir al estado inicial, se puede llamar desde OnPointerExit o OnDisable
    private void RevertToInitialState(bool animate = true)
    {
        // Detenemos cualquier animación en curso para evitar conflictos
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
        else // Revertir instantáneamente
        {
            transform.localScale = initialScale;
            if (tmpText != null)
            {
                tmpText.color = initialColor;
            }
        }
    }

    // Sobreescribimos OnPointerExit para usar el nuevo método con animación
    // OnPointerExit ya estaba definido, así que ajustamos su lógica
    // (El código anterior de OnPointerExit ha sido movido y modificado)

    // Para ser más explícitos, ajustemos OnPointerExit para que llame a RevertToInitialState con animación
    // La versión anterior de OnPointerExit es:
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
    // La nueva lógica en OnPointerExit ya está arriba, pero asegúrate de que esté así:
    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     isPointerOver = false;
    //     if (this.enabled && gameObject.activeInHierarchy) // Asegura que el objeto sigue activo
    //     {
    //         RevertToInitialState(true); // Llama a revertir con animación
    //     }
    // }

    // Y en OnDisable, revertimos sin animación:
    // void OnDisable()
    // {
    //     RevertToInitialState(false); // Llama a revertir sin animación
    //     isPointerOver = false;
    // }
}