using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class ResourceCardFactory : BaseCardFactory
{
    public override BaseCard CreateCard(BaseCardDataAsset dataAsset)
    {
        Assert.IsTrue(dataAsset is ResourceDataAsset);
        Assert.IsTrue(CardPrefab is ResourceCard);
        BaseCard card = Instantiate(CardPrefab);
        card.Initialize(dataAsset);
        return card;
    }
}