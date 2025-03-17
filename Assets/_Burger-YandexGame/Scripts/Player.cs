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

    private List<Ingredient> _ingredients = new List<Ingredient>();
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

    //private void InteractWithItem(Ingredient ingredient)
    //{
    //    if(_ingredients.Contains(ingredient))
    //        return;

    //    Vector3 newPos = CalculateItemPosition();

    //    ingredient.transform.SetParent(_burgerComponents);

    //    ingredient.transform.localPosition = Vector3.zero;
    //    ingredient.transform.localEulerAngles = Vector3.zero;
    //    ingredient.transform.localScale = Vector3.one;

    //    ingredient.StopAnimation();

    //    ingredient.transform.localPosition = newPos;

    //    BurgerTop.transform.localPosition = newPos;
    //    _ingredients.Add(ingredient);

    //}

    //private void InteractWithItem(Ingredient ingredient)
    //{
    //    if(_ingredients.Contains(ingredient))
    //        return;

    //    Vector3 newPos = CalculateItemPosition();

    //    ingredient.transform.SetParent(_burgerComponents);

    //    ingredient.transform.localPosition = Vector3.zero;
    //    ingredient.transform.localEulerAngles = Vector3.zero;
    //    ingredient.transform.localScale = Vector3.one;

    //    ingredient.StopAnimation();

    //    ingredient.transform.localPosition = newPos;

    //    BurgerTop.transform.localPosition = newPos;
    //    _ingredients.Add(ingredient);
    //}

    //private Vector3 CalculateItemPosition()
    //{
    //    float yPos = _burgerDownCollider.size.y;

    //    foreach(var item in _ingredients)
    //    {
    //        if(item.BoxCollider != null)
    //        {
    //            yPos += item.BoxCollider.size.y;
    //        }
    //    }
    //    return new Vector3(0, yPos, 0);
    //}

    private void InteractWithItem(Ingredient ingredient)
    {
        if(_ingredients.Contains(ingredient))
            return;

        // Вычисляем позицию для нового ингредиента
        Vector3 newPos = CalculateItemPosition();

        ingredient.transform.SetParent(_burgerComponents);
        ingredient.transform.localPosition = Vector3.zero;
        ingredient.transform.localEulerAngles = Vector3.zero;
        ingredient.transform.localScale = Vector3.one;
        ingredient.StopAnimation();

        // Располагаем ингредиент в стеке
        ingredient.transform.localPosition = newPos;

        // Добавляем ингредиент в список, чтобы он учитывался при расчётах будущих позиций
        _ingredients.Add(ingredient);

        // Если у ингредиента есть BoxCollider, используем его высоту,
        // иначе высота считается равной нулю
        float ingredientHeight = ingredient.BoxCollider != null ? ingredient.BoxCollider.size.y : 0;

        // Устанавливаем позицию BurgerTop с учётом высоты нового ингредиента,
        // чтобы он располагался поверх стека
        BurgerTop.transform.localPosition = new Vector3(0, newPos.y + ingredientHeight, 0);
    }

    private Vector3 CalculateItemPosition()
    {
        float yPos = _burgerDownCollider.size.y;

        foreach(var item in _ingredients)
        {
            if(item.BoxCollider != null)
            {
                yPos += item.BoxCollider.size.y;
            }
        }
        return new Vector3(0, yPos, 0);
    }


    public void DeleteIngredient(Ingredient ingredient)
    {
        if(ingredient != null && _ingredients.Contains(ingredient))
        {
            // Проверка на Top Burger, не удалять из _ingredients

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
