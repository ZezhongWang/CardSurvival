using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using OllamaSharp.Models.Chat;
using UnityEngine.Assertions;


public class ExplorationSystem : LLMSystem
{
    public static ExplorationSystem Instance { get; private set; }

    private ItemValidator _validator;
    
    
    // Start is called before the first frame update
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
        _validator = GetComponent<ItemValidator>();
    }

    [System.Serializable]
    public class Example
    {
        public string biome;
        public string items;
    }
    
    public override void SetUpContext()
    {
        // read the ChatSetUp.txt
        TextAsset chatSetupAsset = Resources.Load<TextAsset>("LLMContext/ItemSpawnerSetup");
        Debug.Log("ItemSpawnerSetup: " + chatSetupAsset.text);
        
        _messages.Add(new Message
        {
            Role = "system",
            Content = chatSetupAsset.text
        });
        
        // Example format is: human-tree=wood,Resource
        TextAsset examplesAsset = Resources.Load<TextAsset>("LLMContext/ItemSpawnerExamples");
        
        string[] lines = examplesAsset.text.Split('\n');
        List<Example> examples = new List<Example>();
        foreach (string line in lines)
        {
            // Split the line into parts
            string[] parts = line.Split( '=');

            // Create a new Example struct with the parts as field values
            Example example = new Example
            {
                biome = parts[0],
                items = parts[1],
            };

            // Add the new Example to the list
            examples.Add(example);
        }

        foreach (var example in examples)
        {
            _messages.Add(new Message
            {
                Role = "user",
                Content = example.biome
            });
            _messages.Add(new Message
            {
                Role = "assistant",
                Content = example.items
            });
        }
    }

    public async Task<List<BaseCardDataAsset>> GetBiomeItems(BaseCardDataAsset biomeCard)
    {
        // call LLM to get deliverable
        string itemList = await GetResponse(biomeCard.Archetype);
        // deliverable is in format wood,resource
        string[] itemSplit = itemList.Split(',');
        if (itemSplit.Length < 5)
        {
            Debug.LogError("GetBiomeItems: not enough items found: " + itemList);
            return null;
        }
        List<BaseCardDataAsset> items = new List<BaseCardDataAsset>();
        for (int i = 0; i < itemSplit.Length; i++)
        {
            BaseCardDataAsset item = StaticDataSystem.Instance.GetCardDataAsset(itemSplit[i]);
            if (item)
            {
                items.Add(item);
                continue;
            }
            
            bool result = await _validator.ValidateItem(itemSplit[i]);
            if (!result)
            {
                Debug.Log("GetBiomeItems: Invalid deliverable: " + itemSplit[i]);
                return null;
            }

            BaseCardDataAsset newCard = BaseCardDataAsset.CreateInstance("BaseCardDataAsset") as BaseCardDataAsset;
            // todo: should get the card type from LLM.
            newCard.name = itemSplit[i];
            newCard.Archetype = itemSplit[i];
            newCard.SetCardType(CardType.Resource);
            StaticDataSystem.Instance.RegisterCard(newCard);
            items.Add(newCard);
        }
        return items;
    }
    
    public async Task<RecipeDataAsset> TryGetRecipe(CardStack cardStack)
    {
        Assert.IsTrue(cardStack.Count == 2 && cardStack.HasCardType(CardType.Location) &&
               cardStack.HasCardType(CardType.Character));
        
        var ingredients = new List<BaseCardDataAsset>();
        var enumerator = cardStack.GetEnumerator();
        BaseCardDataAsset biomeDataAsset = null;
        while (enumerator.MoveNext())
        {
            ingredients.Add(enumerator.Current.DataAsset);
            if (enumerator.Current.DataAsset.CardType == CardType.Location)
            {
                biomeDataAsset = enumerator.Current.DataAsset;
            }
        }

        if (biomeDataAsset == null)
        {
            return null;
        }
        
        List<BaseCardDataAsset> deliverable = await GetBiomeItems(biomeDataAsset);
        if (deliverable == null)
        {
            return null;
        }
        
        RecipeDataAsset newRecipe = RecipeDataAsset.CreateInstance("RecipeDataAsset") as RecipeDataAsset;
        newRecipe.Ingredients = ingredients;
        newRecipe.Duration = 1;
        newRecipe.Deliverables = deliverable;
        StaticDataSystem.Instance.RegisterRecipe(newRecipe);
        return newRecipe;
    }
}
