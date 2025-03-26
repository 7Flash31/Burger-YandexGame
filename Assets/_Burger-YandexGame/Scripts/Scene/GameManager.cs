using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class GameManager : MonoBehaviour
{
    private RecipeData _recipeData;
    public List<RecipeIngredient> Recipe;

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
            PlayerPrefs.SetInt(SaveData.MoneyKey, newValue);
        }
        else
        {
            PlayerPrefs.SetInt(SaveData.MoneyKey, 0);
        }

        _uiController.UpdateMoneyText(Money);
    }

    public void SpinFortuneWheel()
    {
        if(PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) > 0)
        {
            PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) - 1);

            _uiController.UpdateFortuneWheelText(PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey).ToString());
        }

        else
        {
            _uiController.UpdateFortuneWheelText(PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey).ToString());
        }
    }


    public void SaveGame()
    {
        YandexGame.savesData.Money = PlayerPrefs.GetInt(SaveData.MoneyKey);
        YandexGame.savesData.LastLevel = PlayerPrefs.GetInt(SaveData.LastLevelKey);
        YandexGame.savesData.FortuneSpins = PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey);
        //YandexGame.savesData.UnlockSkinId = PlayerPrefs.GetInt(SaveData.MoneyKey);

        YandexGame.savesData.LastSavedDate = PlayerPrefs.GetString(SaveData.LastSavedDateKey);
        YandexGame.savesData.LastSavedStreak = PlayerPrefs.GetInt(SaveData.LastSavedStreakKey);

        YandexGame.SaveProgress();
    }
}

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

public static class SaveData //PlayerPrefs
{
    public static string MoneyKey { get; private set; } = "Money";
    public static string LastLevelKey { get; private set; } = "LastLevel";
    public static string FortuneWheelSpineKey { get; private set; } = "FortuneWheelSpine";

    public static string MusicKey { get; private set; } = "Music";
    public static string SensitivityKey { get; private set; } = "Sensitivity";

    public static string LastSavedDateKey { get; private set; } = "LastSavedDate";
    public static string LastSavedStreakKey { get; private set; } = "LastSavedStreak";
}