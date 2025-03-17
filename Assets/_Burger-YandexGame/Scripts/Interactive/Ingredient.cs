using DG.Tweening;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    [field: SerializeField] public IngredientType IngredientType { get; private set; } = IngredientType.None;
    [SerializeField] private float _rotateSpeed;

    public BoxCollider BoxCollider { get; private set; }


    private Tween _tween;

    private void Start()
    {
        BoxCollider = GetComponent<BoxCollider>();
        if(BoxCollider == null)
        {
            BoxCollider = GetComponentInChildren<BoxCollider>();
        }

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