using UnityEngine;
using System.Collections;
using TMPro.Examples;

public class RotationTrigger : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private GameObject _newRoadWall;
    [SerializeField] private GameObject _oldRoadWall;
    [SerializeField] private GameObject _cameraPos;
    [SerializeField] private Vector3 _toRotate;
    private CameraController _сameraController;

    private float _tempSensitivityMouse;
    private float _tempSensitivityKeyboard;
    private bool _isRotating;

    private void Start()
    {
        _сameraController = Camera.main.GetComponent<CameraController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player player) && !_isRotating)
        {
            _isRotating = true;
            player.Vertical = 0.7f;

            _tempSensitivityMouse = player.SensitivityMouse;
            _tempSensitivityKeyboard = player.SensitivityKeyboard;

            player.SensitivityMouse = 0f;
            player.SensitivityKeyboard = 0f;

            StartCoroutine(RotatePlayer(player));
        }
    }

    private IEnumerator RotatePlayer(Player player)
    {
        Quaternion startRotation = player.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(_toRotate);
        float journeyLength = Quaternion.Angle(startRotation, endRotation);
        float startTime = Time.time;

        while(Vector3.Angle(player.transform.forward, endRotation * Vector3.forward) > 0.1f)
        {
            float distanceCovered = (Time.time - startTime) * _speed;
            float fractionOfJourney = distanceCovered / journeyLength;

            player.transform.rotation = Quaternion.Slerp(startRotation, endRotation, fractionOfJourney);






            ///!!!!!!!!!!!!!!!!
            _сameraController.OnTriggreRotate(player.transform, false);
            ///!!!!!!!!!!!!!!




            yield return null;
        }

        player.transform.rotation = endRotation;

        _newRoadWall.SetActive(false);
        _oldRoadWall.SetActive(true);



        ///!!!!!!!!!!!!!!!!  OnComplete => return back
        //_сameraController.OnTriggreRotate(player.transform);
        ///!!!!!!!!!!!!!!
        _сameraController.OnTriggreRotate(player.transform, true );




        player.Vertical = 1;
        player.SensitivityMouse = _tempSensitivityMouse;
        player.SensitivityKeyboard = _tempSensitivityKeyboard;
        _isRotating = false;
    }
}
