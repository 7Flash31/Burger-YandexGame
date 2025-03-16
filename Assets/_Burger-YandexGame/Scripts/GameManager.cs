//using DG.Tweening;
//using System.Collections.Generic;
//using UnityEngine;

//public class GameManager : MonoBehaviour
//{
//    [field: SerializeField] public Player Player { get; private set; }
//    [field: SerializeField] public UIController UIController { get; private set; }
//    [SerializeField] private HeadSettings _headSettings;

//    public float GameMusic { get; set; }
//    public bool GameLaunch { get; private set; }
//    public static GameManager Instance { get; private set; }
//    public List<Ingredient> FinalIngredients { get; set; } = new List<Ingredient>();

//    private bool feetHeadTriggered;

//    private void Awake()
//    {
//        DOTween.Init();
//        Player.enabled = false;

//        var headTransform = _headSettings.Head.transform;
//        var idleAnim = _headSettings.HeadAnimations.Idle;

//        headTransform.DOMove(idleAnim.position, _headSettings.HeadRotateSpeed);
//        headTransform.DORotate(idleAnim.eulerAngles, _headSettings.HeadRotateSpeed);

//        if(Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    public void LaunchGame()
//    {
//        GameLaunch = true;
//        Player.enabled = true;
//        UIController.HideHelpPanel();
//    }

//    public void StopGame()
//    {
//        Player.enabled = false;
//        _headSettings.PlayerCamera.transform.SetParent(null);

//        var headTransform = _headSettings.Head.transform;
//        var cameraTransform = _headSettings.PlayerCamera.transform;
//        var eatHead = _headSettings.HeadAnimations.Eat;
//        var eatCamera = _headSettings.CameraAnimations.Eat;

//        Sequence sequence = DOTween.Sequence();

//        sequence.Append(headTransform.DOMove(eatHead.position, _headSettings.SequenceSpeed))
//                .Join(headTransform.DORotate(eatHead.eulerAngles, _headSettings.SequenceSpeed))
//                .Append(cameraTransform.DOMove(eatCamera.position, _headSettings.SequenceSpeed))
//                .Join(cameraTransform.DORotate(eatCamera.eulerAngles, _headSettings.SequenceSpeed))
//                .OnComplete(() => FeetHead());
//    }

//    private void FeetHead()
//    {
//        if(feetHeadTriggered)
//            return;

//        feetHeadTriggered = true;

//        Player.transform.DOMove(_headSettings.FoodPosition.transform.position, _headSettings.SequenceSpeed);
//        Player.GetComponent<Rigidbody>().drag = 4;
//    }

//    public void LookHead()
//    {
//        var cameraTransform = _headSettings.PlayerCamera.transform;
//        var headTransform = _headSettings.Head.transform;
//        var angryCamera = _headSettings.CameraAnimations.Angry;
//        var angryHead = _headSettings.HeadAnimations.Angry;

//        Sequence sequence = DOTween.Sequence();

//        sequence.Append(cameraTransform.DOMove(angryCamera.position, _headSettings.SequenceSpeed))
//                .Join(cameraTransform.DORotate(angryCamera.eulerAngles, _headSettings.SequenceSpeed))
//                .Join(headTransform.DOMove(angryHead.position, _headSettings.SequenceSpeed))
//                .Join(headTransform.DORotate(angryHead.eulerAngles, _headSettings.SequenceSpeed))
//                .OnComplete(() => RotateHead());

//        _headSettings.HeadMesh.material.DOColor(Color.red, _headSettings.HeadRotateSpeed);
//    }

//    private void RotateHead()
//    {
//        _headSettings.FireParticle.SetActive(true);

//        _headSettings.Head.transform
//            .DORotate(_headSettings.HeadAnimations.RotatePos1.eulerAngles, _headSettings.HeadRotateSpeed)
//            .OnComplete(() =>
//            {
//                _headSettings.Head.transform
//                    .DORotate(_headSettings.HeadAnimations.RotatePos2.eulerAngles, _headSettings.HeadRotateSpeed)
//                    .SetEase(Ease.Linear)
//                    .SetLoops(-1, LoopType.Yoyo);
//            });
//    }
//}


using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public Player Player { get; private set; }
    [field: SerializeField] public UIController UIController { get; private set; }

    [SerializeField] private HeadController _headController;

    public float GameMusic { get; set; }
    public bool GameLaunch { get; private set; }
    public static GameManager Instance { get; private set; }
    public List<Ingredient> FinalIngredients { get; set; } = new List<Ingredient>();


    private void Awake()
    {
        DOTween.Init();
        Player.enabled = false;


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
        _headController.PlayEatAnimation();
    }

    public void LookHead()
    {
        _headController.PlayAngryAnimation();
    }
}

public enum IngredientType
{
    Meat,
    Pepper,
    Cheese,
    Tomato,
    Fish,
    Avocado,
    Egg,
    Bacon,
    Cutlet,
    Salad,
}