using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class LLMChatWindow : EditorWindow
{
    private List<string> chatHistory = new List<string>();
    private string userInput = "";
    private Vector2 scrollPos;
    private bool isAwaitingResponse = false;

    private const string ApiKeyEditorPref = "LLMChat_OpenRouterApiKey";
    private string apiKey = "";

    private OpenRouterClient openRouterClient = new OpenRouterClient();
    private IEnumerator<System.Object> coroutine;

    [MenuItem("Window/LLM Chat")]
    public static void ShowWindow()
    {
        GetWindow<LLMChatWindow>("LLM Chat");
    }

    private void OnEnable()
    {
        apiKey = EditorPrefs.GetString(ApiKeyEditorPref, "");
    }

    private void OnGUI()
    {
        GUILayout.Label("LLM Chat Interface", EditorStyles.boldLabel);

        // API Key field (hidden by default, but can be set)
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("API Key:", GUILayout.Width(60));
        string newApiKey = EditorGUILayout.PasswordField(apiKey);
        if (newApiKey != apiKey)
        {
            apiKey = newApiKey;
            EditorPrefs.SetString(ApiKeyEditorPref, apiKey);
        }
        EditorGUILayout.EndHorizontal();

        // Chat history
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
        foreach (var msg in chatHistory)
        {
            GUILayout.Label(msg, EditorStyles.wordWrappedLabel);
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        // User input
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = !isAwaitingResponse;
        userInput = EditorGUILayout.TextField(userInput);
        if (GUILayout.Button("Send", GUILayout.Width(60)) && !isAwaitingResponse)
        {
            SendMessageToChat();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    private void SendMessageToChat()
    {
        if (!string.IsNullOrWhiteSpace(userInput) && !isAwaitingResponse)
        {
            chatHistory.Add($"You: {userInput}");
            string promptToSend = userInput;
            userInput = "";
            scrollPos.y = float.MaxValue;
            isAwaitingResponse = true;
            Repaint();
            // Gather scene context
            string sceneContextJson = SceneContextProvider.GetCurrentSceneContextJson();
            // Start coroutine for async API call
            EditorApplication.update += EditorCoroutine;
            coroutine = SendPromptCoroutine(promptToSend, sceneContextJson);
        }
    }

    // --- EditorCoroutine implementation for Editor scripts ---
    private void EditorCoroutine()
    {
        if (coroutine == null || !coroutine.MoveNext())
        {
            EditorApplication.update -= EditorCoroutine;
        }
    }

    private IEnumerator<System.Object> SendPromptCoroutine(string prompt, string sceneContextJson)
    {
        bool done = false;
        string llmReply = null;
        string error = null;
        yield return openRouterClient.SendPromptCoroutine(apiKey, prompt, (result) => {
            if (!string.IsNullOrEmpty(result.error))
                error = result.error;
            else
                llmReply = result.response;
            done = true;
        }, sceneContextJson);
        while (!done) yield return null;
        if (!string.IsNullOrEmpty(error))
        {
            chatHistory.Add($"LLM: {error}");
        }
        else
        {
            chatHistory.Add($"LLM: {llmReply}");
            // Try to detect and execute LLM commands in JSON format
            if (llmReply != null && llmReply.TrimStart().StartsWith("{") && llmReply.Contains("\"commands\""))
            {
                try
                {
                    LLMCommandExecutor.ExecuteCommands(llmReply);
                    chatHistory.Add("[LLMCommandExecutor] Executed commands from response.");
                }
                catch (System.Exception ex)
                {
                    chatHistory.Add($"[LLMCommandExecutor] Error: {ex.Message}");
                }
            }
        }
        isAwaitingResponse = false;
        scrollPos.y = float.MaxValue;
        Repaint();
    }
}
