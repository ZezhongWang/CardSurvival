using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using UnityEngine;

[System.Serializable]
public class Example
{
    public string ingredients;
    public string deliverable;
}

[System.Serializable]
public class ExampleList
{
    public List<Example> examples;
}

public class BackendLLM : MonoBehaviour
{
    
    private List<Message> _messages = new List<Message>();
    
    private OllamaApiClient _client;
    // Start is called before the first frame update
    void Start()
    {
        // set up the client
        var uri = new Uri("http://localhost:11434");
        _client = new OllamaApiClient(uri, "llama3");
        SetUpContext();
    }

    private async void SetUpContext()
    {
        // read the ChatSetUp.txt
        TextAsset chatSetupAsset = Resources.Load<TextAsset>("StaticData/ChatSetUp");
        Debug.Log("ChatSetUp: " + chatSetupAsset.text);
        
        _messages.Add(new Message
        {
            Role = "system",
            Content = chatSetupAsset.text
        });
        
        // read Examples.json
        TextAsset examplesAsset = Resources.Load<TextAsset>("StaticData/Examples");
        ExampleList exampleList = JsonUtility.FromJson<ExampleList>(examplesAsset.text);
        Debug.Log("Examples: " + examplesAsset.text);
        
        foreach (var example in exampleList.examples)
        {
            _messages.Add(new Message
            {
                Role = "user",
                Content = example.ingredients
            });
            _messages.Add(new Message
            {
                Role = "assistant",
                Content = example.deliverable
            });
        }
        
        ChatRequest request = new ChatRequest
        {
            Messages = _messages,
            Model = _client.SelectedModel,
            Stream = false
        };

        await _client.SendChat(request, stream => { });
    }

    private void OnApplicationQuit()
    {
        Debug.Log("History:" + GetChatHistory());
    }

    async Task<string> SendChat(string message)
    {
        Debug.Log("message: " + message);
        _messages.Add(new Message
        {
            Role = "user",
            Content = message
        });
        
        ChatRequest request = new ChatRequest
        {
            Messages = _messages,
            Model = _client.SelectedModel,
            Stream = false
        };

        IEnumerable<Message> answer = await _client.SendChat(request, stream => { });
        _messages = answer.ToList();

        Debug.Log("response: " + _messages.Last().Content);
        return _messages.Last().Content;
    }
    
    public async Task<string> GetDeliverable(string key)
    {
        return await SendChat(key);
    }
    
    private string GetChatHistory()
    {
        return string.Join("\n", _messages.Select(m => m.Content));
    }
}
