using UnityEngine;
[CreateAssetMenu(fileName = "Object", order = 1)]
public class Objects : ScriptableObject
{
    [SerializeField] GameObject objectType;
    [SerializeField] int weight;
}
