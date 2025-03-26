using UnityEngine;
using UnityEngine.EventSystems;

public class GameLaunchTrigger : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.LaunchGame();
        this.enabled = false;
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            GameManager.Instance.LaunchGame();
            this.enabled = false;
        }
    }
}
