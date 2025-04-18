using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float rotationSmoothSpeed = 5f;
    [SerializeField] private float returnDuration = 0.8f;  // время возврата в секундах

    private Quaternion _rotationOffset;
    private bool _isTweening = false;
    private Coroutine _returnCoroutine;

    private void Start()
    {
        _offset = transform.localPosition;
        _rotationOffset = transform.rotation;
    }

    private void LateUpdate()
    {
        if(GameManager.Instance.Player.CanMove && !_isTweening)
        {
            // простой follow (каждый кадр)
            Vector3 targetPos = _player.position + _player.rotation * _offset;
            transform.position = targetPos;

            Quaternion targetRot = _player.rotation * _rotationOffset;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * rotationSmoothSpeed
            );
        }
    }

    public void OnTriggreRotate(Transform player, bool isComplete)
    {
        // если началась «кинематографическая» часть
        if(!isComplete)
        {
            // отменяем возможный return‑корутин
            if(_returnCoroutine != null)
            {
                StopCoroutine(_returnCoroutine);
                _returnCoroutine = null;
            }

            _isTweening = true;
            Quaternion targetRot = _player.rotation * _rotationOffset;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * rotationSmoothSpeed
            );
            return;
        }

        // кинематографическая часть закончилась — запускаем плавный возврат
        if(_returnCoroutine != null)
            StopCoroutine(_returnCoroutine);

        _returnCoroutine = StartCoroutine(SmoothReturnToFollow());
    }

    private IEnumerator SmoothReturnToFollow()
    {
        _isTweening = true;

        // запоминаем стартовые трансформы
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while(elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / returnDuration);

            // каждый кадр пересчитываем «правильную» позицию/ротацию follow‑режима
            Vector3 dynamicPos = _player.position + _player.rotation * _offset;
            Quaternion dynamicRot = _player.rotation * _rotationOffset;

            transform.position = Vector3.Lerp(startPos, dynamicPos, t);
            transform.rotation = Quaternion.Slerp(startRot, dynamicRot, t);

            yield return null;
        }

        // на всякий случай ставим точно в цель
        transform.position = _player.position + _player.rotation * _offset;
        transform.rotation = _player.rotation * _rotationOffset;

        _isTweening = false;
        _returnCoroutine = null;
    }
}
