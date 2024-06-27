using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RecipeDataAsset : ScriptableObject
{
    public float Duration; // Duration to complete the recipe
    public List<BaseCardDataAsset> Ingredients; // List of ingredients
    public BaseCardDataAsset Deliverable; // The deliverable item

    public bool IsValid()
    {
        return Ingredients != null && Ingredients.Count > 0 && Deliverable != null;
    }
}
