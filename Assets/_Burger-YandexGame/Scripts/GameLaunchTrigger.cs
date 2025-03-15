using UnityEngine;
using UnityEngine.EventSystems;

public class GameLaunchTrigger : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.LaunchGame();
    }
}
