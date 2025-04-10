using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;

public class UIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private GameObject _finalPanel;
    [SerializeField] private GameObject _dailyGiftPanel;
    [SerializeField] private GameObject _bonusLevelPanel;
    [SerializeField] private GameObject _fortuneWheelPanel;
    [SerializeField] private GameObject _blur;

    [Header("Controllers")]
    [SerializeField] private HelpPointer _helpPointer;
    [SerializeField] private StarSystem _starSystem;
    [SerializeField] private DailyRewards _dailyRewards;
    [SerializeField] private MinuteGift _minuteGift;
    [SerializeField] private FinalReward _finalReward;
    [SerializeField] private FortuneWheel _fortuneWheel;

    [Header("Left Buttons")]
    [SerializeField] private GameObject _fortuneWheelButton;
    [SerializeField] private GameObject _minuteGiftButton;
    [SerializeField] private GameObject _dailyGiftButton;

    [Header("Other References")]
    [SerializeField] private GameObject _shopButton;
    [SerializeField] private GameObject _recipeImage;
    [SerializeField] private GameObject _playerHelp;

    [Header("Texts")]
    [SerializeField] private TMP_Text _foodText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _fortuneWheelText;
    [SerializeField] private TMP_Text _dailyGiftText;
    [SerializeField] private TMP_Text _moneyText;

    [Header("Sliders")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sensitivitySlider;

    [Header("Transforms")]
    [SerializeField] private Transform _recipeContent;
    [SerializeField] private Transform _recipeContainer;

    [field: Header("Buttons")]
    [field: SerializeField] public Button IncomeButton { get; private set; }
    [field: SerializeField] public Button LuckButton { get; private set; }

    private bool setAlreadyRecipe;
    [SerializeField] private List<ShopCard> _shopCards;


    private void Start()
    {
        int buildIndex = SceneManager.GetActiveScene().buildIndex;

        _startPanel.SetActive(true);
        _finalPanel.SetActive(false);

        _fortuneWheelButton.SetActive(false);
        _dailyGiftButton.SetActive(false);
        _shopButton.SetActive(false);

        if(buildIndex == 0)
        {
            _helpPointer.Initialize();
        }
        else
        {
            _playerHelp.SetActive(false);
        }

        if(buildIndex > 0)
        {
            _fortuneWheelButton.SetActive(true);
            _shopButton.SetActive(true);

            if(buildIndex == 1)
            {
                FocusObject(_fortuneWheelButton.transform);
                FocusObject(_shopButton.transform);
            }
        }

        if(buildIndex > 2)
        {
            _dailyGiftButton.SetActive(true);
            _dailyRewards.Initialize(_dailyGiftPanel);

            if(buildIndex == 3)
            {
                FocusObject(_dailyGiftButton.transform);
            }
        }

        if(buildIndex % 6 == 0 && buildIndex != 0)
            HideHelpPanel();

        _moneyText.text = PlayerPrefs.GetInt(SaveData.MoneyKey).ToString();
        _minuteGift.ResetTimer();
        UpdateFortuneWheelText(PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey).ToString());


        // Spawn ShopCard for Skins
        //_shopCards.Add();
    }

    private void Update()
    {
        _minuteGift.UpdateTimer();
        _dailyGiftText.text = _dailyRewards.GetTimeUntilNextGift().ToString(@"hh\:mm\:ss");

        _fortuneWheel.SpinFortuneWheel();
    }

    public void HideHelpPanel()
    {
        _startPanel.SetActive(false);
        _helpPointer.StopAnimation();

        _fortuneWheelButton.SetActive(false);
        _minuteGiftButton.SetActive(false);
        _dailyGiftButton.SetActive(false);
    }

    public void ShowMusicSlider() => _musicSlider.gameObject.SetActive(!_musicSlider.gameObject.activeSelf);

    public void SetGameMusic() => PlayerPrefs.SetFloat(SaveData.MusicKey, _musicSlider.value);

    public void SetSensitivity() => PlayerPrefs.SetFloat(SaveData.SensitivityKey, _sensitivitySlider.value);

    public void ShowPanel(GameObject panel) => panel.SetActive(!panel.activeSelf);

    public void EnableIncomeMode(Button button)
    {
        button.interactable = false;
        GameManager.Instance.EnableIncomeMode();
    }

    public void EnableLuckMode(Button button)
    {
        button.interactable = false;
        GameManager.Instance.EnableLuckMode();
    }
    
    public void ShowFinalPanel()
    {
        _finalPanel.SetActive(true);

        int totalCount = GameManager.Instance.FinalIngredients.Count;
        string result;
        if(totalCount == 0)
            result = "Нет ингредиентов";
        else if(totalCount <= 3)
            result = "Стейк";
        else if(totalCount <= 15)
            result = "Мега стейк";
        else
            result = "Супер Мега стейк";
        _foodText.text = result;

        _starSystem.ActivateStars();

        var finalIngredients = GameManager.Instance.FinalIngredients;
        var groupedIngredients = finalIngredients
            .GroupBy(ingredient => Regex.Replace(ingredient.name, @"\s*\(\d+\)$", ""))
            .OrderBy(group => group.Key)
            .ToList();

        // Переменные для накопления суммы денег
        int addedMoneyGood = 0;
        int addedMoneyBad = 0;
        int addedMoneyRecipe = 0;

        foreach(var group in groupedIngredients)
        {
            var representativeIngredient = group.First();
            GameObject image = Instantiate(_recipeImage, _recipeContent);

            Image uiIngredientImage = null;
            Image uiBackground = null;
            TMP_Text countText = image.GetComponentInChildren<TMP_Text>();

            foreach(Transform child in image.transform)
            {
                if(child.CompareTag("UIIngredientImage"))
                    uiIngredientImage = child.GetComponent<Image>();
                else if(child.CompareTag("UIBackground"))
                    uiBackground = child.GetComponent<Image>();
            }

            if(uiIngredientImage != null)
                uiIngredientImage.sprite = representativeIngredient.Icon;

            // Флаг, показывающий, что ингредиент входит в рецепт
            bool isInRecipe = false;
            // Проверяем, присутствует ли ингредиент в рецепте
            foreach(var recipeItem in GameManager.Instance.Recipe)
            {
                if(representativeIngredient.Icon == recipeItem.Ingredient.Icon)
                {
                    isInRecipe = true;
                    if(group.Count() >= recipeItem.Count)
                        uiBackground.color = Color.green;
                    else
                        uiBackground.color = Color.yellow;
                    break;
                }
            }

            if(countText != null)
                countText.text = group.Count().ToString();

            if(isInRecipe)
            {
                addedMoneyRecipe += group.Count() * GameManager.Instance.RecipeIngredientPrice;

                if(representativeIngredient.IsLuckIngredient)
                    addedMoneyGood += group.Count() * (int)Math.Round(GameManager.Instance.GoodIngredientPrice * GameManager.Instance.LuckMultiply);
            }
            else
            {
                if(representativeIngredient.IsBadIngredient)
                    addedMoneyBad += group.Count() * GameManager.Instance.BadIngredientPrice;
                else if(representativeIngredient.IsLuckIngredient)
                    addedMoneyGood += group.Count() * (int)Math.Round(GameManager.Instance.GoodIngredientPrice * GameManager.Instance.LuckMultiply);
                else
                    addedMoneyGood += group.Count() * GameManager.Instance.GoodIngredientPrice;
            }
        }

        var requiredIcons = GameManager.Instance.Recipe.Select(item => item.Ingredient.Icon).ToList();
        var addedIcons = groupedIngredients.Select(g => g.First().Icon).Distinct().ToList();
        var missingIcons = requiredIcons.Except(addedIcons).ToList();

        foreach(var icon in missingIcons)
        {
            GameObject image = Instantiate(_recipeImage, _recipeContent);

            Image uiIngredientImage = null;
            Image uiBackground = null;
            TMP_Text countText = image.GetComponentInChildren<TMP_Text>();

            foreach(Transform child in image.transform)
            {
                if(child.CompareTag("UIIngredientImage"))
                    uiIngredientImage = child.GetComponent<Image>();
                else if(child.CompareTag("UIBackground"))
                    uiBackground = child.GetComponent<Image>();
            }

            if(uiIngredientImage != null)
                uiIngredientImage.sprite = icon;
            if(uiBackground != null)
                uiBackground.color = Color.red;
            if(countText != null)
                countText.text = "0";
        }

        // Сохраняем базовую сумму до применения множителей
        int baseMoney = addedMoneyGood + addedMoneyBad + addedMoneyRecipe;

        // Если нужно обновить деньги до применения множителей, можно это сделать здесь

        int totalMoneyToAdd = baseMoney;
        int incomeBonus = 0;
        int luckBonus = 0;
        int bonusLevel = 0;

        // Применяем множитель income
        if(GameManager.Instance.IncomeModeEnabled)
        {
            int newTotal = (int)Math.Round(totalMoneyToAdd * GameManager.Instance.IncomeMultiply);
            incomeBonus = newTotal - totalMoneyToAdd;
            totalMoneyToAdd = newTotal;
        }

        // Применяем множитель luck
        if(GameManager.Instance.LuckModeEnabled)
        {
            int bonus = (int)Math.Round(totalMoneyToAdd * GameManager.Instance.LuckMultiply);
            luckBonus = bonus;
            totalMoneyToAdd += bonus;
        }
        
        // Применяем множитель luck
        if(GameManager.Instance.BonusLevelEnabled)
        {
            int bonus = (int)Math.Round(totalMoneyToAdd * GameManager.Instance.BonusLevelMultiply);
            bonusLevel = bonus;
            totalMoneyToAdd += bonus;
        }

        // Выводим в debug отдельно базовую сумму, бонусы income и luck, а также итоговую сумму
        Debug.Log($"Деньги за хорошие: {addedMoneyGood}, плохие: {addedMoneyBad}, из рецепта: {addedMoneyRecipe}. " +
                  $"Базовая сумма: {baseMoney}, бонус дохода: {incomeBonus}, бонус удачи: {luckBonus}, бонусный уровень: {bonusLevel}. Всего: {totalMoneyToAdd}");

        GameManager.Instance.UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey) + totalMoneyToAdd);

    }

    public void UpdateMoneyText(int newMoney)
    {
        int currentMoney = int.Parse(_moneyText.text);

        DOTween.To(() => currentMoney, x =>
        {
            currentMoney = x;
            _moneyText.text = currentMoney.ToString();
        }, newMoney, 0.5f).SetEase(Ease.OutCubic);
    }

    public void GetMinuteGift()
    {
        if(!_minuteGift.TimerIsRunning)
        {
            GameManager.Instance.UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey) + _minuteGift.GiftMoney);
            _minuteGift.ResetTimer();
        }
    }

    public void LoadNextScene()
    {
        DOTween.KillAll();
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextIndex);
    }

    public void ReloadScene()
    {
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowFinalRewardPanel()
    {
        _finalPanel.SetActive(false);
        _finalReward.Initialize();
    }

    public void GetRewardMultiply() => _finalReward.GetRewardMultiply();

    public void UpdateSceneText(string newLevel)
    {
        //Localization

        if(true)
        {
            _levelText.text = "Level: " + newLevel;
        }
    }

    public void SetRecipe()
    {
        if(setAlreadyRecipe)
            return;

        foreach(var item in GameManager.Instance.Recipe)
        {
            GameObject image = Instantiate(_recipeImage, _recipeContainer);

            foreach(Transform child in image.transform)
            {
                if(child.CompareTag("UIIngredientImage"))
                {
                    child.GetComponent<Image>().sprite = item.Ingredient.Icon;
                    break;
                }
            }

            image.GetComponentInChildren<TMP_Text>().text = item.Count.ToString();
        }

        setAlreadyRecipe = true;
    }

    public void SpinFortuneWheel()
    {
        if(PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) > 0)
        {
            PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) - 1);

            UpdateFortuneWheelText(PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey).ToString());
            _fortuneWheel.StartSpin();
        }

        else
        {
            UpdateFortuneWheelText(PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey).ToString());
        }
    }

    public void UpdateFortuneWheelText(string newFortune) => _fortuneWheelText.text = newFortune + "/" + "3";

    public void ProposeBonusLevel()
    {
        _bonusLevelPanel.SetActive(true);
        _bonusLevelPanel.transform.localScale = Vector3.zero;
        _bonusLevelPanel.transform.DOScale(Vector3.one, 0.2f);

    }

    public void EnableBonusLevel()
    {
        YandexGame.RewVideoShow(SaveData.BonusLevelReward);
    }

    public void HideBonusPanel() => _bonusLevelPanel.SetActive(false);

    public void SkipBonusLevelLevel() => GameManager.Instance.SkipBonusLevelLevel();

    public void ResetAllButtons()
    {
        foreach(var item in _shopCards)
        {
            if(GameManager.Instance.PurchasedSkins.Contains(item.SkinID))
            {
                item.CardButton.interactable = true;
                item.CardButtonText.text = "Select";
            }

        }
    }

    private void FocusObject(Transform focusObject)
    {
        Sequence sequence = DOTween.Sequence();

        Image image = focusObject.GetComponent<Image>();
        if(image != null)
        {
            Color tempColor = image.color;
            tempColor.a = 0;
            image.color = tempColor;

            sequence.Append(image.DOFade(1, 0.5f));
        }

        focusObject.localScale = Vector3.zero;
        sequence.Join(focusObject.DOScale(1f, 0.5f).SetEase(Ease.OutBack));

        sequence.Append(focusObject.DOScale(1.1f, 0.2f));
        sequence.Append(focusObject.DOScale(1f, 0.2f));
    }
}

[System.Serializable]
public class HelpPointer
{
    [Header("Help Pointer Settings")]
    [SerializeField] private Transform _helpPointer;
    [SerializeField] private Transform _pointerPos1;
    [SerializeField] private Transform _pointerPos2;
    [SerializeField] private float _pointerSpeed;

    private Tween _moveTween;

    public void Initialize()
    {
        if(_helpPointer == null || _pointerPos1 == null || _pointerPos2 == null)
        {
            Debug.LogError("Null Reference");
            return;
        }


        _helpPointer.position = _pointerPos1.position;


        _moveTween = _helpPointer.DOMove(_pointerPos2.position, _pointerSpeed)
                               .SetEase(Ease.Linear)
                               .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopAnimation()
    {
        if(_moveTween != null && _moveTween.IsActive())
        {
            _moveTween.Kill();
        }
    }
}

[System.Serializable]
public class StarSystem
{
    [SerializeField] private GameObject[] _stars;

    [SerializeField] private float _threshold1 = 0.33f;
    [SerializeField] private float _threshold2 = 0.66f;
    [SerializeField] private float _threshold3 = 1f;

    [SerializeField] private float _starsSpeed = 1f;

    private void ShowStar(GameObject star)
    {
        Vector3 startScale = star.transform.localScale;

        star.SetActive(true);

        star.transform.localScale = Vector3.zero;
        star.transform.DOScale(startScale, _starsSpeed).SetEase(Ease.OutBack);
    }

    public void ActivateStars()
    {
        float collectedPercentage = (float)GameManager.Instance.FinalIngredients.Count / GameManager.Instance.TotalIngredientsCount;

        if(collectedPercentage >= _threshold1 && !_stars[0].activeSelf)
        {
            ShowStar(_stars[0]);
        }
        if(collectedPercentage >= _threshold2 && !_stars[1].activeSelf)
        {
            ShowStar(_stars[1]);
        }
        if(collectedPercentage >= _threshold3 && !_stars[2].activeSelf)
        {
            ShowStar(_stars[2]);
        }
    }
}

[System.Serializable]
public class DailyRewards
{
    [SerializeField] private Transform _giftContainer;

    private GameObject _dailyGiftPanel;

    // Вместо DateTime.Date — храним полное время:
    private DateTime _lastLoginDateTime;
    private int _maxStreak;
    private int _currentStreak;

    /// <summary>
    /// Инициализация класса: загрузка сохранённых значений из YandexGame в PlayerPrefs, проверка наград.
    /// </summary>
    public void Initialize(GameObject dailyGiftPanel)
    {
        // Укажем, сколько всего "дней" (подарков) доступно:
        _maxStreak = _giftContainer.childCount;
        _dailyGiftPanel = dailyGiftPanel;

        // Синхронизация с YandexGame:
        LoadData();

        // Проверка — нужно ли дать награду:
        CheckDailyLogin();
    }

    /// <summary>
    /// Основная логика выдачи награды (раз в 24 часа).
    /// </summary>
    private void CheckDailyLogin()
    {
        // Получаем СТРОКУ даты/времени последнего захода:
        string lastLoginStr = PlayerPrefs.GetString(SaveData.LastSavedDateKey);

        // Поскольку она "не может быть пустой" по условию, проверим только парсинг.
        if(!DateTime.TryParse(lastLoginStr, out _lastLoginDateTime))
        {
            // Если парсинг не удался => считаем, что это первый вход

            _currentStreak = 1;
            GiveReward(_currentStreak);

            // Сохраняем новую дату (текущее время) и стрик:
            SaveDate(DateTime.Now, _currentStreak);
        }
        else
        {
            // Парсинг даты прошёл успешно

            // Вычисляем, сколько часов прошло с последнего получения награды
            TimeSpan difference = DateTime.Now - _lastLoginDateTime;

            if(difference.TotalHours >= 24)
            {
                // Прошло 24 часа — значит можно получить подарок
                _currentStreak = PlayerPrefs.GetInt(SaveData.LastSavedStreakKey, 1);

                // Увеличиваем стрик (по кругу, если хотите возвращать на 1 после maxStreak)
                _currentStreak = (_currentStreak % _maxStreak) + 1;

                GiveReward(_currentStreak);
                SaveDate(DateTime.Now, _currentStreak);
            }
            else
            {
                // Меньше 24 часов — награду ещё рано выдавать
                Debug.Log("[DailyRewards] Ещё не прошло 24 часа с последнего получения награды!");
            }
        }
    }

    /// <summary>
    /// Выдача награды за конкретный "день" (стрик).
    /// </summary>
    private void GiveReward(int day)
    {
        // Открываем панель награды
        if(_dailyGiftPanel != null)
        {
            _dailyGiftPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[DailyRewards] _dailyGiftPanel не назначен!");
        }

        // Получаем все объекты Reward
        Reward[] rewards = _giftContainer.GetComponentsInChildren<Reward>();
        if(rewards == null || rewards.Length == 0)
        {
            Debug.LogWarning("[DailyRewards] Не найдено ни одного Reward в _giftContainer!");
            return;
        }

        int index = day - 1;
        if(index < 0 || index >= rewards.Length)
        {
            Debug.LogWarning($"[DailyRewards] Индекс {index} выходит за границы массива Rewards (длина: {rewards.Length}).");
            return;
        }

        // Активируем (Claim) нужную награду
        rewards[index].ClaimReward();
    }

    /// <summary>
    /// Возвращаем, сколько осталось до следующей награды, если прошло меньше 24 часов.
    /// Если время вышло, возвращаем TimeSpan.Zero.
    /// </summary>
    public TimeSpan GetTimeUntilNextGift()
    {
        // Тут также не проверяем IsNullOrEmpty, сразу берём строку
        string lastLoginStr = PlayerPrefs.GetString(SaveData.LastSavedDateKey);

        // Пробуем парсить
        if(!DateTime.TryParse(lastLoginStr, out DateTime lastLogin))
        {
            // Если не распарсили — считаем, что можем выдать награду прямо сейчас
            return TimeSpan.Zero;
        }

        // Следующее получение награды — через 24 часа после последнего
        DateTime nextGiftTime = lastLogin.AddHours(24);

        TimeSpan timeLeft = nextGiftTime - DateTime.Now;
        return (timeLeft.TotalSeconds > 0) ? timeLeft : TimeSpan.Zero;
    }

    /// <summary>
    /// Сохраняем дату последнего входа (полное время) и значение стрика.
    /// </summary>
    private void SaveDate(DateTime dateTime, int streak)
    {
        string dateString = dateTime.ToString("O"); // ISO 8601

        PlayerPrefs.SetString(SaveData.LastSavedDateKey, dateString);
        PlayerPrefs.SetInt(SaveData.LastSavedStreakKey, streak);

        // Сохраняем и во внутреннюю систему YandexGame
        YandexGame.savesData.LastSavedDate = dateString;
        YandexGame.savesData.LastSavedStreak = streak;

        YandexGame.SaveProgress();
    }

    /// <summary>
    /// Синхронизация данных из YandexGame.savesData в PlayerPrefs при старте.
    /// </summary>
    private void LoadData()
    {
        // Если в YandexGame.savesData есть сохранённая строка, пишем её в PlayerPrefs
        if(!string.IsNullOrEmpty(YandexGame.savesData.LastSavedDate))
        {
            PlayerPrefs.SetString(SaveData.LastSavedDateKey, YandexGame.savesData.LastSavedDate);
        }

        if(YandexGame.savesData.LastSavedStreak > 0)
        {
            PlayerPrefs.SetInt(SaveData.LastSavedStreakKey, YandexGame.savesData.LastSavedStreak);
        }
    }
}

[System.Serializable]
public class MinuteGift
{
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private Button _getButton;
    [SerializeField] private float _maxTimeRemaining = 600f;
    [field: SerializeField] public int GiftMoney { get; private set; }

    [HideInInspector] public bool TimerIsRunning { get; private set; } = true;

    private float _timeRemaining;

    public void UpdateTimer()
    {
        if(TimerIsRunning)
        {
            if(_timeRemaining > 0)
            {
                _timeRemaining -= Time.deltaTime;
                DisplayTime(_timeRemaining);
            }
            else
            {
                _timeRemaining = 0;
                TimerIsRunning = false;
                DisplayTime(_timeRemaining);
                _getButton.interactable = true;
            }
        }
    }

    public void ResetTimer()
    {
        _timeRemaining = _maxTimeRemaining;
        TimerIsRunning = true;
    }

    private void DisplayTime(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);
        _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}

[System.Serializable]
public class FinalReward
{
    [SerializeField] private GameObject _rewardPanel;
    [SerializeField] private Transform _arrowPivot;

    [SerializeField] private float _speed = 1f;
    [SerializeField] private Vector3 _startPoint;
    [SerializeField] private Vector3 _endPoint;

    [SerializeField] private List<RewardMultiply> _rewardMultiply;

    private Tween _moveTween;

    public void Initialize()
    {
        _rewardPanel.SetActive(true);

        _arrowPivot.eulerAngles = _startPoint;

        _moveTween = _arrowPivot
            .DORotate(_endPoint, _speed)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void GetRewardMultiply()
    {
        StopAnimation();

        int chosenMultiply = 1;

        chosenMultiply = GetAngleBasedMultiply();

        Debug.Log($"Выбран множитель: x{chosenMultiply}");

    }

    private void StopAnimation()
    {
        if(_moveTween != null && _moveTween.IsActive())
        {
            _moveTween.Kill();
        }
    }

    private float NormalizeAngle(float angle)
    {
        angle = (angle + 180f) % 360f - 180f;
        return angle;
    }

    private int GetAngleBasedMultiply()
    {
        float currentAngle = NormalizeAngle(_arrowPivot.eulerAngles.z);

        float minAngle = _startPoint.z;
        float maxAngle = _endPoint.z;

        if(minAngle > maxAngle)
        {
            float temp = minAngle;
            minAngle = maxAngle;
            maxAngle = temp;
        }

        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        float t = (currentAngle - minAngle) / (maxAngle - minAngle);

        int index = Mathf.FloorToInt(t * _rewardMultiply.Count);
        if(index >= _rewardMultiply.Count)
            index = _rewardMultiply.Count - 1;

        return _rewardMultiply[index].Multiply;
    }

    [System.Serializable]
    private struct RewardMultiply
    {
        [field: SerializeField]
        public int Multiply { get; private set; }
    }
}

[System.Serializable]
public class FortuneWheel
{
    [SerializeField] private GameObject _wheel;
    [SerializeField] private FortuneWheelDetector _fortuneWheelDetector;
    [SerializeField] private float _speed;
    [SerializeField] private float _currentSpeed;

    private bool _spinStarted;

    public void StartSpin()
    {
        _spinStarted = true;
        _currentSpeed = _speed;

        Debug.Log("spin");
    }

    public void SpinFortuneWheel()
    {
        if(_currentSpeed > 0 && _spinStarted)
        {
            _wheel.transform.Rotate(_wheel.transform.forward * -_currentSpeed);
            _currentSpeed -= Time.deltaTime;
        }

        else if(_spinStarted)
        {
            _fortuneWheelDetector.IsStoped = true;
            _spinStarted = false;
        }
    }
}