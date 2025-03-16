using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [field: SerializeField] public Transform BurgerTop { get; private set; }
    [field: SerializeField] public Transform BurgerDown { get; private set; }
    [SerializeField] private Transform _burgerComponents;

    [SerializeField] private float _speed;
    [SerializeField] private float _sensitivityMouse;
    [SerializeField] private float _sensitivityTouch;

    public List<Ingredient> _ingredients = new List<Ingredient>();
    private Rigidbody _rb;
    private BoxCollider _burgerDownCollider;

    private float horizontal;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _burgerDownCollider = BurgerDown.GetComponent<BoxCollider>();
    }

    private void Update()
    {
        horizontal = Input.GetAxis("Horizontal");

        if(Input.GetMouseButton(0))
        {
            horizontal = Input.GetAxis("Mouse X") * _sensitivityMouse;
        }

        if(Input.touchCount > 0)
        {
            foreach(Touch touch in Input.touches)
            {
                if(touch.phase == TouchPhase.Moved)
                {
                    horizontal = touch.deltaPosition.x * _sensitivityTouch;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        float vertical = 1f;

        Vector3 movement = new Vector3(horizontal , 0f, vertical);
        Vector3 newPosition = _rb.position + movement * _speed * Time.fixedDeltaTime;

        _rb.MovePosition(newPosition);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.transform.TryGetComponent(out Ingredient interactableItem))
        {
            InteractWithItem(interactableItem);
        }
    }

    private void InteractWithItem(Ingredient interactableItem)
    {
        if(_ingredients.Contains(interactableItem))
            return;

        interactableItem.transform.SetParent(_burgerComponents);
        interactableItem.transform.localPosition = Vector3.zero;

        Vector3 newPos = CalculateItemPosition();

        interactableItem.transform.localPosition = newPos;
        BurgerTop.transform.localPosition = newPos;

        _ingredients.Add(interactableItem);
    }

    private Vector3 CalculateItemPosition()
    {
        float yPos = _burgerDownCollider.size.y;

        foreach(var item in _ingredients)
        {
            BoxCollider itemCollider = item.GetComponent<BoxCollider>();
            if(itemCollider != null)
            {
                yPos += itemCollider.size.y;
            }
        }
        return new Vector3(0, yPos, 0);
    }

    public void DeleteIngredient(Ingredient ingredient)
    {
        if(ingredient != null && _ingredients.Contains(ingredient))
        {
            _ingredients.Remove(ingredient);

            if(_ingredients.Count == 0)
            {
                GameManager.Instance.FinalGame();

            }
        }
    }

    private void OnDisable()
    {
        GameManager.Instance.FinalIngredients.Clear();
        GameManager.Instance.FinalIngredients.AddRange(_ingredients);
    }
}
