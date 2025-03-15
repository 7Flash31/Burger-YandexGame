using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Help Pointer")]
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private Transform _helpPointer;
    [SerializeField] private Transform _pointerPos1;
    [SerializeField] private Transform _pointerPos2;
    [SerializeField] private float _pointerSpeed;

    [Header("MusicSlider")]
    [SerializeField] private Slider _musicSlider;

    private Tween _moveTween;

    private void Start()
    {
        _helpPointer.position = _pointerPos1.position;

        float distance = Vector3.Distance(_pointerPos1.position, _pointerPos2.position);
        float duration = distance / _pointerSpeed;

        _moveTween = _helpPointer.DOMove(_pointerPos2.position, duration).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }

    private void StopAnimation()
    {
        if(_moveTween != null && _moveTween.IsActive())
        {
            _moveTween.Kill();
        }
    }

    public void HideHelpPanel()
    {
        _startPanel.SetActive(false);
        StopAnimation();
    }

    public void ShowMusicSlider() => _musicSlider.gameObject.SetActive(!_musicSlider.gameObject.activeSelf);

    public void SetGameMusic() => GameManager.Instance.GameMusic = _musicSlider.value;
}
