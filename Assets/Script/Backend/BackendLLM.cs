using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using UnityEngine;


public class BackendLLM : MonoBehaviour
{
    public static BackendLLM Instance { get; private set; }
    
    private OllamaApiClient _client;
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // set up the client
        var uri = new Uri("http://localhost:11434");
        _client = new OllamaApiClient(uri, "llama3");
    }

    public async Task<Message> SendChat(List<Message> messages)
    {
        Debug.Log("message: " + messages.Last().Content);
        
        ChatRequest request = new ChatRequest
        {
            Messages = messages,
            Model = _client.SelectedModel,
            Stream = false
        };

        IEnumerable<Message> answer = await _client.SendChat(request, stream => { });
        Message response = answer.Last(); 
        Debug.Log("response: " + response.Content);
        
        return response;
    }
    
}
