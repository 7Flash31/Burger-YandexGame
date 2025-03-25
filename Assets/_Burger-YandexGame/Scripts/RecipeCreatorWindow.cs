using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;

public class RecipeCreatorWindow : EditorWindow
{
    private string levelName = "";

    private string ingredientsFolder = "Assets\\_Burger-YandexGame\\Prefabs\\Ingredient";
    private string imagesFolder = "Assets\\_Burger-YandexGame\\Sprites\\IngredientsIcon";
    private string saveRecipeDataPath = "Assets\\_Burger-YandexGame\\Recipes";

    private List<GameObject> availablePrefabs = new List<GameObject>();
    private GUIContent[] availablePrefabContents = new GUIContent[0];

    [System.Serializable]
    public class RecipeEntry
    {
        public int selectedIndex = 0;
        public int count = 1;
    }

    private List<RecipeEntry> recipeEntries = new List<RecipeEntry>();

    [MenuItem("Tools/Recipe Creator")]
    public static void ShowWindow()
    {
        GetWindow<RecipeCreatorWindow>("Recipe Creator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Customizing a recipe for a level", EditorStyles.boldLabel);

        levelName = EditorGUILayout.TextField("Level Name", levelName);
        if(GUILayout.Button("Use the name of the active scene"))
        {
            levelName = SceneManager.GetActiveScene().name;
        }

        ingredientsFolder = EditorGUILayout.TextField("Prefab folder", ingredientsFolder);
        imagesFolder = EditorGUILayout.TextField("Image folder", imagesFolder);

        UpdateAvailablePrefabs();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Recipe", EditorStyles.boldLabel);

        for(int i = 0; i < recipeEntries.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            DrawPrefabDropdown(i);

            recipeEntries[i].count = EditorGUILayout.IntField(recipeEntries[i].count, GUILayout.Width(50));

            if(GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                recipeEntries.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if(GUILayout.Button("Add Ingredient"))
        {
            recipeEntries.Add(new RecipeEntry());
        }

        EditorGUILayout.Space();

        if(GUILayout.Button("Save RecipeData"))
        {
            CreateOrUpdateRecipeData();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scene", EditorStyles.boldLabel);

        if(GUILayout.Button("Open Past Scene"))
        {
            var newScene = SceneManager.GetActiveScene().buildIndex - 1;
            var path = "Assets\\_Burger-YandexGame\\Scenes\\Levels\\";
            if(newScene == 0)
            {
                path = "Assets\\_Burger-YandexGame\\Scenes\\Levels\\Level" + ".unity";
                levelName = "Level";
            }
            else
            {
                path = "Assets\\_Burger-YandexGame\\Scenes\\Levels\\Level " + newScene + ".unity";
                levelName = "Level " + newScene.ToString();
            }
            EditorSceneManager.OpenScene(path);
        }

        if(GUILayout.Button("Open Next Scene"))
        {
            var newScene = SceneManager.GetActiveScene().buildIndex + 1;
            var path = "Assets\\_Burger-YandexGame\\Scenes\\Levels\\";
            if(newScene == 0)
            {
                path = "Assets\\_Burger-YandexGame\\Scenes\\Levels\\Level" + ".unity";
                levelName = "Level";
            }
            else
            {
                path = "Assets\\_Burger-YandexGame\\Scenes\\Levels\\Level " + newScene + ".unity";
                levelName = "Level " + newScene.ToString();
            }
            EditorSceneManager.OpenScene(path);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Open Folder", EditorStyles.boldLabel);

        if(GUILayout.Button("Open Recipe Folder"))
        {
            string assetPath = $"{saveRecipeDataPath}/level.asset";
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if(folder != null)
            {
                EditorGUIUtility.PingObject(folder);
            }
            else
            {
                Debug.Log("Папка не найдена: " + assetPath);
            }
        }

        if(GUILayout.Button("Open Prefabs Folder"))
        {
            string assetPath = $"{ingredientsFolder}/Anchovy.prefab";
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if(folder != null)
            {
                EditorGUIUtility.PingObject(folder);
            }
            else
            {
                Debug.Log("Папка не найдена: " + assetPath);
            }
        }
    }

    private void DrawPrefabDropdown(int entryIndex)
    {
        if(availablePrefabContents.Length == 0)
        {
            EditorGUILayout.LabelField("Префабы не найдены");
            return;
        }

        if(recipeEntries[entryIndex].selectedIndex < 0 ||
            recipeEntries[entryIndex].selectedIndex >= availablePrefabContents.Length)
        {
            recipeEntries[entryIndex].selectedIndex = 0;
        }

        GUIContent currentContent = availablePrefabContents[recipeEntries[entryIndex].selectedIndex];

        if(GUILayout.Button(currentContent, EditorStyles.popup, GUILayout.Width(200)))
        {
            IconDropdownWindow.ShowDropdown(
                this, 
                availablePrefabContents,
                (selectedIdx) =>
                {
                    recipeEntries[entryIndex].selectedIndex = selectedIdx;
                }
            );
        }
    }

    private void UpdateAvailablePrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { ingredientsFolder });
        availablePrefabs = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(prefab => prefab != null && prefab.GetComponent<Ingredient>() != null)
            .ToList();

        availablePrefabContents = availablePrefabs.Select(prefab =>
        {
            Texture2D previewTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{imagesFolder}/{prefab.name}.png");
            if(previewTex == null)
            {
                previewTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{imagesFolder}/{prefab.name}.jpg");
            }
            return new GUIContent(prefab.name, previewTex);
        }).ToArray();
    }

    private void CreateOrUpdateRecipeData()
    {
        if(string.IsNullOrEmpty(levelName))
        {
            EditorUtility.DisplayDialog("Ошибка", "Укажите имя уровня", "OK");
            return;
        }

        if(!AssetDatabase.IsValidFolder(saveRecipeDataPath))
        {
            AssetDatabase.CreateFolder("Assets", "RecipeData");
            saveRecipeDataPath = "Assets/RecipeData";
        }

        string assetPath = $"{saveRecipeDataPath}/{levelName}.asset";

        RecipeData recipeData = AssetDatabase.LoadAssetAtPath<RecipeData>(assetPath);
        if(recipeData == null)
        {
            recipeData = ScriptableObject.CreateInstance<RecipeData>();
            AssetDatabase.CreateAsset(recipeData, assetPath);
        }

        var newIngredients = new List<RecipeIngredient>();
        foreach(var entry in recipeEntries)
        {
            if(availablePrefabs.Count > entry.selectedIndex)
            {
                var prefab = availablePrefabs[entry.selectedIndex];
                var ingredientComp = prefab.GetComponent<Ingredient>();
                if(ingredientComp != null)
                {
                    newIngredients.Add(new RecipeIngredient(ingredientComp, entry.count));
                }
                else
                {
                    Debug.LogWarning($"Префаб {prefab.name} не содержит компонента Ingredient.");
                }
            }
        }

        recipeData.SetRecipeIngredients(newIngredients);
        EditorUtility.SetDirty(recipeData);
        AssetDatabase.SaveAssets();
        Debug.Log($"RecipeData для уровня {levelName} успешно создан/обновлен.");

        Object folder = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        if(folder != null)
        {
            EditorGUIUtility.PingObject(folder);
        }
        else
        {
            Debug.Log("Папка не найдена: " + assetPath);
        }
    }
}
