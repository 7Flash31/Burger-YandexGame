using DG.Tweening;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    [field: SerializeField] public IngredientType IngredientType { get; private set; } = IngredientType.None;
    [field: SerializeField] public string IngredientName { get; private set; } = "None";
    [field: SerializeField] public Sprite Icon{ get; private set; } 
    [SerializeField] private float _rotateSpeed;

    public BoxCollider BoxCollider { get; private set; }
    public bool IsDropped { get; set; }


    private Tween _tween;

    private void Start()
    {
        BoxCollider = GetComponent<BoxCollider>();
        IngredientName = gameObject.name;

        _tween = transform.DORotate(new Vector3(90, 360, 0), _rotateSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    public void StopAnimation()
    {
        _tween.Kill();
    }

    private void OnDestroy()
    {
        _tween.Kill();
    }
}

public class IngredientContainer
{
    public int IngredientCount { get; set; }
    public Ingredient Ingredient { get; set; }
}

public struct IngredientContainerStruct
{
    public int IngredientCount { get; set; }
    public IngredientType IngredientType { get; set; }
}