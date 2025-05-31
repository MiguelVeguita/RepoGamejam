using UnityEngine;
using TMPro; // Necesario si usas TextMeshProUGUI
// using UnityEngine.UI; // Descomenta esta l�nea si usas el componente UI.Text antiguo
using System.Collections;
using System.Collections.Generic;

public class SequentialTextFader : MonoBehaviour
{
    [System.Serializable]
    public class FadeTextItem
    {
        // Arrastra tu elemento de texto aqu� desde la Jerarqu�a
        public TextMeshProUGUI textElement; // Para TextMeshPro
        // public Text textElement; // Descomenta y usa esto para UI.Text antiguo

        [Tooltip("Retraso en segundos ANTES de que este texto comience su fade, despu�s de que el anterior HAYA TERMINADO.")]
        public float delayAfterPreviousEnds = 0.2f;

        [Tooltip("Duraci�n en segundos del efecto de fade-in para este texto.")]
        public float fadeDuration = 1.0f;
    }

    [Tooltip("Lista de elementos de texto que se desvanecer�n en secuencia.")]
    public List<FadeTextItem> textItems = new List<FadeTextItem>();

    private int currentItemIndex = 0; // �ndice para rastrear el elemento actual

    void Start()
    {
        // 1. Inicializar todos los textos para que sean invisibles (alpha = 0)
        // Este foreach es solo para la configuraci�n inicial, no para la animaci�n secuencial.
        foreach (FadeTextItem item in textItems)
        {
            if (item.textElement != null)
            {
                Color currentColor = item.textElement.color;
                item.textElement.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
            }
            else
            {
                Debug.LogWarning("Un Text Element en la lista no est� asignado y ser� ignorado.");
            }
        }

        // 2. Iniciar la secuencia de fade-in si hay elementos
        if (textItems.Count > 0)
        {
            StartCoroutine(FadeInSequenceCoroutine());
        }
        else
        {
            Debug.LogWarning("La lista 'textItems' est� vac�a. No hay nada que animar.");
        }
    }

    IEnumerator FadeInSequenceCoroutine()
    {
        // Continuar mientras haya elementos en la lista por procesar
        while (currentItemIndex < textItems.Count)
        {
            FadeTextItem currentItem = textItems[currentItemIndex];

            if (currentItem.textElement == null)
            {
                Debug.LogWarning($"Elemento en el �ndice {currentItemIndex} no tiene TextMeshProUGUI asignado. Saltando.");
                currentItemIndex++; // Avanzar al siguiente elemento
                continue; // Saltar a la siguiente iteraci�n del bucle while
            }

            // Esperar el retraso configurado para este elemento
            // Para el primer elemento (index 0), este es el retraso inicial.
            // Para los siguientes, es el retraso despu�s de que el anterior termin�.
            if (currentItem.delayAfterPreviousEnds > 0)
            {
                yield return new WaitForSeconds(currentItem.delayAfterPreviousEnds);
            }

            // Iniciar el fade para el elemento actual
            yield return StartCoroutine(FadeElementCoroutine(currentItem.textElement, currentItem.fadeDuration));

            // Avanzar al siguiente elemento
            currentItemIndex++;
        }

        // Opcional: Aqu� puedes poner c�digo que se ejecute cuando toda la secuencia haya terminado
        Debug.Log("Toda la secuencia de fade-in completada!");
    }

    IEnumerator FadeElementCoroutine(TMP_Text textToFade, float duration) // Para TextMeshPro
    // IEnumerator FadeElementCoroutine(Text textToFade, float duration) // Para UI.Text antiguo (cambiar TMP_Text por Text)
    {
        if (textToFade == null) yield break; // Seguridad adicional

        if (duration <= 0f) // Si la duraci�n es 0 o negativa, hacerlo visible instant�neamente
        {
            Color finalColorImmediate = textToFade.color;
            textToFade.color = new Color(finalColorImmediate.r, finalColorImmediate.g, finalColorImmediate.b, 1f);
            yield break; // Salir de esta corutina
        }

        float elapsedTime = 0f;
        // El color original ya tiene alfa 0 debido a la inicializaci�n en Start()
        Color baseColor = textToFade.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Clamp01(elapsedTime / duration); // Calcula el alfa basado en el tiempo transcurrido

            textToFade.color = new Color(baseColor.r, baseColor.g, baseColor.b, newAlpha);

            yield return null; // Esperar al siguiente frame
        }

        // Asegurarse de que el alfa sea exactamente 1 al final
        textToFade.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
    }
}
