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
    private bool animationStoped;

    private void Start()
    {
        BoxCollider = GetComponent<BoxCollider>();

        if(!animationStoped)
        {
            var startEuler = transform.eulerAngles;
            _tween = transform.DORotate(
                new Vector3(startEuler.x, startEuler.y + 360f, startEuler.z),
                _rotateSpeed,
                RotateMode.FastBeyond360)
                .SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
        }
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
        animationStoped = true;
    }

    private void OnDestroy()
    {
        _tween.Kill();
    }
}