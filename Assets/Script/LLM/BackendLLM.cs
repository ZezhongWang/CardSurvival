using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using OllamaSharp;
using UnityEngine;

public class BackendLLM : MonoBehaviour
{
    private OllamaApiClient _client;
    // Start is called before the first frame update
    void Start()
    {
        // set up the client
        var uri = new Uri("http://localhost:11434");
        _client = new OllamaApiClient(uri);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async Task<string> GetDeliverable(string key)
    {
        await Task.Delay(1000);
        return "New Resource";
    }
}
