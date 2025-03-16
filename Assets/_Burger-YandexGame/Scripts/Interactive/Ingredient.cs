using UnityEngine;

public class Ingredient : MonoBehaviour
{
    [field: SerializeField] public IngredientInfo IngredientInfo { get; private set; }
}

[System.Serializable]
public class IngredientInfo
{
    public IngredientType IngredientType { get; private set; }
    public string Name { get; private set; }
}