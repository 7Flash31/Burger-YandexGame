using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestLevelMove : MonoBehaviour
{
    [SerializeField] private GameObject _ingredientPrefab;
    [SerializeField] private GameObject _downIngredient;
    [SerializeField] private GameObject _topIngredient;
    [SerializeField] private GameObject _burgerComponent;
    [SerializeField] private float _speed;
    [SerializeField] private float _sensitivityMouse;
    [SerializeField] private float Vertical;

    [SerializeField] private List<GameObject> _ingredients;
    private CharacterController controller;
    private float _verticalVelocity;
    private float _gravity = 9.81f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float _horizontal = Input.GetAxis("Horizontal");

        if(Input.GetMouseButton(0))
        {
            _horizontal = Input.GetAxis("Mouse X") * _sensitivityMouse;
        }

        if(controller.isGrounded)
        {
            _verticalVelocity = 0f;
        }
        else
        {
            _verticalVelocity -= _gravity * Time.deltaTime;
        }

        Vector3 moveDirection = new Vector3(_horizontal, _verticalVelocity, Vertical);
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= _speed;

        controller.Move(moveDirection * Time.deltaTime);

        if(Input.GetKeyDown(KeyCode.Q))
        {
            AddIngredient();
        }
    }

    private void AddIngredient()
    {
        GameObject ingredient = Instantiate(_ingredientPrefab);
        ingredient.transform.SetParent(_burgerComponent.transform);
        ingredient.GetComponent<Ingredient>().StopAnimation();

        GameObject previousObject;
        if(_ingredients.Count == 0)
        {
            previousObject = _downIngredient;
        }
        else
        {
            previousObject = _ingredients[_ingredients.Count - 1];
        }

        ingredient.transform.rotation = previousObject.transform.rotation;

        Vector3 topPrev = GetBoxColliderTopWorldPoint(previousObject);
        Vector3 bottomNewLocal = GetBoxColliderBottomLocal(ingredient);

        ingredient.transform.position = Vector3.zero;

        Vector3 bottomNewWorld = ingredient.transform.TransformPoint(bottomNewLocal);

        ingredient.transform.position = topPrev - (bottomNewWorld - ingredient.transform.position);

        _ingredients.Add(ingredient);

        HingeJoint hingeJoint = ingredient.AddComponent<HingeJoint>();
        hingeJoint.useSpring = true;
        hingeJoint.useLimits = true;

        if(_ingredients.Count == 1)
        {
            hingeJoint.connectedBody = _downIngredient.GetComponent<Rigidbody>();
        }
        else
        {
            hingeJoint.connectedBody = _ingredients[_ingredients.Count - 2].GetComponent<Rigidbody>();
        }

        _topIngredient.transform.rotation = ingredient.transform.rotation;

        Vector3 topNewWorld = GetBoxColliderTopWorldPoint(ingredient);

        Vector3 bottomTopLocal = GetBoxColliderBottomLocal(_topIngredient);

        _topIngredient.transform.position = Vector3.zero;
        Vector3 bottomTopWorld = _topIngredient.transform.TransformPoint(bottomTopLocal);

        _topIngredient.transform.position = topNewWorld - (bottomTopWorld - _topIngredient.transform.position);

        HingeJoint topHinge = _topIngredient.GetComponent<HingeJoint>();
        if(topHinge)
        {
            topHinge.connectedBody = ingredient.GetComponent<Rigidbody>();
        }

        UpdateMasses();
    }

    private void UpdateMasses()
    {
        int totalCount = _ingredients.Count + 1;

        for(int i = 0; i < totalCount; i++)
        {
            GameObject current;
            if(i == 0)
            {
                current = _downIngredient;
            }
            else
            {
                current = _ingredients[i - 1];
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
