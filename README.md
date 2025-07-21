# Unity LLM Chat Editor Plugin

This plugin adds a dockable chat window to the Unity Editor that connects to OpenRouter (OpenAI-compatible LLMs) for AI-powered assistance.

## Features
- Dockable chat window in Unity Editor (Window > LLM Chat)
- Sends prompts to OpenRouter LLMs and displays responses
- Modular code: UI and API logic are separated
- API key stored securely in EditorPrefs

## Setup Instructions

1. **Copy Files**
   - Place `LLMChatWindow.cs` and `OpenRouterClient.cs` in your project's `Assets/Editor/` folder.

2. **Open the Chat Window**
   - In Unity, go to `Window > LLM Chat` to open the chat interface.

3. **Enter Your OpenRouter API Key**
   - Enter your OpenRouter API key in the field at the top of the chat window. The key is stored securely in Unity's EditorPrefs.
   - You can get an API key from https://openrouter.ai/

4. **Start Chatting**
   - Type your prompt and click `Send`. The LLM's response will appear in the chat history.

## Requirements
- Unity 2021 or newer
- Internet connection
- OpenRouter API key

## Customization
- The LLM model can be changed in `OpenRouterClient.cs` (default: `openai/gpt-3.5-turbo`).
- You can further extend the plugin to send scene context, handle commands, or integrate with other Unity systems.

## Troubleshooting
- If you see errors or no response, check your API key and internet connection.
- Errors from the API will be shown in the chat window.

---

**This is an early version. Contributions and improvements are welcome!**
