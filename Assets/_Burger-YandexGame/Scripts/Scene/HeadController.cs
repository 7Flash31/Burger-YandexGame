using DG.Tweening;
using UnityEngine;

public class HeadController : MonoBehaviour
{
    [Header("Reference")]
    public GameObject PlayerCamera;
    public GameObject FoodPosition;
    public MeshRenderer HeadMesh;
    public GameObject FireParticle;

    [Header("Animation Options")]
    public HeadAnimationOptions HeadAnimations;
    public CameraAnimationOptions CameraAnimations;

    [Header("Settings")]
    public float HeadRotateSpeed;
    public float SequenceSpeed;

    private GameObject Head;
    private bool feetHeadTriggered;

    private void Start()
    {
        Head = gameObject;

        var headTransform = Head.transform;
        var idleAnim = HeadAnimations.Idle;

        headTransform.DOMove(idleAnim.position, HeadRotateSpeed);
        headTransform.DORotate(idleAnim.eulerAngles, HeadRotateSpeed);
    }

    public void PlayEatAnimation()
    {
        PlayerCamera.transform.SetParent(null);

        var headTransform = Head.transform;
        var cameraTransform = PlayerCamera.transform;
        var eatHead = HeadAnimations.Eat;
        var eatCamera = CameraAnimations.Eat;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(headTransform.DOMove(eatHead.position, SequenceSpeed))
                .Join(headTransform.DORotate(eatHead.eulerAngles, SequenceSpeed))
                .Append(cameraTransform.DOMove(eatCamera.position, SequenceSpeed))
                .Join(cameraTransform.DORotate(eatCamera.eulerAngles, SequenceSpeed))
                .OnComplete(() => FeetHead());
    }

    public void PlayAngryAnimation()
    {
        var cameraTransform = PlayerCamera.transform;
        var headTransform = Head.transform;
        var angryCamera = CameraAnimations.Angry;
        var angryHead = HeadAnimations.Angry;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(cameraTransform.DOMove(angryCamera.position, SequenceSpeed))
                .Join(cameraTransform.DORotate(angryCamera.eulerAngles, SequenceSpeed))
                .Join(headTransform.DOMove(angryHead.position, SequenceSpeed))
                .Join(headTransform.DORotate(angryHead.eulerAngles, SequenceSpeed))
                .OnComplete(() => RotateHead());

        HeadMesh.material.DOColor(Color.red, HeadRotateSpeed);
    }

    private void RotateHead()
    {
        FireParticle.SetActive(true);

        var headTransform = Head.transform;
        headTransform.DORotate(HeadAnimations.RotatePos1.eulerAngles, HeadRotateSpeed)
                      .OnComplete(() =>
                      {
                          headTransform.DORotate(HeadAnimations.RotatePos2.eulerAngles, HeadRotateSpeed)
                                       .SetEase(Ease.Linear)
                                       .SetLoops(-1, LoopType.Yoyo);
                      });
    }

    private void FeetHead()
    {
        if(feetHeadTriggered)
            return;

        feetHeadTriggered = true;
        GameManager.Instance.Player.transform.DOMove(FoodPosition.transform.position, SequenceSpeed);
        GameManager.Instance.Player.GetComponent<Rigidbody>().drag = 4;
    }

}

[System.Serializable]
public class HeadAnimationOptions
{
    public Transform Idle;
    public Transform Eat;
    public Transform Angry;

    public Transform RotatePos1;
    public Transform RotatePos2;
}

[System.Serializable]
public class CameraAnimationOptions
{
    public Transform Eat;
    public Transform Angry;
}
