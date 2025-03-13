using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField] private float _speed;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        //float vertical = Input.GetAxis("Vertical");
        float vertical = 1;

        Vector3 movement = new Vector3(horizontal, 0f, vertical);

        Vector3 newPosition = _rb.position + movement * _speed * Time.fixedDeltaTime;

        _rb.MovePosition(newPosition);
    }
}
