using DG.Tweening;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Vector3 _offset;

    [SerializeField] private float rotationTweenDuration = 0.5f;

    private Quaternion _rotationOffset = Quaternion.identity;

    private void Start()
    {
        _offset = transform.localPosition;
        _rotationOffset = transform.rotation;
    }

    void LateUpdate()
    {
        if(GameManager.Instance.Player.CanMove)
        {
            Vector3 targetPosition = _player.position + _player.rotation * _offset;
            transform.position = targetPosition;

            Quaternion targetRotation = _player.rotation * _rotationOffset;

            float rotationSmoothSpeed = 5f; // Можно настроить для более медленного или быстрого сглаживания
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
        }
    }
}
