using UnityEngine;

public class Rocking : MonoBehaviour
{
    [Tooltip("Максимальное отклонение в градусах")]
    public float amplitude = 10f;

    [Tooltip("Скорость покачивания")]
    public float speed = 1f;

    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        float angle = amplitude * Mathf.Sin(Time.time * speed);
        transform.localRotation = initialRotation * Quaternion.Euler(0f, 0f, angle);
    }
}
