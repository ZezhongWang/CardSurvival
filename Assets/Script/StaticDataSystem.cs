using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Add this line


public class StaticDataSystem : MonoBehaviour
{
    public static StaticDataSystem Instance { get; private set; }

    private Dictionary<string, RecipeDataAsset> _recipes = new Dictionary<string, RecipeDataAsset>();
    private Dictionary<string, RecipeDataAsset> _recipeLookup = new Dictionary<string, RecipeDataAsset>();
    private Dictionary<string, BaseCardDataAsset> _cards = new Dictionary<string, BaseCardDataAsset>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitStaticData();
    }

    void Update()
    {
    }
    
    public string GenerateIngredientsKey(List<BaseCardDataAsset> ingredients)
    {
        var sortedIngredients = ingredients.OrderBy(ingredient => ingredient.Archetype)
            .Select(ingredient => ingredient.Archetype);
        return string.Join("-", sortedIngredients);
    }

    public RecipeDataAsset FindValidRecipe(List<BaseCardDataAsset> ingredients)
    {
        var key = GenerateIngredientsKey(ingredients);
        if (_recipeLookup.TryGetValue(key, out RecipeDataAsset recipe))
        {
            return recipe;
        }

        return null;
    }
    

    public RecipeDataAsset FindValidRecipe(CardStack stack)
    {
        var enumerator = stack.GetEnumerator();
        var cards = new List<BaseCardDataAsset>();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.DataAsset.CardType == CardType.Location)
            {
                // don't provide any fixed recipe for location cards
                return null;
            }
            cards.Add(enumerator.Current.DataAsset);
        }

        return FindValidRecipe(cards);
    }

    public void RegisterRecipe(RecipeDataAsset recipeDataAsset)
    {
        if (recipeDataAsset != null && !_recipes.ContainsKey(recipeDataAsset.Deliverable.Archetype))
        {
            Debug.Log("Registering recipe: " + recipeDataAsset.Deliverable.Archetype);
            _recipes.Add(recipeDataAsset.Deliverable.Archetype, recipeDataAsset);
            var key = GenerateIngredientsKey(recipeDataAsset.Ingredients);
            _recipeLookup[key] = recipeDataAsset;
        }
    }

    public BaseCardDataAsset GetCardDataAsset(string archetype)
    {
        if (_cards.TryGetValue(archetype, out BaseCardDataAsset card))
        {
            return card;
        }

        return null;
    }

    public void RegisterCard(BaseCardDataAsset cardDataAsset)
    {
        if (cardDataAsset != null && !_cards.ContainsKey(cardDataAsset.name))
        {
            _cards.Add(cardDataAsset.Archetype, cardDataAsset);
        }
    }

    private void InitStaticData()
    {
        Debug.Log("InitStaticData...");
        // load all data asset under StaticData folder
        var recipeDataAssets = Resources.LoadAll<RecipeDataAsset>("StaticData");
        foreach (var recipeDataAsset in recipeDataAssets)
        {
            RegisterRecipe(recipeDataAsset);
        }

        Debug.Log("Registered " + _recipes.Count + " recipes");
    }
}