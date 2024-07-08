using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using OllamaSharp.Models.Chat;



public class RecipeSystem : LLMSystem
{
    public static RecipeSystem Instance { get; private set; }
    
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
    }

    public async Task<RecipeDataAsset> TryGetRecipe(CardStack cardStack)
    {
        if (cardStack.Count == 2 && cardStack.HasCardType(CardType.Location) && cardStack.HasCardType(CardType.Character))
        {
            // iterate cardstack using enumerator
            var enumerator = cardStack.GetEnumerator();
            while (enumerator.MoveNext())
            {
               if (enumerator.Current.DataAsset.CardType == CardType.Location)
               {
                   return await ExplorationSystem.Instance.TryGetRecipe(cardStack);
               }
            }
        }
        
        return await CraftSystem.Instance.TryGetRecipe(cardStack);
    }
}
