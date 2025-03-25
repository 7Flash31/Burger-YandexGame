using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //[field: SerializeField] public List<RecipeIngredient> Recipe { get; private set; }

    private RecipeData _recipeData;

    public List<RecipeIngredient> Recipe;

    public float GameMusic { get; set; }
    public bool GameLaunch { get; private set; }
    public int TotalIngredientsCount { get; private set; }
    public int Money { get; private set; }
    public Player Player { get; private set; }

    public List<Ingredient> FinalIngredients { get; set; } = new List<Ingredient>();
    public static GameManager Instance { get; private set; }

    private UIController _uiController;
    private HeadController _headController;

    private void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        if(scene.buildIndex == 0)
        {
            _recipeData = Resources.Load<RecipeData>($"Recipe/Level");
        }
        else
        {
            _recipeData = Resources.Load<RecipeData>($"Recipe/Level {scene.buildIndex}");
        }

        Recipe = _recipeData.RecipeIngredients;


        Player = FindAnyObjectByType<Player>();
        _uiController = FindAnyObjectByType<UIController>();
        _headController = FindAnyObjectByType<HeadController>();
        TotalIngredientsCount = FindObjectsByType<Ingredient>(FindObjectsSortMode.None).Length;

        _uiController.UpdateSceneText((scene.buildIndex + 1).ToString());
        _uiController.SetRecipe();

        Player.enabled = false;

    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoad;

        DOTween.Init();

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

//[Serializable]
//public class RecipeIngredient
//{
//    [field: SerializeField] public Ingredient Ingredient { get; private set; }
//    [field: SerializeField] public int Count { get; private set; }
//}

[System.Serializable]
public class RecipeIngredient
{
    [SerializeField] private Ingredient ingredient;
    [SerializeField] private int count;

    public Ingredient Ingredient => ingredient;
    public int Count => count;

    public RecipeIngredient(Ingredient ingredient, int count)
    {
        this.ingredient = ingredient;
        this.count = count;
    }
}