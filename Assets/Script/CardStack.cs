using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class CardStack : MonoBehaviour
{
    private LinkedList<BaseCard> _cardsInStack;
    private bool _isProcessingRecipe = false;
    private float _progress;
    private RecipeDataAsset _currentRecipe;

    [SerializeField] private ProgressBar _progressBar;

    public int Count => _cardsInStack.Count;

    public void Initialize(BaseCard card)
    {
        _cardsInStack = new LinkedList<BaseCard>();
        _cardsInStack.AddFirst(card);
        card.StackNode = _cardsInStack.First;
        card.transform.SetParent(this.transform);
        card.transform.localPosition = Vector3.zero;
        _progressBar.gameObject.SetActive(false);
    }

    public LinkedList<BaseCard>.Enumerator GetEnumerator()
    {
        return _cardsInStack.GetEnumerator();
    }

    private void Start()
    {
        // Initialization code here
    }

    public void Concat(CardStack other)
    {
        Debug.LogFormat("Concatenating stacks, bottom {0}, top {1}", _cardsInStack.Count, other._cardsInStack.Count);
        // Append each card from other._cardsInStack to _cardsInStack
        var node = other._cardsInStack.First;
        while (node != null)
        {
            _cardsInStack.AddLast(node.Value);
            node.Value.SetStack(this);
            node.Value.StackNode = _cardsInStack.Last;
            node = node.Next;
        }

        other._cardsInStack.Clear();
        Destroy(other.gameObject);
        OnCardChanged();
    }

    public void UnStackCard(BaseCard card)
    {
        Assert.IsNotNull(card);
        _cardsInStack.Remove(card.StackNode);
        card.StackNode = null;
        card.transform.SetParent(null);
        OnCardChanged();
    }

    private void OnCardChanged()
    {
        if (_cardsInStack.Count == 0)
        {
            Destroy(gameObject);
        }

        CheckForRecipe();
    }

    private void Update()
    {
        if (_isProcessingRecipe)
        {
            _progress += Time.deltaTime / _currentRecipe.Duration;
            _progressBar.UpdateValue(_progress);
            if (_progress >= 1.0f)
            {
                _progress = 0.0f;
                ProcessRecipe();
            }
        }
    }

    private void ProcessRecipe()
    {
        bool CardsChanged = false;
        // Remove the ingredients from the stack, and add the deliverable to canvas
        foreach (var ingredient in _currentRecipe.Ingredients)
        {
            // iterate through the stack to find the card with the same archetype and remove it
            var enumerator = _cardsInStack.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.DataAsset.Archetype == ingredient.Archetype)
                {
                    if (enumerator.Current.ReduceDurability() <= 0)
                    {
                        _cardsInStack.Remove(enumerator.Current);
                        Destroy(enumerator.Current.gameObject);
                        CardsChanged = true;
                    }

                    break;
                }
            }
        }

        // instance the deliverable card next to the stack
        var deliverableCard = GameManager.Instance.CardFactory.CreateCard(_currentRecipe.Deliverable);
        var rectTransform = deliverableCard.GetComponent<RectTransform>();
        float halfCardWidth = rectTransform.rect.width * rectTransform.localScale.x;
        deliverableCard.transform.position = transform.position + new Vector3(halfCardWidth + 10, 0, 0);
        
        SetProcessingState(false);
        if (CardsChanged)
        {
            OnCardChanged();
        }
    }

    public void SetProcessingState(bool isProcessing)
    {
        _isProcessingRecipe = isProcessing;
        _progressBar.gameObject.SetActive(isProcessing);
        if (!isProcessing)
        {
            _progress = 0.0f;
            _currentRecipe = null;
        }
    }

    private async void CheckForRecipe()
    {
        if (Count <= 1)
        {
            SetProcessingState(false);
            return;
        }
        
        _currentRecipe = StaticDataSystem.Instance.FindValidRecipe(this);
        if (_currentRecipe == null && Count > 1)
        {
            _currentRecipe = await RecipeGenerator.Instance.TryGetRecipe(this);
            if (_currentRecipe != null)
            {
                Debug.LogFormat("CheckForRecipe: Add new recipe for {0}", _currentRecipe.Deliverable.Archetype);
            }
        }
        else
        {
            Debug.LogFormat("CheckForRecipe: Found existing recipe for {0}", _currentRecipe.Deliverable.Archetype);
        }

        SetProcessingState(_currentRecipe != null);
    }

    public override string ToString()
    {
        return string.Join(", ", _cardsInStack.Select(card => card.DataAsset.Archetype));
    }
}