using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private GameObject _finalPanel;
    [SerializeField] private GameObject _dailyGiftPanel;
    [SerializeField] private GameObject _MinuteGiftPanel;

    [SerializeField] private HelpPointer _helpPointer;
    [SerializeField] private StarSystem _starSystem;
    [SerializeField] private DailyRewards _dailyRewards;
    [SerializeField] private MinuteGift _minuteGift;

    [SerializeField] private TMP_Text _foodText;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Image _recipeImage;
    [SerializeField] private Transform _recipeContent;

    private void Start()
    {
        _startPanel.SetActive(true);
        _finalPanel.SetActive(false);

        _helpPointer.Initialize();
        _dailyRewards.Initialize(_dailyGiftPanel);
        _minuteGift.ResetTimer();
    }

    void Update()
    {
        _minuteGift.UpdateTimer();
    }

    public void HideHelpPanel()
    {
        _startPanel.SetActive(false);
        _helpPointer.StopAnimation();
    }

    public void ShowMusicSlider() => _musicSlider.gameObject.SetActive(!_musicSlider.gameObject.activeSelf);

    public void SetGameMusic() => GameManager.Instance.GameMusic = _musicSlider.value;

    public void ShowDailyPanel() => _dailyGiftPanel.SetActive(!_dailyGiftPanel.activeSelf);

    public void ShowFinalPanel()
    {
        _finalPanel.SetActive(true);

        //FoodText

        int totalCount = GameManager.Instance.FinalIngredients.Count;
        string result = "";

        if(totalCount >= 1 && totalCount <= 3)
        {
            result = "Стейк";
        }
        else if(totalCount >= 4 && totalCount <= 15)
        {
            result = "Мега стейк";
        }
        else if(totalCount >= 16)
        {
            result = "Супер Мега стейк";
        }
        else
        {
            result = "Нет ингредиентов";
        }

        _foodText.text = result;

        //Stars

        _starSystem.ActivateStars();

        //Recipe
        var groupedIngredients = GameManager.Instance.FinalIngredients
            .GroupBy(ingredient => Regex.Replace(ingredient.name, @"\s*\(\d+\)$", ""))
            .OrderBy(group => group.Key);

        foreach(var group in groupedIngredients)
        {
            print($"Имя: {group.Key}, Количество: {group.Count()}");

            // Используем первый элемент группы для получения спрайта
            var representativeIngredient = group.First();
            Image image = Instantiate(_recipeImage, _recipeContent);
            image.sprite = representativeIngredient.Icon;
            image.GetComponentInChildren<TMP_Text>().text = group.Count().ToString();
        }
    }

    public void UpdateMoneyText(int newManey)
    {

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
}

[System.Serializable]
public class HelpPointer
{
    [Header("Help Pointer Settings")]
    [SerializeField] private Transform _helpPointer;
    [SerializeField] private Transform _pointerPos1;
    [SerializeField] private Transform _pointerPos2;
    [SerializeField] private float _pointerSpeed;

    private Tween moveTween;

    public void Initialize()
    {
        if(_helpPointer == null || _pointerPos1 == null || _pointerPos2 == null)
        {
            Debug.LogError("Null Reference");
            return;
        }


        _helpPointer.position = _pointerPos1.position;


        moveTween = _helpPointer.DOMove(_pointerPos2.position, _pointerSpeed)
                               .SetEase(Ease.Linear)
                               .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopAnimation()
    {
        if(moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
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
        CheckDailyLogin();
    }

    private void CheckDailyLogin()
    {
        string lastLoginStr = PlayerPrefs.GetString("LastLoginDate", "");

        if(string.IsNullOrEmpty(lastLoginStr))
        {
            _lastLoginDate = _today; 
            _currentStreak = 1;
            PlayerPrefs.SetString("LastLoginDate", _today.ToString("yyyy-MM-dd"));
            PlayerPrefs.SetInt("LoginStreak", _currentStreak);
            GiveReward(_currentStreak);
        }
        else
        {
            if(DateTime.TryParse(lastLoginStr, out _lastLoginDate))
            {
                TimeSpan difference = _today - _lastLoginDate;

                if(difference.TotalDays >= 1)
                {
                    if(difference.TotalDays == 1)
                    {
                        _currentStreak = PlayerPrefs.GetInt("LoginStreak", 1);

                        _currentStreak = (_currentStreak % _maxStreak) + 1;
                        PlayerPrefs.SetInt("LoginStreak", _currentStreak);
                        PlayerPrefs.SetString("LastLoginDate", _today.ToString("yyyy-MM-dd"));
                        GiveReward(_currentStreak);
                    }
                    else
                    {
                        _currentStreak = 1;
                        _lastLoginDate = _today;
                        PlayerPrefs.SetInt("LoginStreak", _currentStreak);
                        PlayerPrefs.SetString("LastLoginDate", _today.ToString("yyyy-MM-dd"));
                        GiveReward(_currentStreak);
                    }
                }
                else
                {
                    Debug.Log("Сегодня уже выполнен вход.");
                }
            }
            else
            {
                _lastLoginDate = _today;
                _currentStreak = 1;
                PlayerPrefs.SetString("LastLoginDate", _today.ToString("yyyy-MM-dd"));
                PlayerPrefs.SetInt("LoginStreak", _currentStreak);
                GiveReward(_currentStreak);
            }
        }
    }

    private void GiveReward(int day)
    {
        _dailyGiftPanel.SetActive(true);
        Debug.Log("Награда за день " + day);

        Reward[] reward = _giftContainer.GetComponentsInChildren<Reward>();

        reward[day - 1].ClaimReward();
    }

    public DateTime GetLastLogin()
    {
        return _lastLoginDate;
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