using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public Player Player { get; private set; }
    [field: SerializeField] public UIController UIController { get; private set; }

    [field: Header("Head")]
    [SerializeField] private HeadSettings headSettings;

    public float GameMusic { get;  set; }
    public bool GameLaunch { get; private set; }

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        DOTween.Init();
        Player.enabled = false;

        var headTransform = headSettings.Head.transform;
        var idleAnim = headSettings.HeadAnimations.Idle;

        headTransform.DOMove(idleAnim.position, headSettings.HeadRotateSpeed);
        headTransform.DORotate(idleAnim.eulerAngles, headSettings.HeadRotateSpeed);

        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LaunchGame()
    {
        GameLaunch = true;
        Player.enabled = true;
        UIController.HideHelpPanel();
    }

    public void StopGame()
    {
        Player.enabled = false;
        headSettings.PlayerCamera.transform.SetParent(null);

        var headTransform = headSettings.Head.transform;
        var cameraTransform = headSettings.PlayerCamera.transform;
        var eatHead = headSettings.HeadAnimations.Eat;
        var eatCamera = headSettings.CameraAnimations.Eat;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(headTransform.DOMove(eatHead.position, headSettings.SequenceSpeed))
                .Join(headTransform.DORotate(eatHead.eulerAngles, headSettings.SequenceSpeed))
                .Append(cameraTransform.DOMove(eatCamera.position, headSettings.SequenceSpeed))
                .Join(cameraTransform.DORotate(eatCamera.eulerAngles, headSettings.SequenceSpeed))
                .OnComplete(() => FeetHead());
    }

    private bool feetHeadTriggered;

    private void FeetHead()
    {
        if(feetHeadTriggered) 
            return;
        feetHeadTriggered = true;

        Player.transform.DOMove(headSettings.FoodPosition.transform.position, headSettings.SequenceSpeed);
        Player.GetComponent<Rigidbody>().drag = 4;
    }

    public void LookHead()
    {
        var cameraTransform = headSettings.PlayerCamera.transform;
        var headTransform = headSettings.Head.transform;
        var angryCamera = headSettings.CameraAnimations.Angry;
        var angryHead = headSettings.HeadAnimations.Angry;

        Sequence sequence = DOTween.Sequence();
        Debug.Log("Angry");
        sequence.Append(cameraTransform.DOMove(angryCamera.position, headSettings.SequenceSpeed))
                .Join(cameraTransform.DORotate(angryCamera.eulerAngles, headSettings.SequenceSpeed))
                .Join(headTransform.DOMove(angryHead.position, headSettings.SequenceSpeed))
                .Join(headTransform.DORotate(angryHead.eulerAngles, headSettings.SequenceSpeed))
                .OnComplete(() => RotateHead());
    }

    private void RotateHead()
    {
        headSettings.Head.transform
            .DORotate(headSettings.HeadAnimations.RotatePos1.eulerAngles, headSettings.HeadRotateSpeed)
            .OnComplete(() =>
            {
                headSettings.Head.transform
                    .DORotate(headSettings.HeadAnimations.RotatePos2.eulerAngles, headSettings.HeadRotateSpeed)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo);
            });
    }
}

[System.Serializable]
public class HeadSettings
{
    [Header("Reference")]
    public GameObject Head;
    public GameObject PlayerCamera;
    public GameObject FoodPosition;

    [Header("Animation Options")]
    public HeadAnimationOptions HeadAnimations;
    public CameraAnimationOptions CameraAnimations;

    [Header("Settings")]
    public float HeadRotateSpeed;
    public float SequenceSpeed;

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
}