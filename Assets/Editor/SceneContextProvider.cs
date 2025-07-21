using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class SceneContextProvider
{
    [System.Serializable]
    public class SceneContext
    {
        public string sceneName;
        public List<GameObjectInfo> gameObjects = new List<GameObjectInfo>();
    }
    [System.Serializable]
    public class GameObjectInfo
    {
        public string name;
        public List<string> components = new List<string>();
    }

    public static string GetCurrentSceneContextJson()
    {
        var context = new SceneContext();
        var scene = SceneManager.GetActiveScene();
        context.sceneName = scene.name;
        foreach (var go in scene.GetRootGameObjects())
        {
            AddGameObjectRecursive(go, context.gameObjects);
        }
        return JsonUtility.ToJson(context, true);
    }

    private static void AddGameObjectRecursive(GameObject go, List<GameObjectInfo> list)
    {
        var info = new GameObjectInfo();
        info.name = go.name;
        foreach (var comp in go.GetComponents<Component>())
        {
            if (comp != null)
                info.components.Add(comp.GetType().Name);
        }
        list.Add(info);
        foreach (Transform child in go.transform)
        {
            AddGameObjectRecursive(child.gameObject, list);
        }
    }
}
