using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [field: SerializeField] public Transform BurgerTop { get; private set; }
    [field: SerializeField] public Transform BurgerDown { get; private set; }
                            
    [SerializeField] private Transform _burgerComponents;
    [SerializeField] private float _speed;
    [SerializeField] private float _sensitivityMouse;
    [SerializeField] private float _sensitivityTouch;

    private List<Ingredient> _ingredients = new List<Ingredient>();
    private BoxCollider _burgerDownCollider;
    private float _horizontal;

    public float Vertical { get; set; }

    private float _gravity = 9.81f;
    private bool _hasTriggered;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;
    private float verticalVelocity = 0f;

    private void Start()
    {
        _burgerDownCollider = BurgerDown.GetComponent<BoxCollider>();
        controller = GetComponent<CharacterController>();
        Vertical = 1f;
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

        if(controller.isGrounded)
        {
            verticalVelocity = 0f;
        }
        else
        {
            verticalVelocity -= _gravity * Time.deltaTime;
        }

        //if(StopPlayer)
        //{
        //    _horizontal = 0;
        //    Vertical = 0;
        //    _speed = 1;
        //}
            

        // Формирование итогового вектора движения
        moveDirection = new Vector3(_horizontal, verticalVelocity, Vertical);
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= _speed;

        controller.Move(moveDirection * Time.deltaTime);


        //float targetAngle = -_horizontal * _maxRockingAngle;
        //_currentRockingAngle = Mathf.Lerp(_currentRockingAngle, targetAngle, Time.deltaTime * _rockingSmooth);
        //_burgerComponents.localRotation = Quaternion.Euler(0, 0, _currentRockingAngle);
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

        //Spring

        //ingredient.gameObject.AddComponent<Rigidbody>();
        //ingredient.gameObject.AddComponent<ConfigurableJoint>();



        //for(int i = 0; i < _ingredients.Count; i++)
        //{
        //    ConfigurableJoint configurableJoint = null;
        //    if(i == 0)
        //    {
        //        configurableJoint = _ingredients[i].GetComponent<ConfigurableJoint>();
        //        configurableJoint.connectedBody = BurgerDown.GetComponent<Rigidbody>();
        //    }

        //    else
        //    {
        //        configurableJoint = _ingredients[i].GetComponent<ConfigurableJoint>();
        //        configurableJoint.connectedBody = _ingredients[i - 1].GetComponent<Rigidbody>();
        //    }

        //    configurableJoint.xMotion = ConfigurableJointMotion.Limited;
        //    configurableJoint.yMotion = ConfigurableJointMotion.Limited;
        //    configurableJoint.zMotion = ConfigurableJointMotion.Limited;

        //    configurableJoint.angularXMotion = ConfigurableJointMotion.Limited;
        //    configurableJoint.angularYMotion = ConfigurableJointMotion.Limited;
        //    configurableJoint.angularZMotion = ConfigurableJointMotion.Limited;
        //}

        //
        //ingredient.GetComponent<BoxCollider>().isTrigger = false;

        //ConfigurableJoint configurableJoint2 = BurgerTop.gameObject.AddComponent<ConfigurableJoint>();
        //configurableJoint2.connectedBody = _ingredients[_ingredients.Count - 1].GetComponent<Rigidbody>();

        //configurableJoint2.xMotion = ConfigurableJointMotion.Limited;
        //configurableJoint2.yMotion = ConfigurableJointMotion.Limited;
        //configurableJoint2.zMotion = ConfigurableJointMotion.Limited;

        //configurableJoint2.angularXMotion = ConfigurableJointMotion.Limited;
        //configurableJoint2.angularYMotion = ConfigurableJointMotion.Limited;
        //configurableJoint2.angularZMotion = ConfigurableJointMotion.Limited;


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
