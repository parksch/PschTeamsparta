using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum ScriptableType
{
    MainData
}

public class ScriptableManager : MonoBehaviour
{
    static ScriptableManager instance = null;
    public static ScriptableManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ScriptableManager>(true);

                if (instance == null)
                {
                    instance = new GameObject().AddComponent<ScriptableManager>();
                    instance.gameObject.name = "ScriptableManager";
                    DontDestroyOnLoad(instance.gameObject);
                }

            }

            return instance;
        }
    }

    Dictionary<ScriptableType, ScriptableObject> scriptableObjects = new Dictionary<ScriptableType, ScriptableObject>();

    void Awake()
    {
        scriptableObjects.Clear();
        LoadAllScriptableObjects();
    }

    void LoadAllScriptableObjects()
    {
        foreach (ScriptableType type in System.Enum.GetValues(typeof(ScriptableType)))
        {
            var asset = Resources.Load<ScriptableObject>($"ScriptableObject/{type}Scriptable");

            if(asset != null)
            {
                scriptableObjects[type] = asset;
            }
        }
    }

    public T Get<T>(ScriptableType type) where T : ScriptableObject
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            scriptableObjects.Clear();
            LoadAllScriptableObjects();
        }
#endif

        if (scriptableObjects.TryGetValue(type, out ScriptableObject obj))
        {
            return obj as T;
        }
        Debug.LogError($"ScriptableObject '{type}'을(를) 찾을 수 없습니다.");

        return null;
    }
}
