using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using OllamaSharp.Models.Chat;
using Random = System.Random;
public class LLMSystem : MonoBehaviour
{
    protected List<Message> _messages = new List<Message>();

    public void Start()
    {
        SetUpContext();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("History:" + GetChatHistory());
    }
    
    public virtual void SetUpContext()
    {
        // implemented by child classes
    }
    
    protected async Task<string> GetResponse(string request)
    {
        _messages.Add(new Message
        {
            Role = "user",
            Content = request
        });
        
        Message answer = await BackendLLM.Instance.SendChat(_messages);
        _messages.Add(answer);
        return answer.Content;
    }
    
    private string GetChatHistory()
    {
        return string.Join("\n", _messages.Select(m => m.Content));
    }
}
