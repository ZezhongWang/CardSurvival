using System.Collections.Generic;
using UnityEngine;

public class GeneralCardFactory : MonoBehaviour
{
    [SerializeField]
    private List<CardType> _cardTypeKeys;
    
    [SerializeField]
    private List<AbstractCardFactory> _cardFactoryValues;
    
    private Dictionary<CardType, AbstractCardFactory> _cardFactories;

    private void Start()
    {
        InitializeDictionary();
    }
    
    private void InitializeDictionary()
    {
        _cardFactories = new Dictionary<CardType, AbstractCardFactory>();
        for (int i = 0; i < Mathf.Min(_cardTypeKeys.Count, _cardFactoryValues.Count) ; i++)
        {
            _cardFactories[_cardTypeKeys[i]] = _cardFactoryValues[i];
        }
    }
    
    public BaseCard CreateCard(BaseCardDataAsset dataAsset)
    {
        if (_cardFactories.ContainsKey(dataAsset.CardType))
        {
            return _cardFactories[dataAsset.CardType].CreateCard(dataAsset);
        }
        Debug.LogError("No factory found for card type: " + dataAsset.CardType);
        return null;
    }
}