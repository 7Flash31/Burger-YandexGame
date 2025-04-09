using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class HeadController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private GameObject _foodPosition;
    [SerializeField] private GameObject _fireParticle;
    [SerializeField] private Transform _playerContainer;
    [SerializeField] private MeshRenderer _headMesh;

    [Header("Animation Options")]
    [SerializeField] private HeadAnimationOptions _headAnimations;
    [SerializeField] private CameraAnimationOptions _cameraAnimations;

    [Header("Settings")]
    [SerializeField] private float _headRotateSpeed;
    [SerializeField] private float _sequenceSpeed;

    private GameObject Head;
    private bool feetHeadTriggered;

    private void Start()
    {
        Head = gameObject;
        _playerCamera = Camera.main.gameObject;
        var headTransform = Head.transform;
        var idleAnim = _headAnimations.Idle;

        headTransform.DOMove(idleAnim.position, _headRotateSpeed);
        headTransform.DORotate(idleAnim.eulerAngles, _headRotateSpeed);
    }

    public void PlayEatAnimation()
    {
        _playerCamera.transform.SetParent(null);

        var headTransform = Head.transform;
        var cameraTransform = _playerCamera.transform;
        var eatHead = _headAnimations.Eat;
        var eatCamera = _cameraAnimations.Eat;

        DG.Tweening.Sequence sequence = DOTween.Sequence();
        sequence.Append(headTransform.DOMove(eatHead.position, _sequenceSpeed))
                .Join(headTransform.DORotate(eatHead.eulerAngles, _sequenceSpeed))
                .Append(cameraTransform.DOMove(eatCamera.position, _sequenceSpeed))
                .Join(cameraTransform.DORotate(eatCamera.eulerAngles, _sequenceSpeed))
                .OnComplete(() => FeetHead());
    }

    public void PlayAngryAnimation()
    {
        var cameraTransform = _playerCamera.transform;
        var headTransform = Head.transform;
        var angryCamera = _cameraAnimations.Angry;
        var angryHead = _headAnimations.Angry;

        DG.Tweening.Sequence sequence = DOTween.Sequence();
        sequence.Append(cameraTransform.DOMove(angryCamera.position, _sequenceSpeed))
                .Join(cameraTransform.DORotate(angryCamera.eulerAngles, _sequenceSpeed))
                .Join(headTransform.DOMove(angryHead.position, _sequenceSpeed))
                .Join(headTransform.DORotate(angryHead.eulerAngles, _sequenceSpeed))
                .OnComplete(() => RotateHead());

        _headMesh.material.DOColor(Color.red, _headRotateSpeed);
    }

    private void RotateHead()
    {
        _fireParticle.SetActive(true);

        var headTransform = Head.transform;
        headTransform.DORotate(_headAnimations.RotatePos1.eulerAngles, _headRotateSpeed)
                      .OnComplete(() =>
                      {
                          headTransform.DORotate(_headAnimations.RotatePos2.eulerAngles, _headRotateSpeed)
                                       .SetEase(Ease.Linear)
                                       .SetLoops(-1, LoopType.Yoyo);
                      });
    }

    private void FeetHead()
    {
        if(feetHeadTriggered)
            return;

        feetHeadTriggered = true;
        _playerContainer.DOMove(_foodPosition.transform.position, _sequenceSpeed);

        Rigidbody rigidbody = _playerContainer.AddComponent<Rigidbody>();
        rigidbody.drag = 4;
        rigidbody.isKinematic = false;

        GameManager.Instance.Player.BurgerTop.GetComponent<Rigidbody>().isKinematic = false;
        GameManager.Instance.Player.DeleteJoint(_playerContainer);
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
