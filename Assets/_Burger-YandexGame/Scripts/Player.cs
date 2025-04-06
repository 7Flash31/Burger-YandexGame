using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Player : MonoBehaviour
{
    [field: SerializeField] public Transform BurgerTop { get; private set; }
    [field: SerializeField] public Transform BurgerDown { get; private set; }

    public float Vertical { get; set; }

    [SerializeField] private Transform _burgerComponents;
    [SerializeField] private float _speed;
    [SerializeField] private float _sensitivityMouse;
    [SerializeField] private float _sensitivityTouch;
    [SerializeField] private float _horizontalSmoothSpeed;
    [SerializeField] private float _verticalSmoothSpeed;

    [SerializeField] private List<Ingredient> _ingredients = new List<Ingredient>();
    private CharacterController controller;

    private float _gravity = 9.81f;
    private float _verticalSmooth;
    private float _verticalVelocity;
    private float _horizontalSmooth;
    private float _horizontal;

    private bool _hasTriggered;

    private Vector3 moveDirection;

    private void Start()
    {
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
            _verticalVelocity = 0f;
        }
        else
        {
            _verticalVelocity -= _gravity * Time.deltaTime;
        }

        _horizontalSmooth = Mathf.Lerp(_horizontalSmooth, _horizontal, Time.deltaTime * _horizontalSmoothSpeed);
        _verticalSmooth = Mathf.Lerp(_verticalSmooth, Vertical, Time.deltaTime * _verticalSmoothSpeed);

        moveDirection = new Vector3(_horizontalSmooth, _verticalVelocity, _verticalSmooth);
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= _speed;

    }

    private void FixedUpdate()
    {
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.transform.TryGetComponent(out Ingredient interactableItem))
        {
            AddIngredient(interactableItem);
        }

        if(_hasTriggered)
            return;

        if(collider.gameObject.TryGetComponent(out Trap trap))
        {
            DeleteRandomIngredient(trap.RemoveIngredient);
            _hasTriggered = true;
        }
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

                Destroy(_ingredients[i].gameObject);

                _ingredients.RemoveAt(i);
            }
        }

        if(_ingredients.Count == 0)
        {
            BurgerTop.transform.rotation = BurgerDown.transform.rotation;

            Vector3 topNewWorld = GetBoxColliderTopWorldPoint(BurgerDown.gameObject);

            Vector3 bottomTopLocal = GetBoxColliderBottomLocal(BurgerTop.gameObject);

            BurgerTop.transform.position = Vector3.zero;
            Vector3 bottomTopWorld = BurgerTop.transform.TransformPoint(bottomTopLocal);

            BurgerTop.transform.position = topNewWorld - (bottomTopWorld - BurgerTop.transform.position);

            HingeJoint topHinge = BurgerTop.GetComponent<HingeJoint>();
            if(topHinge)
            {
                topHinge.connectedBody = BurgerDown.GetComponent<Rigidbody>();
            }
        }
        for(int i = 0; i < _ingredients.Count; i++)
        {
            GameObject previousObject = null;
            if(i == 0)
            {
                // Первый ингредиент «опирается» на нижнюю булку
                previousObject = BurgerDown.gameObject;
            }
            else
            {
                // Следующие ингредиенты опираются на предыдущий
                previousObject = _ingredients[i - 1].gameObject;
            }

            _ingredients[i].transform.rotation = previousObject.transform.rotation;

            Vector3 topPrev = GetBoxColliderTopWorldPoint(previousObject);
            Vector3 bottomNewLocal = GetBoxColliderBottomLocal(_ingredients[i].gameObject);

            _ingredients[i].transform.position = Vector3.zero;

            Vector3 bottomNewWorld = _ingredients[i].transform.TransformPoint(bottomNewLocal);

            _ingredients[i].transform.position = topPrev - (bottomNewWorld - _ingredients[i].transform.position);

            HingeJoint hingeJoint = _ingredients[i].gameObject.GetComponent<HingeJoint>();
            hingeJoint.useSpring = true;
            hingeJoint.useLimits = true;

            if(i == 0)
            {
                hingeJoint.connectedBody = BurgerDown.GetComponent<Rigidbody>();
            }
            else
            {
                hingeJoint.connectedBody = _ingredients[i - 1].GetComponent<Rigidbody>();
            }

            BurgerTop.transform.rotation = _ingredients[i].transform.rotation;

            Vector3 topNewWorld = GetBoxColliderTopWorldPoint(_ingredients[i].gameObject);

            Vector3 bottomTopLocal = GetBoxColliderBottomLocal(BurgerTop.gameObject);

            BurgerTop.transform.position = Vector3.zero;
            Vector3 bottomTopWorld = BurgerTop.transform.TransformPoint(bottomTopLocal);

            BurgerTop.transform.position = topNewWorld - (bottomTopWorld - BurgerTop.transform.position);

            HingeJoint topHinge = BurgerTop.GetComponent<HingeJoint>();
            if(topHinge)
            {
                topHinge.connectedBody = _ingredients[i].GetComponent<Rigidbody>();
            }
        }

        StartCoroutine(SetHasTrigger());
    }

    public void SetMassScale(float massScale, float connectedMassScale)
    {
        foreach(var item in _ingredients)
        {
            HingeJoint hingeJoint = item.GetComponent<HingeJoint>();
            hingeJoint.massScale = massScale;
            hingeJoint.connectedMassScale = connectedMassScale;
        }

        BurgerTop.GetComponent<HingeJoint>().massScale = massScale;
        BurgerTop.GetComponent<HingeJoint>().connectedMassScale = connectedMassScale;
    }

    public void DeleteJoint()
    {
        foreach(var item in _ingredients)
        {
            Destroy(item.GetComponent<HingeJoint>());
            Destroy(item.GetComponent<Rigidbody>());

            item.transform.SetParent(transform.parent);
            item.transform.localPosition = new Vector3(0, item.transform.localPosition.y, 0);
        }

        Destroy(BurgerDown.GetComponent<HingeJoint>());
        Destroy(BurgerDown.GetComponent<Rigidbody>());
        BurgerDown.SetParent(transform.parent);
        BurgerDown.transform.localPosition = new Vector3(0, BurgerDown.transform.localPosition.y, 0);

        Destroy(BurgerTop.GetComponent<HingeJoint>());
        Destroy(BurgerTop.GetComponent<Rigidbody>());
        BurgerTop.SetParent(transform.parent);
        BurgerTop.transform.localPosition = new Vector3(0, BurgerTop.transform.localPosition.y, 0);

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

    private void AddIngredient(Ingredient ingredient)
    {
        if(_ingredients.Contains(ingredient))
            return;

        ingredient.transform.SetParent(_burgerComponents.transform);
        ingredient.StopAnimation();

        GameObject previousObject;
        if(_ingredients.Count == 0)
        {
            previousObject = BurgerDown.gameObject;
        }
        else
        {
            previousObject = _ingredients[_ingredients.Count - 1].gameObject;
        }

        ingredient.transform.rotation = previousObject.transform.rotation;

        Vector3 topPrev = GetBoxColliderTopWorldPoint(previousObject);
        Vector3 bottomNewLocal = GetBoxColliderBottomLocal(ingredient.gameObject);

        ingredient.transform.position = Vector3.zero;

        Vector3 bottomNewWorld = ingredient.transform.TransformPoint(bottomNewLocal);

        ingredient.transform.position = topPrev - (bottomNewWorld - ingredient.transform.position);

        _ingredients.Add(ingredient);

        HingeJoint hingeJoint = ingredient.gameObject.AddComponent<HingeJoint>();
        hingeJoint.useSpring = true;
        hingeJoint.useLimits = true;

        if(_ingredients.Count == 1)
        {
            hingeJoint.connectedBody = BurgerDown.GetComponent<Rigidbody>();
        }
        else
        {
            hingeJoint.connectedBody = _ingredients[_ingredients.Count - 2].GetComponent<Rigidbody>();
        }

        BurgerTop.transform.rotation = ingredient.transform.rotation;

        Vector3 topNewWorld = GetBoxColliderTopWorldPoint(ingredient.gameObject);

        Vector3 bottomTopLocal = GetBoxColliderBottomLocal(BurgerTop.gameObject);

        BurgerTop.transform.position = Vector3.zero;
        Vector3 bottomTopWorld = BurgerTop.transform.TransformPoint(bottomTopLocal);

        BurgerTop.transform.position = topNewWorld - (bottomTopWorld - BurgerTop.transform.position);

        HingeJoint topHinge = BurgerTop.GetComponent<HingeJoint>();
        if(topHinge)
        {
            topHinge.connectedBody = ingredient.GetComponent<Rigidbody>();
        }

        UpdateMasses();
        SetMassScale(10, 10);
    }

    private void UpdateMasses()
    {
        int totalCount = _ingredients.Count + 1;

        for(int i = 0; i < totalCount; i++)
        {
            GameObject current;
            if(i == 0)
            {
                current = BurgerDown.gameObject;
            }
            else
            {
                current = _ingredients[i - 1].gameObject;
            }

            if(current != null)
            {
                Rigidbody rb = current.GetComponent<Rigidbody>();
                if(rb != null)
                {
                    rb.mass = totalCount - i;
                }
            }
        }
    }

    private Vector3 GetBoxColliderTopWorldPoint(GameObject go)
    {
        BoxCollider coll = go.GetComponent<BoxCollider>();
        if(!coll)
            return go.transform.position;

        Vector3 topLocal = coll.center + new Vector3(0, coll.size.y / 2f, 0);
        return go.transform.TransformPoint(topLocal);
    }

    private Vector3 GetBoxColliderBottomLocal(GameObject go)
    {
        BoxCollider coll = go.GetComponent<BoxCollider>();
        if(!coll)
            return Vector3.zero;

        return coll.center - new Vector3(0, coll.size.y / 2f, 0);
    }
}
