using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RecipeDataAsset : ScriptableObject
{
    public float Duration; // Duration to complete the recipe
    public List<BaseCardDataAsset> Ingredients; // List of ingredients
    public List<BaseCardDataAsset> Deliverables; // The deliverable item

    public bool IsValid()
    {
        return Ingredients != null && Ingredients.Count > 0 && Deliverables.Count > 0;
    }
    
    public BaseCardDataAsset GetDeliverable()
    {
        // get deliverable from the list by random
        return Deliverables[Random.Range(0, Deliverables.Count)];
    }

    public override string ToString()
    {
        return string.Format("Recipe: {0} -> {1}", string.Join(", ", Ingredients), string.Join(", ", Deliverables));
    }
    
    public string GenerateKey()
    {
        return string.Join("-", Ingredients) + " -> " + string.Join("-", Deliverables);
    }
}
