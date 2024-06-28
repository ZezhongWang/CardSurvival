using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

public class LLMAdapter : MonoBehaviour
{
    public static LLMAdapter Instance { get; private set; }
    
    private BackendLLM _backendLLM;
    
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
        _backendLLM = GetComponent<BackendLLM>();
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    public async Task<RecipeDataAsset> TryGetRecipe(CardStack cardStack)
    {
        RecipeDataAsset NewRecipe = RecipeDataAsset.CreateInstance("RecipeDataAsset") as RecipeDataAsset;
        NewRecipe.Ingredients = new List<BaseCardDataAsset>();
        var enumerator = cardStack.GetEnumerator();
        while (enumerator.MoveNext())
        {
            NewRecipe.Ingredients.Add(enumerator.Current.DataAsset);
        }
        
        string key = StaticDataSystem.Instance.GenerateIngredientsKey(NewRecipe.Ingredients);
        NewRecipe.Duration = 1;
        
        // call LLM to get deliverable
        string deliverable = await _backendLLM.GetDeliverable(key);
        // deliverable is in format wood,resource
        string[] deliverableSplit = deliverable.Split(',');
        if (deliverableSplit.Length != 2)
        {
            Debug.Log("TryGetRecipe: Invalid deliverable format: " + deliverable);
            return null;
        }
        deliverable = deliverableSplit[0];
        
        NewRecipe.Deliverable = StaticDataSystem.Instance.GetCardDataAsset(deliverable);
        if (NewRecipe.Deliverable == null)
        {
            // create a new BaseCardDataAsset
            BaseCardDataAsset NewCard = BaseCardDataAsset.CreateInstance("BaseCardDataAsset") as BaseCardDataAsset;
            bool success = Enum.TryParse(deliverableSplit[1], out CardType cardType);
            NewCard.name = deliverable;
            NewCard.Archetype = deliverable;
            NewCard.SetCardType(success? cardType : CardType.Resource); 
            StaticDataSystem.Instance.RegisterCard(NewCard);
            NewRecipe.Deliverable = NewCard;
        }
        StaticDataSystem.Instance.RegisterRecipe(NewRecipe);
        return NewRecipe;
    }
    
}
