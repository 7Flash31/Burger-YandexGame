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
    [field: SerializeField] public int IncomeModePrice { get; private set; }
    [field: SerializeField] public int LuckModePrice { get; private set; }

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

    public Player Player { get; private set; }
    public bool GameLaunch { get; private set; }
    public int TotalIngredientsCount { get; private set; }

    [field: Header("Multiply")]
    [field: SerializeField] public float IncomeMultiply { get; private set; }
    [field: SerializeField] public float LuckMultiply { get; private set; }
    [field: SerializeField] public float BonusLevelMultiply { get; private set; }

    public bool IncomeModeEnabled { get; private set; }
    public bool LuckModeEnabled { get; private set; }
    public bool BonusLevelEnabled { get; private set; }

    [field: SerializeField] public float LuckModeChance { get; private set; }
    [field: SerializeField] public float LuckModeBonus { get; private set; } = 0.25f;

    public List<RecipeIngredient> Recipe { get; private set; }
    public List<Ingredient> FinalIngredients { get; set; } = new List<Ingredient>();

    public int CurrentSkinID { get; set; }
    public List<int> PurchasedSkins { get; set; } = new List<int>();

    private AudioSource _musicSource;

    private void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        int buildIndex = scene.buildIndex;

        IncomeModeEnabled = false;
        LuckModeEnabled = false;
        BonusLevelEnabled = false;

        _roadMaterialOne.color = _roadColorDefaultOne;
        _roadMaterialTwo.color = _roadColorDefaultTwo;

        if(buildIndex == 0)
        {
            _recipeData = Resources.Load<RecipeData>($"Recipe/Level");
            PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, 2);
        }
        else
        {
            _recipeData = Resources.Load<RecipeData>($"Recipe/Level {buildIndex}");
        }

        Recipe = _recipeData.RecipeIngredients;

        Player = FindAnyObjectByType<Player>();
        UIController = FindAnyObjectByType<UIController>();
        _headController = FindAnyObjectByType<HeadController>();
        TotalIngredientsCount = FindObjectsByType<Ingredient>(FindObjectsSortMode.None).Length;

        UIController.UpdateSceneText((buildIndex + 1).ToString());
        UIController.SetRecipe();

        Player.CanMove = false;

        if(buildIndex % 6 == 0 && buildIndex != 0)
        {
            ProposeBonusLevel();
        }
        if(buildIndex % 10 == 0 && buildIndex != 0)
        {
            PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) + 1);
            UIController.UpdateFortuneWheelText(PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey).ToString());  
        }

        RefreshLuckModeChance(buildIndex);
    }

    private void Awake()
    {
        _musicSource = GetComponent<AudioSource>();
        _musicSource.volume = PlayerPrefs.GetFloat(SaveData.MusicKey, 0.3f);

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
        Player.CanMove = true;
        UIController.HideHelpPanel();
    }

    public void StopGame()
    {
        Player.CanMove = false;
        _headController.PlayEatAnimation();
    }

    public void FinalGame()
    {
        _headController.PlayAngryAnimation();
        UIController.ShowFinalPanel();
    }

    public void LoseGame()
    {
        Player.CanMove = false;
        Player.Vertical = 0;
        Camera.main.transform.SetParent(null);
        Player.transform.root.gameObject.SetActive(false);
        UIController.ShowDeathPanel();
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
        int money = PlayerPrefs.GetInt(SaveData.MoneyKey);
        bool enableLuckMode = false;

        if(isGift)
        {
            enableLuckMode = true;
        }
        else if(money > LuckModePrice || reward)
        {
            if(money > LuckModePrice)
                UpdateMoney(money - LuckModePrice);

            enableLuckMode = true;
        }
        else
        {
            YandexGame.RewVideoShow(SaveData.LuckReward);
        }

        if(enableLuckMode)
        {
            LuckModeEnabled = true;
            RefreshLuckModeChance(SceneManager.GetActiveScene().buildIndex);

            foreach(var item in FindObjectsByType<Ingredient>(FindObjectsSortMode.None))
            {
                if(Random.value < LuckModeChance && !item.IsLuckIngredient)
                {
                    item.IsLuckIngredient = true;
                }
            }

            if(isGift)
            {
                UIController.LuckButton.interactable = false;
            }
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

        if(PlayerPrefs.GetInt(SaveData.MoneyKey) > IncomeModePrice || reward)
        {
            if(PlayerPrefs.GetInt(SaveData.MoneyKey) > IncomeModePrice)
                UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey) - IncomeModePrice);

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

        foreach(var item in FindObjectsByType<Trap>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            item.gameObject.SetActive(true);
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

        CurrentSkinID = skinIndex;
    }

    public void UpdateMusic(float newVolume)
    {
        _musicSource.volume = newVolume;
    }

    private void Rewarded(int id)
    {
        if(id == SaveData.LuckReward)
            EnableLuckMode(reward: true);

        else if(id == SaveData.IncomeReward)
            EnableIncomeMode(reward: true);
        
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

        foreach(var item in FindObjectsByType<Trap>(FindObjectsSortMode.None))
        {
            item.gameObject.SetActive(false);
        }
    }

    public void RefreshLuckModeChance(int luckLevel)
    {
        float chance = 0f;

        if(luckLevel > 1)
        {
            if(luckLevel <= 10) 
                chance = (luckLevel - 1) * 0.01f;

            else if(luckLevel <= 20)
                chance = 9 * 0.01f + (luckLevel - 10) * 0.008f;

            else if(luckLevel <= 50)
                chance = 9 * 0.01f + 10 * 0.008f + (luckLevel - 20) * 0.005f;
            else
                chance = 9 * 0.01f + 10 * 0.008f + 30 * 0.005f + (luckLevel - 50) * 0.001f;
        }

        if(LuckModeEnabled)
            chance *= LuckModeBonus;

        LuckModeChance = Mathf.Clamp(chance, 0f, 0.50f);
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
    public static string SoundKey { get; private set; } = "Sound";
    public static string MouseSensitivityKey { get; private set; } = "MouseSensitivity";
    public static string KeyboardSensitivityKey { get; private set; } = "KeyboardSensitivity";

    public static string LastSavedDateKey { get; private set; } = "LastSavedDate";
    public static string LastSavedStreakKey { get; private set; } = "LastSavedStreak";

    public static string SkinIdKey { get; private set; } = "SkinID";

    public static string LuckLevelKey { get; private set; } = "LuckLevel";

    public static int LuckReward { get; private set; } = 100;
    public static int IncomeReward { get; private set; } = 101;
    public static int BonusLevelReward { get; private set; } = 102;
    public static int FortuneWheelReward { get; private set; } = 103;
}