using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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
        NewRecipe.Duration = 1.0f;
        
        // call LLM to get deliverable
        string deliverable = await _backendLLM.GetDeliverable(key);
        NewRecipe.Deliverable = StaticDataSystem.Instance.GetCardDataAsset(deliverable);
        if (NewRecipe.Deliverable == null)
        {
            // create a new BaseCardDataAsset
            BaseCardDataAsset NewCard = BaseCardDataAsset.CreateInstance("BaseCardDataAsset") as BaseCardDataAsset;
            NewCard.name = deliverable;
            NewCard.Archetype = deliverable;
            // todo: should diversify card type here
            NewCard.CardType = CardType.Resource;
            StaticDataSystem.Instance.RegisterCard(NewCard);
            NewRecipe.Deliverable = NewCard;
        }
        StaticDataSystem.Instance.RegisterRecipe(NewRecipe);
        return NewRecipe;
    }
    
}
