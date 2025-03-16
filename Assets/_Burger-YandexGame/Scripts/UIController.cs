using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private GameObject _finalPanel;

    [SerializeField] private HelpPointer _helpPointer;

    [SerializeField] private TMP_Text _foodText;

    [SerializeField] private Slider _musicSlider;

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

        if(ingredient != null)
        {
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
            Debug.Log(result);
        }        //Stars

        //Recipe
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