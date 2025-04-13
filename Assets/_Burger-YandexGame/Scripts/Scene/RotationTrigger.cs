using DG.Tweening;
using UnityEngine;

public class RotationTrigger : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private GameObject _newRoadWall;
    [SerializeField] private GameObject _oldRoadWall;
    [SerializeField] private Vector3 _toRotate;

    private float _tempSensitivityMouse;
    private float _tempSensitivityKeyboard;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player player))
        {
            player.Vertical = 0.4f;

            _tempSensitivityMouse = player.SensitivityMouse;
            _tempSensitivityKeyboard = player.SensitivityKeyboard;

            player.SensitivityMouse = 0f;
            player.SensitivityKeyboard = 0f;

            player.transform.DORotate(_toRotate, _speed).OnComplete(() =>
            {
                _newRoadWall.SetActive(false);
                _oldRoadWall.SetActive(true);

                player.Vertical = 1;
                player.SensitivityMouse = _tempSensitivityMouse;
                player.SensitivityKeyboard = _tempSensitivityKeyboard;
            });
        }
    }

}
