using DG.Tweening;
using TMPro;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public bool IsBadIngredient { get; private set; }

    [SerializeField] private float _rotateSpeed;

    public BoxCollider BoxCollider { get; private set; }
    public bool IsDropped { get; set; }
    public bool IsLuckIngredient { get; set; }

    private Tween _tween;

    private void Start()
    {
        BoxCollider = GetComponent<BoxCollider>();

        _tween = transform.DORotate(new Vector3(90, 360, 0), _rotateSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    public void StopAnimation()
    {
        TextMeshPro meshRenderer = GetComponentInChildren<TextMeshPro>();
        ParticleSystem particleSystem = GetComponentInChildren<ParticleSystem>();

        if(meshRenderer != null)
        {
            Destroy(meshRenderer.gameObject);
        }
        if(particleSystem != null)
        {
            Destroy(particleSystem.gameObject);
        }

        _tween.Kill();
    }

    private void OnDestroy()
    {
        _tween.Kill();
    }
}