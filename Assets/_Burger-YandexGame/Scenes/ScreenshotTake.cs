using UnityEngine;
using System.Collections;

public class ScreenshotTake : MonoBehaviour
{
    [SerializeField] private Transform _camera;
    [SerializeField] private GameObject[] _screenshotObject;
    [SerializeField] private int Index;

    private Vector3 _screenPosition;

    private void Start()
    {
        StartCoroutine(CaptureScreenshots());
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            ScreenCapture.CaptureScreenshot($"C:\\Users\\arter\\Desktop\\screen\\screenshot{Index}.png");
            Index++;
        }
    }

    private IEnumerator CaptureScreenshots()
    {
        for(int i = 0; i < _screenshotObject.Length; i++)
        {
            // Настраиваем камеру и позицию объекта
            _camera.SetParent(_screenshotObject[i].transform);
            _camera.localPosition = new Vector3(0, -0.7f, 0);
            _screenshotObject[i].transform.position = new Vector3(_screenPosition.x += 50, _screenPosition.y, _screenPosition.z);

            // Ждем конца кадра, чтобы все изменения отобразились
            yield return new WaitForEndOfFrame();

            // Захватываем скриншот
            ScreenCapture.CaptureScreenshot($"C:\\Users\\arter\\Desktop\\screen\\screenshot{i}.png");
            Debug.Log("Screenshot " + i + " captured.");
        }
        Debug.Log("Complete");
    }
}
