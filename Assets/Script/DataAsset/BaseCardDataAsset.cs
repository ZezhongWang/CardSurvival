using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CardType
{
    Character,
    Resource,
    Building,
    Food,
    Location
    // Add more card types as needed
}

public class BaseCardDataAsset : ScriptableObject
{
    public CardType CardType;
    public string Archetype;
    public int Durability;
    
    public void SetCardType(CardType cardType)
    {
        CardType = cardType;
        switch (CardType)
        {
            case CardType.Character:
            case CardType.Building:
            case CardType.Location:
                Durability = int.MaxValue;
                break;
            default:
                Durability = 1;
                break;
        }
    }
}
