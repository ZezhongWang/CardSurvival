using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseCardFactory : AbstractCardFactory
{
    public BaseCard CardPrefab;
    
    public override BaseCard CreateCard(BaseCardDataAsset dataAsset)
    {
        Assert.IsNotNull(CardPrefab);
        BaseCard card = Instantiate(CardPrefab);
        card.Initialize(dataAsset);
        return card;
    }
}

