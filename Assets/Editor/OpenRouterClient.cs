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


    public IEnumerator SendPromptCoroutine(string apiKey, string prompt, Action<LLMResult> callback, string sceneContextJson = null)
    {
        string systemInstruction =
            "You are a Unity Editor assistant. Always respond ONLY with a JSON object containing a 'commands' array. " +
            "Each command is an object with a 'command' field (e.g., 'CreateObject', 'AddComponent', 'UpdateComponent'), and relevant parameters. " +
            "Do not include any explanation or text outside the JSON.\n" +
            "You will also receive the current Unity scene context as JSON. Use it to reason about what objects exist.\n" +
            "Example: If the user says 'create a red ball with physics', respond with:\n" +
            "{\n  \"commands\": [\n    { \"command\": \"CreateObject\", \"name\": \"RedBall\" },\n    { \"command\": \"AddComponent\", \"target\": \"RedBall\", \"component\": \"Rigidbody\" },\n    { \"command\": \"AddComponent\", \"target\": \"RedBall\", \"component\": \"SphereCollider\" },\n    { \"command\": \"UpdateComponent\", \"target\": \"RedBall\", \"component\": \"Renderer\", \"property\": \"material.color\", \"value\": \"red\" }\n  ]\n}";

        var messages = new List<Message>()
        {
            new Message { role = "system", content = systemInstruction }
        };
        if (!string.IsNullOrEmpty(sceneContextJson))
        {
            messages.Add(new Message { role = "user", content = "[SCENE CONTEXT]" + System.Environment.NewLine + sceneContextJson });
        }
        messages.Add(new Message { role = "user", content = prompt });

        var requestBody = new OpenRouterRequest()
        {
            model = "deepseek/deepseek-chat-v3-0324:free",
            messages = messages
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
