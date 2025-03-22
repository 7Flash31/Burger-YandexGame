using System.Collections;
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

    [SerializeField] private float _maxRockingAngle = 15f;
    [SerializeField] private float _rockingSmooth = 5f;

    private List<Ingredient> _ingredients = new List<Ingredient>();
    private Rigidbody _rb;
    private BoxCollider _burgerDownCollider;

    private float _horizontal;

    private float _currentRockingAngle = 0f;

    private bool _hasTriggered = false;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _burgerDownCollider = BurgerDown.GetComponent<BoxCollider>();
    }

    private void Update()
    {
        _horizontal = Input.GetAxis("Horizontal");

        if(Input.GetMouseButton(0))
        {
            _horizontal = Input.GetAxis("Mouse X") * _sensitivityMouse;
        }

        if(Input.touchCount > 0)
        {
            foreach(Touch touch in Input.touches)
            {
                if(touch.phase == TouchPhase.Moved)
                {
                    _horizontal = touch.deltaPosition.x * _sensitivityTouch;
                }
            }
        }

        float targetAngle = -_horizontal * _maxRockingAngle;
        _currentRockingAngle = Mathf.Lerp(_currentRockingAngle, targetAngle, Time.deltaTime * _rockingSmooth);
        _burgerComponents.localRotation = Quaternion.Euler(0, 0, _currentRockingAngle);
    }

    private void FixedUpdate()
    {
        float vertical = 1f;
        Vector3 movement = new Vector3(_horizontal, 0f, vertical);

        Vector3 newPosition = _rb.position + movement * _speed * Time.fixedDeltaTime;
        _rb.MovePosition(newPosition);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.transform.TryGetComponent(out Ingredient interactableItem))
        {
            InteractWithItem(interactableItem);
        }

        if(_hasTriggered)
            return;

        if(collider.gameObject.TryGetComponent(out Trap trap))
        {
            DeleteRandomIngredient(trap.RemoveIngredient);
            _hasTriggered = true;
        }

    }

    private void InteractWithItem(Ingredient ingredient)
    {
        if(_ingredients.Contains(ingredient))
            return;

        if(ingredient.IsDropped)
            return;

        ingredient.transform.SetParent(_burgerComponents);
        ingredient.transform.localPosition = Vector3.zero;
        ingredient.transform.localEulerAngles = Vector3.zero;
        
        ingredient.transform.localScale = Vector3.one;
        ingredient.StopAnimation();

        _ingredients.Add(ingredient);

        SetIngredientPosition();
    }

    private void SetIngredientPosition()
    {
        float currentY = _burgerDownCollider.size.y;

        foreach(var item in _ingredients)
        {
            if(item.BoxCollider != null)
            {
                float bottomOffset = item.BoxCollider.size.y / 2f - item.BoxCollider.center.y;

                item.transform.localPosition = new Vector3(0, currentY + bottomOffset, 0);

                float topOffset = item.BoxCollider.center.y + item.BoxCollider.size.y / 2f;

                currentY = item.transform.localPosition.y + topOffset;
            }
        }

        BurgerTop.localPosition = new Vector3(0, currentY, 0);
    }

    //public void DeleteIngredient(Ingredient ingredient)
    //{
    //    if(ingredient != null && _ingredients.Contains(ingredient))
    //    {
    //        _ingredients.Remove(ingredient);

    //        if(_ingredients.Count == 0)
    //        {
    //            //GameManager.Instance.FinalGame();
    //        }
    //    }
    //}

    public void DeleteRandomIngredient(int count)
    {
        int a = Mathf.Min(_ingredients.Count, count);

        for(int i = a - 1; i >= 0; i--)
        {
            if(_ingredients[i] != null)
            {
                _ingredients[i].IsDropped = true;
                _ingredients[i].transform.SetParent(null);

                Destroy(_ingredients[i], 0.5f);

                _ingredients.RemoveAt(i);
            }
        }

        StartCoroutine(SetHasTrigger());

        SetIngredientPosition();
    }

    private void OnDisable()
    {
        GameManager.Instance.FinalIngredients.Clear();
        GameManager.Instance.FinalIngredients.AddRange(_ingredients);
    }

    private IEnumerator SetHasTrigger()
    {
        yield return new WaitForSeconds(0.1f);
        _hasTriggered = false;
    }
}
