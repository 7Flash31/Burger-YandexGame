using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField] private Transform _burgerComponents;
    [SerializeField] private Transform _burgerDown;
    [SerializeField] private Transform _burgerTop;
    [SerializeField] private float _speed;

    private Rigidbody _rb;
    private BoxCollider _burgerDownCollider;

    private List<InteractableItem> _interactables = new List<InteractableItem>();

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _burgerDownCollider = _burgerDown.GetComponent<BoxCollider>();
    }

    private void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = 1;

        Vector3 movement = new Vector3(horizontal, 0f, vertical);

        Vector3 newPosition = _rb.position + movement * _speed * Time.fixedDeltaTime;

        _rb.MovePosition(newPosition);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.transform.TryGetComponent(out InteractableItem interactableItem))
        {
            InteractWithItem(interactableItem);
        }
    }

    private void InteractWithItem(InteractableItem interactableItem)
    {
        if(_interactables.Contains(interactableItem))
            return;

        interactableItem.transform.SetParent(_burgerComponents);
        interactableItem.transform.localPosition = Vector3.zero;

        Vector3 newPos = CalculateItemPosition();
        interactableItem.transform.localPosition = newPos;

        _burgerTop.transform.localPosition = newPos;

        _interactables.Add(interactableItem);
    }

    private Vector3 CalculateItemPosition()
    {
        float yPos = _burgerDownCollider.size.y;

        foreach(var item in _interactables)
        {
            BoxCollider itemCollider = item.GetComponent<BoxCollider>();
            if(itemCollider != null)
            {
                yPos += itemCollider.size.y;
            }
        }
        return new Vector3(0, yPos, 0);
    }
}
