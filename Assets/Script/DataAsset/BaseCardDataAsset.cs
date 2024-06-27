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
}
