using UnityEngine;
using DG.Tweening; // Asegúrate de que DOTween esté importado
using System.Collections;
using System.Collections.Generic;

public class SequentialTextFader : MonoBehaviour
{
    [System.Serializable]
    public class FadeItem
    {
        [Tooltip("Arrastra aquí el GameObject que tiene el CanvasGroup. Este objeto debería contener el texto y el script ButtonEffect.")]
        public CanvasGroup elementCanvasGroup;

        [Tooltip("Retraso en segundos ANTES de que este elemento comience su fade, después de que el anterior HAYA TERMINADO.")]
        public float delayAfterPreviousEnds = 0.2f;

        [Tooltip("Duración en segundos del efecto de fade-in para este elemento.")]
        public float fadeDuration = 1.0f;
    }

    [Tooltip("Lista de elementos (con CanvasGroup) que se desvanecerán en secuencia.")]
    public List<FadeItem> itemsToFade = new List<FadeItem>();

    private int currentItemIndex = 0;

    void Start()
    {
        // 1. Inicializar todos los CanvasGroups: invisibles y no interactuables.
        // El color del TextMeshProUGUI (incluyendo su alfa) debe estar como lo quieres al final (usualmente alfa = 1).
        // ButtonEffect tomará el color inicial del TextMeshProUGUI en su propio Start().
        foreach (FadeItem item in itemsToFade)
        {
            if (item.elementCanvasGroup != null)
            {
                item.elementCanvasGroup.alpha = 0f;
                item.elementCanvasGroup.interactable = false;
                item.elementCanvasGroup.blocksRaycasts = false; // Importante para evitar clics "fantasma"
            }
            else
            {
                Debug.LogWarning("Un elementCanvasGroup en la lista no está asignado y será ignorado.", this);
            }
        }

        // 2. Iniciar la secuencia de fade-in.
        if (itemsToFade.Count > 0)
        {
            StartCoroutine(FadeInSequenceCoroutine());
        }
        else
        {
            Debug.LogWarning("La lista 'itemsToFade' está vacía. No hay nada que animar.", this);
        }
    }

    IEnumerator FadeInSequenceCoroutine()
    {
        while (currentItemIndex < itemsToFade.Count)
        {
            FadeItem currentItem = itemsToFade[currentItemIndex];

            if (currentItem.elementCanvasGroup == null)
            {
                Debug.LogWarning($"Elemento en el índice {currentItemIndex} no tiene CanvasGroup asignado. Saltando.", this);
                currentItemIndex++;
                continue;
            }

            // Si el GameObject del CanvasGroup está inactivo, el fade no será visible.
            // Considera si necesitas activarlos aquí. Por ahora, se activará con una advertencia.
            if (!currentItem.elementCanvasGroup.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"El GameObject del CanvasGroup '{currentItem.elementCanvasGroup.name}' está inactivo. Se intentará activar para el fade.", this);
                currentItem.elementCanvasGroup.gameObject.SetActive(true);
            }

            // Esperar el retraso configurado.
            if (currentItem.delayAfterPreviousEnds > 0)
            {
                yield return new WaitForSeconds(currentItem.delayAfterPreviousEnds);
            }

            // Iniciar el fade para el elemento actual usando DOTween y esperar a que termine.
            if (currentItem.fadeDuration > 0f)
            {
                // DOFade anima el CanvasGroup.alpha. WaitForCompletion() pausa la corutina hasta que el tween termine.
                yield return currentItem.elementCanvasGroup.DOFade(1f, currentItem.fadeDuration)
                                     .SetEase(Ease.Linear) // Puedes cambiar el tipo de Ease según necesites
                                     .WaitForCompletion();
            }
            else // Si la duración es 0, hacerlo visible instantáneamente.
            {
                currentItem.elementCanvasGroup.alpha = 1f;
            }

            // Una vez que el fade-in está completo, hacerlo interactuable.
            currentItem.elementCanvasGroup.interactable = true;
            currentItem.elementCanvasGroup.blocksRaycasts = true; // Permitir que reciba eventos de ratón

            currentItemIndex++;
        }

        Debug.Log("Toda la secuencia de fade-in completada!", this);
    }
}