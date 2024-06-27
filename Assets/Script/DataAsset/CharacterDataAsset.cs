using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterDataAsset : BaseCardDataAsset
{
    public int MaxHealth;
    
    private void Awake()
    {
        CardType = CardType.Character;
        Durability = int.MaxValue;
    }
}
