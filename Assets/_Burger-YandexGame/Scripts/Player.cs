using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [field: SerializeField] public Transform BurgerTop { get; private set; }
    [field: SerializeField] public Transform BurgerDown { get; private set; }
    [field: SerializeField] public float SensitivityMouse { get; set; }
    [field: SerializeField] public float SensitivityKeyboard { get; set; }

    [field: SerializeField] public float Vertical { get; set; }
    public bool CanMove { get; set; } = true;

    [SerializeField] private Transform _burgerComponents;
    [SerializeField] private float _speed;
    [SerializeField] private float _sensitivityTouch;

    [SerializeField] private List<Ingredient> _ingredients = new List<Ingredient>();

    private Rigidbody _rb;
    private float _horizontal;
    private bool _hasTriggered;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        Vertical = 1f;
        SensitivityMouse = PlayerPrefs.GetFloat(SaveData.MouseSensitivityKey, 1f);
        SensitivityKeyboard = PlayerPrefs.GetFloat(SaveData.KeyboardSensitivityKey, 1f);
        _sensitivityTouch = SensitivityMouse;

        GameManager.Instance.ChangeSkin(GameManager.Instance.CurrentSkinID);
    }

    private void Update()
    {
        _horizontal = Input.GetAxis("Horizontal");

        if (Input.GetMouseButton(0))
        {
            _horizontal = Input.GetAxis("Mouse X") * SensitivityMouse;
        }

        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    _horizontal = touch.deltaPosition.x * _sensitivityTouch;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if(CanMove)
        {
            Vector3 velocity = new Vector3(_horizontal, 0, Vertical) * _speed;
            Vector3 worldVelocity = transform.TransformDirection(velocity);
            _rb.velocity = worldVelocity;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.TryGetComponent(out Ingredient interactableItem))
        {
            AddIngredient(interactableItem);
        }

        if (_hasTriggered)
            return;

        if (collider.gameObject.TryGetComponent(out Trap trap))
        {
            DeleteIngredient(trap.RemoveIngredient);
            _hasTriggered = true;
        }
    }

    private void OnDisable()
    {
        GameManager.Instance.FinalIngredients.Clear();
        GameManager.Instance.FinalIngredients.AddRange(_ingredients);
    }

    public void UpdateSensitivity(float newSensitivity, bool isMouseSensitivity)
    {
        if(isMouseSensitivity)
            SensitivityMouse = newSensitivity;
        else
            SensitivityKeyboard = newSensitivity;
    }

    public void DeleteIngredient(int count)
    {
        int a = Mathf.Min(_ingredients.Count, count);

        for (int i = a - 1; i >= 0; i--)
        {
            if (_ingredients[i] != null)
            {
                _ingredients[i].IsDropped = true;
                _ingredients[i].transform.SetParent(null);

                Destroy(_ingredients[i].gameObject);

                _ingredients.RemoveAt(i);
            }
        }

        if (_ingredients.Count == 0)
        {
            BurgerTop.transform.rotation = BurgerDown.transform.rotation;

            Vector3 topNewWorld = GetBoxColliderTopWorldPoint(BurgerDown.gameObject);
            Vector3 bottomTopLocal = GetBoxColliderBottomLocal(BurgerTop.gameObject);

            BurgerTop.transform.position = Vector3.zero;
            Vector3 bottomTopWorld = BurgerTop.transform.TransformPoint(bottomTopLocal);

            BurgerTop.transform.position = topNewWorld - (bottomTopWorld - BurgerTop.transform.position);

            HingeJoint topHinge = BurgerTop.GetComponent<HingeJoint>();
            if (topHinge)
            {
                topHinge.connectedBody = BurgerDown.GetComponent<Rigidbody>();
            }
        }

        for (int i = 0; i < _ingredients.Count; i++)
        {
            GameObject previousObject = null;
            if (i == 0)
            {
                previousObject = BurgerDown.gameObject;
            }
            else
            {
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

            if (i == 0)
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
            if (topHinge)
            {
                topHinge.connectedBody = _ingredients[i].GetComponent<Rigidbody>();
            }
        }

        if(_ingredients.Count > 0)
        {
            StartCoroutine(SetHasTrigger());
        }
        else
        {
            GameManager.Instance.LoseGame();
        }

    }

    public void DeleteJoint(Transform playerContainer)
    {
        foreach (var item in _ingredients)
        {
            Destroy(item.GetComponent<HingeJoint>());
            Destroy(item.GetComponent<Rigidbody>());

            item.transform.SetParent(playerContainer);
            item.transform.localPosition = new Vector3(0, item.transform.localPosition.y, 0);

        }

        Destroy(BurgerDown.GetComponent<HingeJoint>());
        Destroy(BurgerDown.GetComponent<Rigidbody>());
        BurgerDown.SetParent(playerContainer);
        BurgerDown.transform.localPosition = new Vector3(0, BurgerDown.transform.localPosition.y, 0);

        Destroy(BurgerTop.GetComponent<HingeJoint>());
        Destroy(BurgerTop.GetComponent<Rigidbody>());
        BurgerTop.SetParent(playerContainer);
        BurgerTop.transform.localPosition = new Vector3(0, BurgerTop.transform.localPosition.y, 0);

    }

    private IEnumerator SetHasTrigger()
    {
        yield return new WaitForSeconds(0.1f);
        _hasTriggered = false;
    }

    private void AddIngredient(Ingredient ingredient)
    {
        if (_ingredients.Contains(ingredient))
            return;

        ingredient.transform.SetParent(_burgerComponents.transform);
        ingredient.StopAnimation();

        GameObject previousObject;
        if (_ingredients.Count == 0)
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

        if (_ingredients.Count == 1)
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
        if (topHinge)
        {
            topHinge.connectedBody = ingredient.GetComponent<Rigidbody>();
        }

        ingredient.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        ingredient.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        UpdateMasses();
        SetMassScale(10, 7);
    }

    private void UpdateMasses()
    {
        int totalCount = _ingredients.Count + 1;

        for (int i = 0; i < totalCount; i++)
        {
            GameObject current;
            if (i == 0)
            {
                current = BurgerDown.gameObject;
            }
            else
            {
                current = _ingredients[i - 1].gameObject;
            }

            if (current != null)
            {
                Rigidbody rb = current.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.mass = totalCount - i;
                }
            }
        }
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

    private Vector3 GetBoxColliderTopWorldPoint(GameObject go)
    {
        BoxCollider coll = go.GetComponent<BoxCollider>();
        if (!coll)
            return go.transform.position;

        Vector3 topLocal = coll.center + new Vector3(0, coll.size.y / 2f, 0);
        return go.transform.TransformPoint(topLocal);
    }

    private Vector3 GetBoxColliderBottomLocal(GameObject go)
    {
        BoxCollider coll = go.GetComponent<BoxCollider>();
        if (!coll)
            return Vector3.zero;

        return coll.center - new Vector3(0, coll.size.y / 2f, 0);
    }
}
