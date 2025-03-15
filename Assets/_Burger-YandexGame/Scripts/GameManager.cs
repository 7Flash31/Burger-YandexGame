using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public Player Player { get; private set; }
    [field: SerializeField] public UIController UIController { get; private set; }

    [field: Header("Head")]
    [SerializeField] private GameObject _head;
    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private GameObject _feedPosition;
    [SerializeField] private Transform[] _cameraPositions;
    [SerializeField] private Transform[] _headPositions;
    [SerializeField] private float _headRotateSpeed;
    [SerializeField] private float _sequenceSpeed;

    public float GameMusic { get;  set; }
    public bool GameLaunch { get; private set; }

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        DOTween.Init();
        Player.enabled = false;

        Vector3 newPos = new Vector3(0, 3, 64);
        Vector3 newRot = new Vector3(-30, 0, 0);

        _head.transform.DOMove(newPos, _headRotateSpeed);
        _head.transform.DORotate(newRot, _headRotateSpeed);

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
        _playerCamera.transform.SetParent(null);

        Sequence sequence = DOTween.Sequence();

        sequence.Append(_head.transform.DOMove(_headPositions[0].transform.position, _sequenceSpeed));
        sequence.Join(_head.transform.DORotate(_headPositions[0].transform.eulerAngles, _sequenceSpeed));

        sequence.Append(_playerCamera.transform.DOMove(_cameraPositions[0].transform.position, _sequenceSpeed));
        sequence.Join(_playerCamera.transform.DORotate(_cameraPositions[0].transform.eulerAngles, _sequenceSpeed));

        sequence.OnComplete(() =>
        {
            FeetHead();
        });       
    }

    private void FeetHead()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(Player.transform.DOMove(_feedPosition.transform.position, _sequenceSpeed));

        Player.gameObject.GetComponent<Rigidbody>().drag = 4;
    }

    private void LookHead()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(_head.transform.DOMove(_headPositions[1].transform.position, _sequenceSpeed));
        sequence.Join(_head.transform.DORotate(_headPositions[1].transform.eulerAngles, _sequenceSpeed));

        sequence.Append(_playerCamera.transform.DOMove(_cameraPositions[1].transform.position, _sequenceSpeed));
        sequence.Join(_playerCamera.transform.DORotate(_cameraPositions[1].transform.eulerAngles, _sequenceSpeed));
    }
}