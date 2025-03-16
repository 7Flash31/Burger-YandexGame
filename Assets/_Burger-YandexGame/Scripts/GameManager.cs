using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public Player Player { get; private set; }
    [field: SerializeField] public UIController UIController { get; private set; }

    [SerializeField] private HeadController _headController;

    public float GameMusic { get; set; }
    public bool GameLaunch { get; private set; }
    public List<Ingredient> FinalIngredients { get; set; } = new List<Ingredient>();
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        DOTween.Init();
        Player.enabled = false;


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

    private void Update()
    {
        print(FinalIngredients.Count);
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
    }

    //public Dictionary<IngredientType, int> CalculateRecipe()
    //{
    //    var ingredientCounts = new Dictionary<IngredientType, int>();

    //    foreach(var ingredient in FinalIngredients)
    //    {
    //        if(ingredientCounts.ContainsKey(ingredient.IngredientInfo.IngredientType))
    //            ingredientCounts[ingredient.IngredientInfo.IngredientType]++;
    //        else
    //            ingredientCounts[ingredient.IngredientInfo.IngredientType] = 1;
    //    }
    //    print(ingredientCounts.Count);
    //    return ingredientCounts;
    //}
}

public enum IngredientType
{
    Meat,
    Pepper,
    Cheese,
    Tomato,
    Fish,
    Avocado,
    Egg,
    Bacon,
    Cutlet,
    Salad,
}