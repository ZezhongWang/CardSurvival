using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public BaseCardDataAsset DataAsset;

    public LinkedListNode<BaseCard> StackNode { get; set; }

    [SerializeField] private TextMeshProUGUI _nameText;

    private Vector2 _cursorOffset;

    // Start is called before the first frame update
    void Start()
    {
        if (GetStack() == null)
        {
            // create a stack for the card
            var CardStack = Instantiate(GameManager.Instance.CardStackPrefab, transform.position, Quaternion.identity,
                GameManager.Instance.Canvas.transform);
            CardStack.GetComponent<CardStack>().Initialize(this);
        }
        
        if (DataAsset != null)
        {
            Initialize(DataAsset);
        }
    }

    public virtual void Initialize(BaseCardDataAsset dataAsset)
    {
        DataAsset = dataAsset;
        _nameText.text = dataAsset.name;
    }

    public CardStack GetStack()
    {
        return transform.parent.GetComponent<CardStack>();
    }

    public void SetStack(CardStack stack)
    {
        transform.SetParent(stack.transform);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag " + DataAsset.name);
        // This makes the card not interfere with raycasting
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        _cursorOffset = new Vector2(eventData.position.x - transform.position.x,
            eventData.position.y - transform.position.y);
        if (GetStack().Count > 1)
        {
            // Create a new stack for the card
            var newStack = Instantiate(GameManager.Instance.CardStackPrefab, transform.position, Quaternion.identity,
                GameManager.Instance.Canvas.transform).GetComponent<CardStack>();
            newStack.Initialize(this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // move with card stack
        GetStack().transform.position = eventData.position - _cursorOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        // This makes the card interact with raycasting again
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var aboveCard = eventData.pointerDrag.GetComponent<BaseCard>();
        if (aboveCard != null)
        {
            aboveCard.StackOn(GetStack());
        }
    }

    public void Unstack()
    {
        // Unstack the card from the card stack
    }

    public void StackOn(CardStack other)
    {
        other.Concat(GetStack());
        transform.SetParent(other.transform);
        // Todo: should adjust sorting layer here...
    }

    public override string ToString()
    {
        return DataAsset.name;
    }
}