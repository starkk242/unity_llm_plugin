
# Unity LLM Chat Editor Plugin

This plugin adds a dockable chat window to the Unity Editor that connects to OpenRouter (OpenAI-compatible LLMs) for AI-powered assistance. The LLM can directly create and modify GameObjects in your scene by returning JSON commands.

## Current Capabilities

- Dockable chat window in Unity Editor (`Window > LLM Chat`)
- Sends prompts to OpenRouter LLMs and displays responses
- LLM is instructed to always respond with a JSON object containing a `commands` array
- The agent can:
  - Create new GameObjects
  - Add components (e.g., Rigidbody, Collider, Renderer) to GameObjects
  - Update component properties (e.g., set color)
- Executes these commands in the Unity Editor scene automatically
- Modular code: UI, API logic, and command execution are separated
- API key stored securely in EditorPrefs

## What You Can Ask For (Examples)

You can use natural language to ask for scene changes. The LLM will respond with the correct command JSON, and the agent will execute it.

**Example 1: Create a red ball with physics**

Prompt:
```
Create a red ball with physics
```

LLM Response (executed automatically):
```
{
  "commands": [
    { "command": "CreateObject", "name": "RedBall" },
    { "command": "AddComponent", "target": "RedBall", "component": "Rigidbody" },
    { "command": "AddComponent", "target": "RedBall", "component": "SphereCollider" },
    { "command": "UpdateComponent", "target": "RedBall", "component": "Renderer", "property": "material.color", "value": "red" }
  ]
}
```

**Example 2: Add a BoxCollider to the Main Camera**

Prompt:
```
Add a BoxCollider to the Main Camera
```

LLM Response:
```
{
  "commands": [
    { "command": "AddComponent", "target": "Main Camera", "component": "BoxCollider" }
  ]
}
```

**Example 3: Change the color of Cube to blue**

Prompt:
```
Change the color of Cube to blue
```

LLM Response:
```
{
  "commands": [
    { "command": "UpdateComponent", "target": "Cube", "component": "Renderer", "property": "material.color", "value": "blue" }
  ]
}
```

## Setup Instructions

1. **Copy Files**
   - Place `LLMChatWindow.cs`, `OpenRouterClient.cs`, and `LLMCommandExecutor.cs` in your project's `Assets/Editor/` folder.

2. **Open the Chat Window**
   - In Unity, go to `Window > LLM Chat` to open the chat interface.

3. **Enter Your OpenRouter API Key**
   - Enter your OpenRouter API key in the field at the top of the chat window. The key is stored securely in Unity's EditorPrefs.
   - You can get an API key from https://openrouter.ai/

4. **Start Chatting**
   - Type your prompt and click `Send`. The LLM's response will appear in the chat history and, if it contains commands, will be executed in your scene.

## Requirements
- Unity 2021 or newer
- Internet connection
- OpenRouter API key

## Customization
- The LLM model can be changed in `OpenRouterClient.cs` (default: `openai/gpt-3.5-turbo`).
- You can further extend the plugin to send scene context, handle more commands, or integrate with other Unity systems.

## Troubleshooting
- If you see errors or no response, check your API key and internet connection.
- Errors from the API will be shown in the chat window.

---

**This is an early version. Contributions and improvements are welcome!**
