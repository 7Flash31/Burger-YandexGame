using UnityEngine;

public class HeadTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Сначала обрабатываем специальный случай: BurgerTop должен запускать FinalGame.
        if(other.transform == GameManager.Instance.Player.BurgerTop)
        {
            GameManager.Instance.FinalGame();
        }

        // Для всех остальных объектов выполняем нужное действие.
        other.gameObject.SetActive(false);
    }
}
