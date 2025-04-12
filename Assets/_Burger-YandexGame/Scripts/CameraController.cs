using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Vector3 _offset;

    private void Start()
    {
        _offset = transform.localPosition;
    }

    void LateUpdate()
    {
        if(GameManager.Instance.Player.CanMove)
        {
            Vector3 targetPosition = _player.position + _offset;

            transform.position = targetPosition;
        }
    }
}
