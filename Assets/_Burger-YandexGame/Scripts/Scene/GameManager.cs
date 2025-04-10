using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [field: Header("Price")]
    [field: SerializeField] public int GoodIngredientPrice { get; private set; }
    [field: SerializeField] public int BadIngredientPrice { get; private set; }
    [field: SerializeField] public int RecipeIngredientPrice { get; private set; }
    [SerializeField] private int _incomeModePrice;
    [SerializeField] private int _luckModePrice;

    [Header("Prefabs")]
    [SerializeField] private GameObject _luckTextPrefab;
    [SerializeField] private GameObject _luckParticlePrefab;

    [Header("Materials")]
    [SerializeField] private Material _roadMaterialOne;
    [SerializeField] private Material _roadMaterialTwo;

    [Space(10)]
    [SerializeField] private Color _roadColorDefaultOne;
    [SerializeField] private Color _roadColorDefaultTwo;

    [Space(10)]
    [SerializeField] private Color _roadColorBonusLevelOne;
    [SerializeField] private Color _roadColorBonusLevelTwo;

    [Header("UI")]
    [HideInInspector] public UIController UIController;
    private RecipeData _recipeData;
    private HeadController _headController;

    [field: Header("Skins")]
    [field: SerializeField] public SkinData[] Skins { get; private set; }

    // Game State
    public Player Player { get; private set; }
    public bool GameLaunch { get; private set; }
    public int TotalIngredientsCount { get; private set; }

    [field: Header("Multiply")]
    [field: SerializeField] public float IncomeMultiply { get; private set; }
    [field: SerializeField] public float LuckMultiply { get; private set; }
    [field: SerializeField] public float BonusLevelMultiply { get; private set; }

    // Mods
    public bool IncomeModeEnabled { get; private set; }
    public bool LuckModeEnabled { get; private set; }
    public bool BonusLevelEnabled { get; private set; }

    public List<RecipeIngredient> Recipe { get; private set; }
    public List<Ingredient> FinalIngredients { get; set; } = new List<Ingredient>();

    public int CurrentSkinID { get; set; }
    public List<int> PurchasedSkins { get; set; } = new List<int>();

    private void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        IncomeModeEnabled = false;
        LuckModeEnabled = false;
        BonusLevelEnabled = false;

        _roadMaterialOne.color = _roadColorDefaultOne;
        _roadMaterialTwo.color = _roadColorDefaultTwo;

        if(scene.buildIndex == 0)
        {
            _recipeData = Resources.Load<RecipeData>($"Recipe/Level");

            PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, 2);
        }
        else
        {
            _recipeData = Resources.Load<RecipeData>($"Recipe/Level {scene.buildIndex}");
        }

        Recipe = _recipeData.RecipeIngredients;

        Player = FindAnyObjectByType<Player>();
        UIController = FindAnyObjectByType<UIController>();
        _headController = FindAnyObjectByType<HeadController>();
        TotalIngredientsCount = FindObjectsByType<Ingredient>(FindObjectsSortMode.None).Length;

        UIController.UpdateSceneText((scene.buildIndex + 1).ToString());
        UIController.SetRecipe();

        Player.enabled = false;

        if(scene.buildIndex % 6 == 0 && scene.buildIndex != 0)
        {
            ProposeBonusLevel();
        }
        if(scene.buildIndex % 10 == 0 && scene.buildIndex != 0)
        {
            PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) + 1);
            UIController.UpdateFortuneWheelText(PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey).ToString());  
        }
    }

    private void Awake()
    {
        YandexGame.savesData.LastSavedDate = "";
        YandexGame.savesData.LastSavedStreak = 0;

        PurchasedSkins.Add(0);

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
        UIController.HideHelpPanel();
    }

    public void StopGame()
    {
        Player.enabled = false;
        _headController.PlayEatAnimation();
    }

    public void FinalGame()
    {
        _headController.PlayAngryAnimation();
        UIController.ShowFinalPanel();

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

        UIController.UpdateMoneyText(PlayerPrefs.GetInt(SaveData.MoneyKey, 0));
    }

    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public void SaveGame()
    {
        YandexGame.savesData.Money = PlayerPrefs.GetInt(SaveData.MoneyKey);
        YandexGame.savesData.LastLevel = PlayerPrefs.GetInt(SaveData.LastLevelKey);
        YandexGame.savesData.FortuneSpins = PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey);


        //YandexGame.savesData.UnlockSkinId = PlayerPrefs.GetInt(SaveData.SkinIdKey);

        YandexGame.savesData.LastSavedDate = PlayerPrefs.GetString(SaveData.LastSavedDateKey);
        YandexGame.savesData.LastSavedStreak = PlayerPrefs.GetInt(SaveData.LastSavedStreakKey);

        YandexGame.SaveProgress();
    }

    public void EnableLuckMode(bool reward = false, bool isGift = false)
    {
        if(isGift)
        {
            LuckModeEnabled = true;

            foreach(var item in FindObjectsByType<Ingredient>(FindObjectsSortMode.None))
            {
                if(Random.value < 0.5)
                {
                    Instantiate(_luckTextPrefab, item.transform);
                    Instantiate(_luckParticlePrefab, item.transform);

                    item.IsLuckIngredient = true;
                }
            }

            UIController.LuckButton.interactable = false;

            return;
        }

        if(PlayerPrefs.GetInt(SaveData.MoneyKey) > _luckModePrice || reward)
        {
            if(PlayerPrefs.GetInt(SaveData.MoneyKey) > _luckModePrice)
                UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey) - _luckModePrice);

            LuckModeEnabled = true;

            foreach(var item in FindObjectsByType<Ingredient>(FindObjectsSortMode.None))
            {
                if(Random.value < 0.5)
                {
                    Instantiate(_luckTextPrefab, item.transform);
                    Instantiate(_luckParticlePrefab, item.transform);

                    item.IsLuckIngredient = true;
                }
            }
        }
        else
        {
            YandexGame.RewVideoShow(SaveData.LuckReward);
        }
    }

    public void EnableIncomeMode(bool reward = false, bool isGift = false)
    {
        if(isGift)
        {
            float multipler = 1;
            IncomeMultiply = 1;
            for(int i = 0; i <= SceneManager.GetActiveScene().buildIndex; i++)
            {
                multipler += 0.05f;
            }

            IncomeMultiply = multipler;
            IncomeModeEnabled = true;

            UIController.IncomeButton.interactable = false;

            return;
        }

        if(PlayerPrefs.GetInt(SaveData.MoneyKey) > _incomeModePrice || reward)
        {
            if(PlayerPrefs.GetInt(SaveData.MoneyKey) > _incomeModePrice)
                UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey) - _incomeModePrice);

            float multipler = 1;
            IncomeMultiply = 1;
            for(int i = 0; i <= SceneManager.GetActiveScene().buildIndex; i++)
            {
                multipler += 0.05f;
            }

            IncomeMultiply = multipler;
            IncomeModeEnabled = true;
        }
        else
        {
            YandexGame.RewVideoShow(SaveData.IncomeReward);
        }
    }

    public void EnableBonusLevel()
    {
        UIController.HideBonusPanel();
        BonusLevelEnabled = true;
        LaunchGame();
    }
    
    public void SkipBonusLevelLevel()
    {
        UIController.HideBonusPanel();
        BonusLevelEnabled = false;

        _roadMaterialOne.color = _roadColorDefaultOne;
        _roadMaterialTwo.color = _roadColorDefaultTwo;

        foreach(var item in FindObjectsByType<Ingredient>(FindObjectsSortMode.None))
        {
            if(item.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.material.color = new Color(108f / 255f, 108f / 255f, 108f / 255f);

            }
        }

        LaunchGame();
    }

    public void ChangeSkin(int skinIndex)
    {
        if(Skins[skinIndex] == null)
            return;

        Player.BurgerDown.GetComponent<MeshFilter>().mesh = Skins[skinIndex].BurgerDownMesh;
        Player.BurgerDown.GetComponent<MeshRenderer>().material = Skins[skinIndex].BurgerDownMaterial;

        Player.BurgerTop.GetComponent<MeshFilter>().mesh = Skins[skinIndex].BurgerTopMesh;
        Player.BurgerTop.GetComponent<MeshRenderer>().material = Skins[skinIndex].BurgerTopMaterial;
    }

    private void Rewarded(int id)
    {
        if(id == SaveData.LuckReward)
            EnableLuckMode(true);

        else if(id == SaveData.IncomeReward)
            EnableIncomeMode(true);
        
        else if(id == SaveData.BonusLevelReward)
            EnableBonusLevel();
        
        else if(id == SaveData.FortuneWheelReward)
        {
            PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) + 1);
        }
    }

    private void ProposeBonusLevel()
    {
        UIController.ProposeBonusLevel();

        _roadMaterialOne.color = _roadColorBonusLevelOne;
        _roadMaterialTwo.color = _roadColorBonusLevelTwo;

        foreach(var item in FindObjectsByType<Ingredient>(FindObjectsSortMode.None))
        {
            if(item.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.material.color = Color.yellow;
            }
        }
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

    public static string SkinIdKey { get; private set; } = "SkinID";

    public static int LuckReward { get; private set; } = 100;
    public static int IncomeReward { get; private set; } = 101;
    public static int BonusLevelReward { get; private set; } = 102;
    public static int FortuneWheelReward { get; private set; } = 103;
}