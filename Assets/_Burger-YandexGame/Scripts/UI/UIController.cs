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
    //[SerializeField] private GameObject _MinuteGiftPanel;

    [Header("Controllers")]
    [SerializeField] private HelpPointer _helpPointer;
    [SerializeField] private StarSystem _starSystem;
    [SerializeField] private DailyRewards _dailyRewards;
    [SerializeField] private MinuteGift _minuteGift;
    [SerializeField] private FinalReward _finalReward;

    [Header("Left Buttons")]
    [SerializeField] private GameObject _fortuneWheelButton;
    [SerializeField] private GameObject _minuteGiftButton;
    [SerializeField] private GameObject _dailyGiftButton;

    [Header("Other Reference")]
    [SerializeField] private GameObject _shopButton;
    [SerializeField] private GameObject _recipeImage;
    [SerializeField] private GameObject _playerHelp;

    [SerializeField] private TMP_Text _foodText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _fortuneWheel;
    [SerializeField] private TMP_Text _dailyGiftText;
    [SerializeField] private TMP_Text _moneyText;

    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sensitivitySlider;

    [SerializeField] private Transform _recipeContent;
    [SerializeField] private Transform _recipeContainer;

    private bool setAlreadyRecipe;

    //private void Start()
    //{
    //    _startPanel.SetActive(true);
    //    _finalPanel.SetActive(false);

    //    _fortuneWheelButton.SetActive(false);
    //    _dailyGiftButton.SetActive(false);
    //    _shopButton.SetActive(false);

    //    if(SceneManager.GetActiveScene().buildIndex == 0)
    //        _helpPointer.Initialize();
    //    else
    //        _playerHelp.SetActive(false);

    //    if(SceneManager.GetActiveScene().buildIndex > 0)
    //    {
    //        _fortuneWheelButton.SetActive(true);
    //        _shopButton.SetActive(true);

    //        if(SceneManager.GetActiveScene().buildIndex == 1)
    //        {
    //            FocusObject(_fortuneWheelButton.transform);
    //            FocusObject(_shopButton.transform);
    //        }
    //    }

    //    if(SceneManager.GetActiveScene().buildIndex > 2)
    //    {
    //        _dailyGiftButton.SetActive(true);
    //        _dailyRewards.Initialize(_dailyGiftPanel);

    //        if(SceneManager.GetActiveScene().buildIndex == 3)
    //        {
    //            FocusObject(_dailyGiftButton.transform);
    //        }
    //    }

    //    _minuteGift.ResetTimer();
    //}

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

        _moneyText.text = PlayerPrefs.GetInt(SaveData.MoneyKey).ToString();
        _minuteGift.ResetTimer();
    }

    private void Update()
    {
        _minuteGift.UpdateTimer();
        _dailyGiftText.text = _dailyRewards.GetTimeUntilNextGift().ToString(@"hh\:mm\:ss");
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

    //public void ShowDailyPanel() => _dailyGiftPanel.SetActive(!_dailyGiftPanel.activeSelf);

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

            // Расчёт денег для данной группы ингредиентов
            if(isInRecipe)
            {
                // Для ингредиентов, входящих в рецепт, используем RecipeIngredientPrice
                addedMoneyRecipe += group.Count() * GameManager.Instance.RecipeIngredientPrice;
            }
            else
            {
                // Определяем, является ли ингредиент плохим через свойство IsBadIngredient
                if(representativeIngredient.IsBadIngredient)
                    addedMoneyBad += group.Count() * GameManager.Instance.BadIngredientPrice;
                else
                    addedMoneyGood += group.Count() * GameManager.Instance.GoodIngredientPrice;
            }
        }

        // Обработка недостающих ингредиентов рецепта (вывод с красным фоном и количеством "0")
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

        int totalMoneyToAdd = addedMoneyGood + addedMoneyBad + addedMoneyRecipe;
        GameManager.Instance.UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey) + totalMoneyToAdd);

        if(GameManager.Instance.IncomeModeEnabled)
        {
            totalMoneyToAdd = (int)Math.Round(totalMoneyToAdd * GameManager.Instance.IncomeMultiply);
        }

        Debug.Log($"Деньги за хорошие: {addedMoneyGood}, плохие: {addedMoneyBad}, из рецепта: {addedMoneyRecipe}. Всего: {totalMoneyToAdd}");
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
            GameManager.Instance.UpdateMoney(_minuteGift.GiftMoney);
            _minuteGift.ResetTimer();

            print(123);
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

            //foreach(Transform child in image.transform)
            //{
            //    if(child.CompareTag("UIBackground"))
            //    {
            //        child.GetComponent<Image>().sprite = item.Ingredient.Icon;
            //        break;
            //    }
            //}

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
        GameManager.Instance.SpinFortuneWheel();
    }

    public void UpdateFortuneWheelText(string newFortune) => _fortuneWheel.text = newFortune + "/" + "3";

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

    private DateTime _lastLoginDate;
    private DateTime _today;
    private int _maxStreak;
    private int _currentStreak;

    public void Initialize(GameObject dailyGiftPanel)
    {
        _today = DateTime.Now.Date;
        _maxStreak = _giftContainer.childCount;
        _dailyGiftPanel = dailyGiftPanel;

        LoadData();
        CheckDailyLogin();
    }

    private void CheckDailyLogin()
    {
        string lastLoginStr = PlayerPrefs.GetString(SaveData.LastSavedDateKey);

        if(string.IsNullOrEmpty(lastLoginStr))
        {
            _lastLoginDate = _today;
            _currentStreak = 1;
            string todayStr = _today.ToString("yyyy-MM-dd");
            PlayerPrefs.SetString(SaveData.LastSavedDateKey, todayStr);
            PlayerPrefs.SetInt(SaveData.LastSavedStreakKey, _currentStreak);
            SaveDate(todayStr, _currentStreak);
            GiveReward(_currentStreak);
        }
        else
        {
            if(DateTime.TryParse(lastLoginStr, out _lastLoginDate))
            {
                TimeSpan difference = _today - _lastLoginDate;

                if(difference.TotalDays >= 1)
                {
                    string todayStr = _today.ToString("yyyy-MM-dd");

                    if(difference.TotalDays == 1)
                    {
                        _currentStreak = PlayerPrefs.GetInt(SaveData.LastSavedStreakKey, 1);
                        _currentStreak = (_currentStreak % _maxStreak) + 1;
                        PlayerPrefs.SetInt(SaveData.LastSavedStreakKey, _currentStreak);
                        PlayerPrefs.SetString(SaveData.LastSavedDateKey, todayStr);
                        SaveDate(todayStr, _currentStreak);
                        GiveReward(_currentStreak);
                    }
                    else
                    {
                        _currentStreak = 1;
                        _lastLoginDate = _today;
                        PlayerPrefs.SetInt(SaveData.LastSavedStreakKey, _currentStreak);
                        PlayerPrefs.SetString(SaveData.LastSavedDateKey, todayStr);
                        SaveDate(todayStr, _currentStreak);
                        GiveReward(_currentStreak);
                    }
                }
            }
            else
            {
                _lastLoginDate = _today;
                _currentStreak = 1;
                string todayStr = _today.ToString("yyyy-MM-dd");
                PlayerPrefs.SetString(SaveData.LastSavedDateKey, todayStr);
                PlayerPrefs.SetInt(SaveData.LastSavedStreakKey, _currentStreak);
                SaveDate(todayStr, _currentStreak);
                GiveReward(_currentStreak);
            }
        }
    }

    private void GiveReward(int day)
    {
        _dailyGiftPanel.SetActive(true);

        Reward[] reward = _giftContainer.GetComponentsInChildren<Reward>();

        reward[day - 1].ClaimReward();
    }

    public TimeSpan GetTimeUntilNextGift()
    {
        string lastLoginStr = PlayerPrefs.GetString(SaveData.LastSavedDateKey, "");
        if(string.IsNullOrEmpty(lastLoginStr))
        {
            return TimeSpan.Zero;
        }

        if(!DateTime.TryParse(lastLoginStr, out DateTime lastLogin))
        {
            return TimeSpan.Zero;
        }

        DateTime nextGiftTime = lastLogin.AddDays(1);
        TimeSpan timeLeft = nextGiftTime - DateTime.Now;
        return timeLeft.TotalSeconds > 0 ? timeLeft : TimeSpan.Zero;
    }

    public void SaveDate(string date, int streak)
    {
        PlayerPrefs.SetString(SaveData.LastSavedDateKey, date);
        PlayerPrefs.SetInt(SaveData.LastSavedStreakKey, streak);

        YandexGame.savesData.LastSavedDate = date;
        YandexGame.savesData.LastSavedStreak = streak;

        YandexGame.SaveProgress();
    }

    public void LoadData()
    {
        PlayerPrefs.SetString(SaveData.LastSavedDateKey, YandexGame.savesData.LastSavedDate);
        PlayerPrefs.SetInt(SaveData.LastSavedStreakKey, YandexGame.savesData.LastSavedStreak);
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
        _getButton.interactable = false;
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