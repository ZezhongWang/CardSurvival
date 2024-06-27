using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractCardFactory : MonoBehaviour
{
    public abstract BaseCard CreateCard(BaseCardDataAsset dataAsset);
}

