//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
//public class Player : MonoBehaviour
//{
//    [field: SerializeField] public Transform BurgerTop { get; private set; }
//    [field: SerializeField] public Transform BurgerDown { get; private set; }
//    [SerializeField] private Transform _burgerComponents;

//    [SerializeField] private float _speed;
//    [SerializeField] private float _sensitivityMouse;
//    [SerializeField] private float _sensitivityTouch;

//    private List<Ingredient> _ingredients = new List<Ingredient>();
//    private Rigidbody _rb;
//    private BoxCollider _burgerDownCollider;

//    private float horizontal;

//    private void Start()
//    {
//        _rb = GetComponent<Rigidbody>();
//        _burgerDownCollider = BurgerDown.GetComponent<BoxCollider>();
//    }

//    private void Update()
//    {
//        horizontal = Input.GetAxis("Horizontal");

//        if(Input.GetMouseButton(0))
//        {
//            horizontal = Input.GetAxis("Mouse X") * _sensitivityMouse;
//        }

//        if(Input.touchCount > 0)
//        {
//            foreach(Touch touch in Input.touches)
//            {
//                if(touch.phase == TouchPhase.Moved)
//                {
//                    horizontal = touch.deltaPosition.x * _sensitivityTouch;
//                }
//            }
//        }

//        // Вычисление целевого угла покачивания
//        float targetAngle = -horizontal * maxRockingAngle;
//        // Плавный переход к целевому углу
//        currentRockingAngle = Mathf.Lerp(currentRockingAngle, targetAngle, Time.deltaTime * rockingSmooth);
//        // Применяем поворот к родительскому объекту, содержащему ингредиенты
//        _burgerComponents.localRotation = Quaternion.Euler(0, 0, currentRockingAngle);
//    }

//    private void FixedUpdate()
//    {
//        float vertical = 1f;

//        Vector3 movement = new Vector3(horizontal , 0f, vertical);
//        Vector3 newPosition = _rb.position + movement * _speed * Time.fixedDeltaTime;

//        _rb.MovePosition(newPosition);
//    }

//    private void OnTriggerEnter(Collider collider)
//    {
//        if(collider.transform.TryGetComponent(out Ingredient interactableItem))
//        {
//            InteractWithItem(interactableItem);
//        }
//    }

//    private void InteractWithItem(Ingredient ingredient)
//    {
//        if(_ingredients.Contains(ingredient))
//            return;

//        Vector3 newPos = CalculateItemPosition();

//        ingredient.transform.SetParent(_burgerComponents);
//        ingredient.transform.localPosition = Vector3.zero;
//        ingredient.transform.localEulerAngles = Vector3.zero;
//        ingredient.transform.localScale = Vector3.one;
//        ingredient.StopAnimation();

//        ingredient.transform.localPosition = newPos;

//        _ingredients.Add(ingredient);

//        float ingredientHeight = ingredient.BoxCollider != null ? ingredient.BoxCollider.size.y : 0;

//        BurgerTop.transform.localPosition = new Vector3(0, newPos.y + ingredientHeight, 0);
//    }

//    private Vector3 CalculateItemPosition()
//    {
//        float yPos = _burgerDownCollider.size.y;

//        foreach(var item in _ingredients)
//        {
//            if(item.BoxCollider != null)
//            {
//                yPos += item.BoxCollider.size.y;
//            }
//        }
//        return new Vector3(0, yPos, 0);
//    }


//    public void DeleteIngredient(Ingredient ingredient)
//    {
//        if(ingredient != null && _ingredients.Contains(ingredient))
//        {
//            _ingredients.Remove(ingredient);


//            if(_ingredients.Count == 0)
//            {
//                GameManager.Instance.FinalGame();
//            }
//        }
//    }

//    private void OnDisable()
//    {
//        GameManager.Instance.FinalIngredients.Clear();
//        GameManager.Instance.FinalIngredients.AddRange(_ingredients);
//    }
//}

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

    // Добавьте эти переменные для покачивания
    [SerializeField] private float maxRockingAngle = 15f; // Максимальный угол наклона влево/вправо
    [SerializeField] private float rockingSmooth = 5f;    // Скорость «возврата» к целевому углу
    private float currentRockingAngle = 0f;               // Текущее значение покачивания

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
        // Получаем ввод по оси Horizontal
        horizontal = Input.GetAxis("Horizontal");

        // Управление мышью
        if(Input.GetMouseButton(0))
        {
            horizontal = Input.GetAxis("Mouse X") * _sensitivityMouse;
        }

        // Управление касаниями (на мобильных)
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

        // 1. Вычисляем целевой угол покачивания (от -maxRockingAngle до +maxRockingAngle)
        float targetAngle = -horizontal * maxRockingAngle;

        // 2. Плавно интерполируем текущий угол к целевому
        currentRockingAngle = Mathf.Lerp(currentRockingAngle, targetAngle, Time.deltaTime * rockingSmooth);

        // 3. Применяем вращение к родительскому объекту, содержащему ингредиенты
        _burgerComponents.localRotation = Quaternion.Euler(0, 0, currentRockingAngle);
    }

    private void FixedUpdate()
    {
        float vertical = 1f; // Постоянно двигаемся вперёд (как в вашем коде)
        Vector3 movement = new Vector3(horizontal, 0f, vertical);

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

    private void InteractWithItem(Ingredient ingredient)
    {
        if(_ingredients.Contains(ingredient))
            return;

        // Рассчитываем позицию, куда кладём следующий ингредиент
        Vector3 newPos = CalculateItemPosition();

        ingredient.transform.SetParent(_burgerComponents);
        ingredient.transform.localPosition = Vector3.zero;
        ingredient.transform.localEulerAngles = Vector3.zero;
        ingredient.transform.localScale = Vector3.one;
        ingredient.StopAnimation();

        ingredient.transform.localPosition = newPos;

        _ingredients.Add(ingredient);

        float ingredientHeight = ingredient.BoxCollider != null ? ingredient.BoxCollider.size.y : 0;

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
