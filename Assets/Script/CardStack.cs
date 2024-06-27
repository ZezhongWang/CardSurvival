using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.PlayerLoop;

public class CardStack : MonoBehaviour
{
    private LinkedList<BaseCard> _cardsInStack;
    private bool _isProcessingRecipe = false;
    private float _progress;
    private RecipeDataAsset _currentRecipe;
    
    [SerializeField]
    private ProgressBar _progressBar;
    
    public int Count => _cardsInStack.Count;
    public void Initialize(BaseCard card)
    {
        _cardsInStack = new LinkedList<BaseCard>();
        _cardsInStack.AddFirst(card);
        card.StackNode = _cardsInStack.First;
        card.transform.SetParent(this.transform);
        _progressBar.gameObject.SetActive(false);
    }
    
    public System.Collections.Generic.LinkedList<BaseCard>.Enumerator GetEnumerator()
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
            node = node.Next;
        }

        other._cardsInStack.Clear();
        Destroy(other.gameObject);
        OnCardChanged();
    }
    
    private void StackCard(BaseCard card)
    {
        Assert.IsNotNull(card);
        _cardsInStack.AddLast(card);
        card.StackNode = _cardsInStack.Last;
        card.transform.SetParent(this.transform);
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
        // Remove the ingredients from the stack, and add the deliverable to canvas
        foreach (var ingredient in _currentRecipe.Ingredients)
        {
            // iterate through the stack to find the card with the same archetype and remove it
            var enumerator = _cardsInStack.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.DataAsset.Archetype == ingredient.Archetype)
                {
                    _cardsInStack.Remove(enumerator.Current);
                    Destroy(enumerator.Current.gameObject);
                    break;
                }
            }
        }
        
        var deliverableCard = GameManager.Instance.CardFactory.CreateCard(_currentRecipe.Deliverable);
        deliverableCard.transform.position = transform.position;
        _currentRecipe = null;
        SetProcessingState(false);
        StackCard(deliverableCard);
    }

    private void SetProcessingState(bool isProcessing)
    {
        _isProcessingRecipe = isProcessing;
        _progressBar.gameObject.SetActive(isProcessing);
    }
    
    private void CheckForRecipe()
    {
        _currentRecipe = StaticDataSystem.Instance.FindValidRecipe(this);
        SetProcessingState(_currentRecipe != null);

        if (_currentRecipe == null)
        {
            Debug.LogFormat("CheckForRecipe: No recipe found for stack: {0}", this);
        }
        else
        {
            Debug.LogFormat("CheckForRecipe: Found recipe for {0}", _currentRecipe.Deliverable.name);
        }
    }

    public override string ToString()
    {
        return string.Join(", ", _cardsInStack.Select(card => card.DataAsset.name));
    }
    
    
}
