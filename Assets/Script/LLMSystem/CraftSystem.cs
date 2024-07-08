using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using OllamaSharp.Models.Chat;



public class CraftSystem : LLMSystem
{
    public static CraftSystem Instance { get; private set; }

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
        public string ingredients;
        public string deliverable;
        public string cardType;
    }
    
    public override void SetUpContext()
    {
        // read the ChatSetUp.txt
        TextAsset chatSetupAsset = Resources.Load<TextAsset>("LLMContext/ChatSetUp");
        Debug.Log("ChatSetUp: " + chatSetupAsset.text);
        
        _messages.Add(new Message
        {
            Role = "system",
            Content = chatSetupAsset.text
        });
        
        // Example format is: human-tree=wood,Resource
        TextAsset examplesAsset = Resources.Load<TextAsset>("LLMContext/Examples");
        
        string[] lines = examplesAsset.text.Split('\n');
        List<Example> examples = new List<Example>();
        foreach (string line in lines)
        {
            // Split the line into parts
            string[] parts = line.Split(new[] { '=', ',' }, StringSplitOptions.None);

            // Create a new Example struct with the parts as field values
            Example example = new Example
            {
                ingredients = parts[0],
                deliverable = parts[1],
                cardType = parts[2]
            };

            // Add the new Example to the list
            examples.Add(example);
        }

        foreach (var example in examples)
        {
            _messages.Add(new Message
            {
                Role = "user",
                Content = example.ingredients
            });
            _messages.Add(new Message
            {
                Role = "assistant",
                Content = example.deliverable + "," + example.cardType
            });
        }
    }

    private async Task<BaseCardDataAsset> GetDeliverable(string ingredientKey)
    {
        // call LLM to get deliverable
        string deliverableAndType = await GetResponse(ingredientKey);
        // deliverable is in format wood,resource
        string[] deliverableSplit = deliverableAndType.Split(',');
        if (deliverableSplit.Length != 2)
        {
            Debug.Log("TryGetRecipe: Invalid deliverable format: " + deliverableSplit);
            return null;
        }
        string deliverable = deliverableSplit[0];
        BaseCardDataAsset deliverableCard = StaticDataSystem.Instance.GetCardDataAsset(deliverable);
        if (deliverableCard)
        {
            return deliverableCard;
        }
        
        string Type = deliverableSplit[1];
        bool result = await _validator.ValidateItem(deliverable);
        if (!result)
        {
            Debug.Log("TryGetRecipe: Invalid deliverable: " + deliverable);
            return null;
        }

        BaseCardDataAsset newCard = BaseCardDataAsset.CreateInstance("BaseCardDataAsset") as BaseCardDataAsset;
        bool success = Enum.TryParse(Type, out CardType cardType);
        newCard.name = deliverable;
        newCard.Archetype = deliverable;
        newCard.SetCardType(success? cardType : CardType.Resource);
        StaticDataSystem.Instance.RegisterCard(newCard);
        return newCard;
    }
    
    public async Task<RecipeDataAsset> TryGetRecipe(CardStack cardStack)
    {
        var ingredients = new List<BaseCardDataAsset>();
        var enumerator = cardStack.GetEnumerator();
        while (enumerator.MoveNext())
        {
            ingredients.Add(enumerator.Current.DataAsset);
        }
        string key = StaticDataSystem.Instance.GenerateIngredientsKey(ingredients);
        
        BaseCardDataAsset deliverable = await GetDeliverable(key);
        if (deliverable == null)
        {
            // no valid deliverable means no valid recipe
            return null;
        }
        var deliverables = new List<BaseCardDataAsset>();
        deliverables.Add(deliverable);
        
        RecipeDataAsset newRecipe = RecipeDataAsset.CreateInstance("RecipeDataAsset") as RecipeDataAsset;
        newRecipe.Ingredients = ingredients;
        newRecipe.Duration = 1;
        newRecipe.Deliverables = deliverables;
        StaticDataSystem.Instance.RegisterRecipe(newRecipe);
        return newRecipe;
    }
}
