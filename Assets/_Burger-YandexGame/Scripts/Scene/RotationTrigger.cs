using DG.Tweening;
using UnityEngine;

public class RotationTrigger : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private GameObject _closeWall;
    [SerializeField] private Vector3 _toRotate;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player player))
        {
            player.Vertical = 0;

            player.transform.DORotate(_toRotate, _speed).OnComplete(() =>
            {
                player.Vertical = 1;
                _closeWall.SetActive(false);
            });
        }
    }

}
