using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public Player Player { get; private set; }
    [field: SerializeField] public UIController UIController { get; private set; }

    public bool GameLaunch { get; private set; }
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        DOTween.Init();
        Player.enabled = false;

        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LaunchGame()
    {
        GameLaunch = true;
        Player.enabled = true;
        UIController.HideHelpPanel();
    }

    public void StopGame()
    {

    }
}