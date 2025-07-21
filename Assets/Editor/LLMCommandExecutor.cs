using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

public static class LLMCommandExecutor
{
    [Serializable]
    public class LLMCommand
    {
        public string command;
        public string name; // For CreateObject
        public string target; // For UpdateComponent, AnimateObject, etc.
        public string component; // For AddComponent, UpdateComponent
        public string property; // For UpdateComponent
        public string value; // For UpdateComponent
        // Add more fields as needed
    }

    public static void ExecuteCommands(string json)
    {
        try
        {
            var wrapper = JsonUtility.FromJson<LLMCommandArrayWrapper>(json);
            if (wrapper != null && wrapper.commands != null)
            {
                foreach (var cmd in wrapper.commands)
                {
                    ExecuteCommand(cmd);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LLMCommandExecutor] Failed to parse/execute commands: {ex.Message}\n{json}");
        }
    }

    private static void ExecuteCommand(LLMCommand cmd)
    {
        switch (cmd.command)
        {
            case "CreateObject":
                var go = new GameObject(cmd.name);
                Undo.RegisterCreatedObjectUndo(go, "LLM Create Object");
                break;
            case "AddComponent":
                var targetGo = GameObject.Find(cmd.target);
                if (targetGo != null)
                {
                    var type = Type.GetType(cmd.component) ?? Type.GetType($"UnityEngine.{cmd.component}, UnityEngine");
                    if (type != null && type.IsSubclassOf(typeof(Component)))
                    {
                        Undo.AddComponent(targetGo, type);
                    }
                }
                break;
            case "UpdateComponent":
                var go2 = GameObject.Find(cmd.target);
                if (go2 != null)
                {
                    var comp = go2.GetComponent(cmd.component);
                    if (comp != null)
                    {
                        var prop = comp.GetType().GetProperty(cmd.property);
                        if (prop != null)
                        {
                            object val = Convert.ChangeType(cmd.value, prop.PropertyType);
                            prop.SetValue(comp, val);
                        }
                    }
                }
                break;
            // Add more command cases as needed
            default:
                Debug.LogWarning($"[LLMCommandExecutor] Unknown command: {cmd.command}");
                break;
        }
    }

    [Serializable]
    private class LLMCommandArrayWrapper
    {
        public LLMCommand[] commands;
    }
}
