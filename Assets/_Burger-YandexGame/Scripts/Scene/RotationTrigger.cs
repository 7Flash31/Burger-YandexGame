using DG.Tweening;
using UnityEngine;

public class RotationTrigger : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private GameObject _newRoad;
    [SerializeField] private Vector3 _toRotate;

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();

        player.Vertical /= 2;

        player.transform.DORotate(_toRotate, _speed).OnComplete(() =>
        {
            player.Vertical = 1;
        });
    }
}
