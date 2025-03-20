using UnityEngine;

public class Springboard : MonoBehaviour
{
    [SerializeField] private int _upForce;
    [SerializeField] private int _forwardForce;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.TryGetComponent(out Player player))
        {
            if(player.gameObject.TryGetComponent(out Rigidbody rb))
            {
                rb.AddForce(new Vector3(0, _upForce, _forwardForce));
            }
        }
    }
}
