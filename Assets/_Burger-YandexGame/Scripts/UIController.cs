using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private GameObject _finalPanel;
    [SerializeField] private HelpPointer _helpPointer;
    [SerializeField] private StarSystem _starSystem;
    [SerializeField] private TMP_Text _foodText;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Transform _recipeContent;
    [SerializeField] private Image _recipeImage;

    private void Start()
    {
        _startPanel.SetActive(true);
        _helpPointer.Initialize();
    }

    public void HideHelpPanel()
    {
        if(_startPanel != null)
        {
            _startPanel.SetActive(false);
            _helpPointer.StopAnimation();
        }
    }

    public void ShowMusicSlider() => _musicSlider.gameObject.SetActive(!_musicSlider.gameObject.activeSelf);

    public void SetGameMusic() => GameManager.Instance.GameMusic = _musicSlider.value;

    public void ShowFinalPanel()
    {
        _finalPanel.SetActive(true);

        //FoodText

        int ingredient = GameManager.Instance.FinalIngredients.Count;

        int totalCount = ingredient;
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
        Dictionary<Ingredient, int> ingredientCount = GameManager.Instance.CalculateIngredientCount();

        foreach(var item in ingredientCount)
        {
            Image recipeImage = Instantiate(_recipeImage, _recipeContent);
            recipeImage.sprite = item.Key.Icon;
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