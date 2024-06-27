using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCard : BaseCard
{

    public int Health { get; set; }

    public override void Initialize(BaseCardDataAsset dataAsset)
    {
        base.Initialize(dataAsset);
        Health = ((CharacterDataAsset)dataAsset).MaxHealth;
    }
}
