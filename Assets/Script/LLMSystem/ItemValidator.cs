using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using OllamaSharp.Models.Chat;



public class ItemValidator : LLMSystem
{
    [System.Serializable]
    public class Example
    {
        public string item;
        public string valid;
    }
    
    public override void SetUpContext()
    {
        // read the ChatSetUp.txt
        TextAsset chatSetupAsset = Resources.Load<TextAsset>("LLMContext/ItemValidatorSetUp");
        Debug.Log("ItemValidatorSetUp: " + chatSetupAsset.text);
        
        _messages.Add(new Message
        {
            Role = "system",
            Content = chatSetupAsset.text
        });
        
        // Example format is: human-tree=wood,Resource
        TextAsset examplesAsset = Resources.Load<TextAsset>("LLMContext/ItemValidatorExamples");
        
        string[] lines = examplesAsset.text.Split('\n');
        List<Example> examples = new List<Example>();
        foreach (string line in lines)
        {
            // Split the line into parts
            string[] parts = line.Split("=");

            // Create a new Example struct with the parts as field values
            Example example = new Example
            {
                item = parts[0],
                valid = parts[1],
            };

            // Add the new Example to the list
            examples.Add(example);
        }

        foreach (var example in examples)
        {
            _messages.Add(new Message
            {
                Role = "user",
                Content = example.item
            });
            _messages.Add(new Message
            {
                Role = "assistant",
                Content = example.valid
            });
        }
    }

    public async Task<bool> ValidateItem(string item)
    {
        string answer = await GetResponse(item);
        return answer == "Yes";   
    }
}
