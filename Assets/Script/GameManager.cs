using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField]
    private GameObject cardStackPrefab;
    
    public GameObject CardStackPrefab 
    {
        get { return cardStackPrefab; }
    }

    public GeneralCardFactory CardFactory { get; private set; }
    
    [SerializeField]
    private Canvas canvas;
    
    public Canvas Canvas
    {
        get { return canvas; }
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keep the manager across scenes
        }
        else
        {
            Destroy(gameObject);
        }
        CardFactory = GetComponent<GeneralCardFactory>();
    }
}