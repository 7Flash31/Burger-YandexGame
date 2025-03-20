using DG.Tweening;
using UnityEngine;

public class TrapObstacle : Trap
{
    private void Start()
    {
        transform.DORotate(new Vector3(transform.rotation.x, 360, transform.rotation.z), rotateSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }
}
