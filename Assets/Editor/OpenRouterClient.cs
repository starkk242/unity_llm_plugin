using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class OpenRouterClient
{
    public class LLMResult
    {
        public string response;
        public string error;
    }

    [Serializable]
    private class OpenRouterRequest
    {
        public string model;
        public List<Message> messages;
    }
    [Serializable]
    private class Message
    {
        public string role;
        public string content;
    }
    [Serializable]
    private class OpenRouterResponse
    {
        public Choice[] choices;
    }
    [Serializable]
    private class Choice
    {
        public Message message;
    }

    public IEnumerator SendPromptCoroutine(string apiKey, string prompt, Action<LLMResult> callback)
    {
        var requestBody = new OpenRouterRequest()
        {
            model = "openai/gpt-3.5-turbo",
            messages = new List<Message>()
            {
                new Message { role = "system", content = "You are a helpful Unity Editor assistant." },
                new Message { role = "user", content = prompt }
            }
        };
        string json = JsonUtility.ToJson(requestBody, true);

        using (var uwr = new UnityWebRequest("https://openrouter.ai/api/v1/chat/completions", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            var asyncOp = uwr.SendWebRequest();
            while (!asyncOp.isDone)
                yield return null;

#if UNITY_2022_2_OR_NEWER
            bool hasError = uwr.result != UnityWebRequest.Result.Success;
#else
            bool hasError = uwr.isNetworkError || uwr.isHttpError;
#endif
            LLMResult result = new LLMResult();
            if (hasError)
            {
                result.error = $"[Error] {uwr.error}\n{uwr.downloadHandler.text}";
            }
            else
            {
                string responseText = uwr.downloadHandler.text;
                try
                {
                    var resp = JsonUtility.FromJson<OpenRouterResponse>(responseText);
                    if (resp != null && resp.choices != null && resp.choices.Length > 0)
                    {
                        result.response = resp.choices[0].message.content;
                    }
                    else
                    {
                        result.error = "[Could not parse LLM response]";
                    }
                }
                catch
                {
                    result.error = "[Could not parse LLM response]";
                }
            }
            callback?.Invoke(result);
        }
    }
}
