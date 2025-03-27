using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [field: SerializeField] public int GoodIngredientPrice { get; private set; }
    [field: SerializeField] public int BadIngredientPrice { get; private set; }
    [field: SerializeField] public int RecipeIngredientPrice { get; private set; }

    [SerializeField] private GameObject _luckTextPrefab;

    private RecipeData _recipeData;
    private UIController _uiController;
    private HeadController _headController;

    public Player Player { get; private set; }
    public bool GameLaunch { get; private set; }
    public int TotalIngredientsCount { get; private set; }

    public float IncomeMultiply { get; private set; }
    public float LuckMultiply { get; private set; }
    public bool IncomeModeEnabled { get; private set; }
    public bool LuckModeEnabled { get; private set; }

    public List<RecipeIngredient> Recipe;
    public List<Ingredient> FinalIngredients { get; set; } = new List<Ingredient>();

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

        //UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey, 0) + _finalReward);
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

        _uiController.UpdateMoneyText(PlayerPrefs.GetInt(SaveData.MoneyKey, 0));
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

    public void EnableLuckMode()
    {
        LuckModeEnabled = true;

        foreach(var item in FindObjectsByType<Ingredient>(FindObjectsSortMode.None))
        {

            Instantiate(_luckTextPrefab, item.transform);
        }
    }

    public void EnableIncomeMode(bool reward = false)
    {
        int incomeModePrice = 50;
        if(PlayerPrefs.GetInt(SaveData.MoneyKey) > incomeModePrice || reward)
        {
            float multipler = 1;
            IncomeMultiply = 1;
            for(int i = 0; i <= SceneManager.GetActiveScene().buildIndex; i++)
            {
                multipler += 0.05f;
            }

            IncomeMultiply = multipler;
            IncomeModeEnabled = true;

            print(IncomeMultiply);
        }
        else
        {
            YandexGame.RewVideoShow(SaveData.LuckReward);
        }
    }

    private void Rewarded(int id)
    {
        if(id == SaveData.LuckReward)
            EnableLuckMode();

        else if(id == SaveData.IncomeReward)
            EnableIncomeMode(true);
    }

    private void OnEnable()
    {
        YandexGame.RewardVideoEvent += Rewarded;
    }

    private void OnDisable()
    {
        YandexGame.RewardVideoEvent -= Rewarded;
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


    public static int LuckReward { get; private set; } = 100;
    public static int IncomeReward { get; private set; } = 101;
}