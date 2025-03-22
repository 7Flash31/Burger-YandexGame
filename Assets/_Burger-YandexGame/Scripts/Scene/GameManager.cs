using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public Player Player { get; private set; }
    [SerializeField] private UIController _uiController;

    [SerializeField] private HeadController _headController;

    public float GameMusic { get; set; }
    public bool GameLaunch { get; private set; }
    public int TotalIngredientsCount { get; private set; }
    public int Money { get; private set; }

    public List<Ingredient> FinalIngredients { get; set; } = new List<Ingredient>();
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        DOTween.Init();
        Player.enabled = false;

        TotalIngredientsCount = FindObjectsByType<Ingredient>(FindObjectsSortMode.None).Length;

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
        _uiController.HideHelpPanel();
    }

    public void StopGame()
    {        
        Player.enabled = false;
        _headController.PlayEatAnimation();
    }

    public void FinalGame()
    {
        _headController.PlayAngryAnimation();
        _uiController.ShowFinalPanel();
    }

    public void UpdateMoney(int newValue)
    {
        if(newValue >= 0)
        {
            Money = newValue;
        }
        else
        {
            Money = 0;
        }

        _uiController.UpdateMoneyText(Money);
    }
}

public enum IngredientType
{
    Meat,
    Pepper,
    Cheese,
    Tomato,
    Fish,
    Avocado,
    Egg,
    Bacon,
    Cutlet,
    Salad,
    None,
}