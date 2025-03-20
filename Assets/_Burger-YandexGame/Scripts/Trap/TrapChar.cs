using DG.Tweening;
using UnityEngine;

public class TrapChar : Trap
{
    [SerializeField] private Vector3 startAnimation;

    private void Start()
    {
        transform.eulerAngles = startAnimation;

        transform.DORotate(new Vector3(-50, startAnimation.y, startAnimation.z), rotateSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
