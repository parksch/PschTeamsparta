#if UNITY_EDITOR 

using UnityEditor;
using UnityEngine;
using System.IO;
using Unity.Jobs;
using Newtonsoft.Json;


#if JsonLoader
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Text;
using System;
using Newtonsoft.Json.Linq;
using JsonClass;
#else
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif

public class JsonLoader : EditorWindow
{
    static string jsonFilePath = "Assets/JsonFiles";
    static string scriptableObjectPath = "Assets/Resources/ScriptableObject";
    static string scriptableFunctionPath = "Assets/Scripts/Scriptable/Function";
    static string scriptableDataPath = "Assets/Scripts/Scriptable/Data";
    static string scriptableManagerPath = "Assets/Scripts/Scriptable/Manager";

#if JsonLoader
    static bool isButtonEnabled = true;

    [MenuItem("Tools/JsonLoader")]
    public static void ShowMyEditor()
    {
        EditorWindow window = GetWindow<JsonLoader>();
        window.titleContent = new GUIContent("JsonLoader");
        window.minSize = new Vector2(500, 500);
    }

    public void OnGUI()
    {
        EditorGUILayout.LabelField("Json 파일을 .cs 파일로 생성");

        if (!IsFileEmpty(jsonFilePath, "*.json") && isButtonEnabled)
        {
            GUI.enabled = true;
        }
        else
        {
            GUI.enabled = false;
        }

        if (GUILayout.Button("Conversion"))
        {
            isButtonEnabled = false;
            OnClickJsonConversion();
        }

        EditorGUILayout.LabelField(".cs 파일을 ScriptableObject 파일로 생성");

        if (!IsFileEmpty(scriptableDataPath, "*.cs") && isButtonEnabled)
        {
            GUI.enabled = true;
        }
        else
        {
            GUI.enabled = false;
        }

        if (GUILayout.Button("CreateObject"))
        {
            isButtonEnabled = false;
            OnClickCsConversion();
        }

        EditorGUILayout.LabelField("*개발자 Test 용: Scriptable 파일을 Json으로 만들어주는 프로그램* (에러 발생 가능성)");

        if (!IsFileEmpty(scriptableObjectPath,"*.asset") && isButtonEnabled)
        {
            GUI.enabled = true;
        }
        else
        {
            GUI.enabled = false;
        }

        if (GUILayout.Button("Conversion"))
        {
            isButtonEnabled = false;
            OnClickScriptableConversion();
        }
    }

    bool IsFileEmpty(string path,string extension)
    {
        DirectoryInfo infos = new DirectoryInfo(path);
        return !(infos.GetFiles(extension).Length > 0);
    }

    void OnClickJsonConversion()
    {
        AddDirectory(scriptableDataPath);
        AddDirectory(scriptableFunctionPath);
        string fileName = string.Empty;

        try
        {
            DirectoryInfo info = new DirectoryInfo(jsonFilePath);
            List<string> csFileNames = new List<string>();

            foreach (FileInfo file in info.GetFiles("*.json"))
            {
                fileName = Path.GetFileNameWithoutExtension(file.Name);
                string path = Path.Combine(jsonFilePath, file.Name);

                csFileNames.Add(char.ToUpper(fileName[0]) + fileName.Substring(1));
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

                JToken jsonToken = JToken.Parse(textAsset.text);
                JObject jObject = jsonToken is JArray array ? (JObject)array[0] : (JObject)jsonToken;

                string classString = GenerateClassFromJson(jObject, fileName, jsonToken is JArray);
                File.WriteAllText(scriptableDataPath + "/" + fileName + "Scriptable.cs", classString);

                if (!File.Exists(scriptableFunctionPath + "/" + fileName + "Function" + ".cs"))
                {
                    string partialFunction = GenerateClassFromJson(jObject, fileName, jsonToken is JArray, true, true);
                    File.WriteAllText(scriptableFunctionPath + "/" + fileName + "Function" + ".cs", partialFunction);
                }
            }

            GenerateScriptableManager(csFileNames);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("결과", "cs 파일 생성 성공", "확인");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("결과", fileName + "\n" + "cs 파일 생성 실패 \n" + e.Message, "확인");
        }

        isButtonEnabled = true;
    }

    void OnClickCsConversion()
    {
        AddDirectory(scriptableObjectPath);
        string fileName = string.Empty;

        try
        {
            DirectoryInfo info = new DirectoryInfo(jsonFilePath);

            string[] assetPaths = Directory.GetFiles(scriptableObjectPath, "*.asset");

            foreach (var assetPath in assetPaths)
            {
                if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath) != null)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }

            foreach (FileInfo file in info.GetFiles("*.json"))
            {
                fileName = Path.GetFileNameWithoutExtension(file.Name);
                string path = Path.Combine(jsonFilePath, file.Name);
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                JToken jsonToken = JToken.Parse(textAsset.text);

                CreateScriptableAsset(scriptableObjectPath, fileName, jsonToken);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("결과", "ScriptableObject 생성 성공", "확인");
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("결과", fileName + "\n" + "ScriptableObject 생성 실패\n" + e.Message, "확인");
        }

        isButtonEnabled = true;
    }

    void OnClickScriptableConversion()
    {
        AddDirectory(jsonFilePath);

        try
        {
            string json = string.Empty;

            List<MainData> mainDatas = ScriptableManager.Instance.Get<MainDataScriptable>(ScriptableType.MainData).mainData;
            json = JsonConvert.SerializeObject(mainDatas);
            File.WriteAllText(jsonFilePath + "/MainData.json", json);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("결과", "Scriptable 에서 Json 변환", "확인");
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("결과", "Scriptable 에서 Json 변환 실패\n" + e.Message, "확인");
        }

        isButtonEnabled = true;
    }

    void CreateScriptableAsset(string path, string name, JToken jToken)
    {
        name = char.ToUpper(name[0]) + name.Substring(1);
        string dataPath = Path.Combine(path, $"{name}Scriptable.asset");
        Assembly assembly = Assembly.Load("Assembly-CSharp");
        System.Type scriptableType = assembly.GetType($"JsonClass.{name}Scriptable");

        ScriptableObject scriptableObject = null;

        if (jToken is JArray)
        {
            scriptableObject = CreateInstance(scriptableType);
            System.Type classType = assembly.GetType($"JsonClass.{name}");
            JArray jArray = (JArray)jToken;
            FieldInfo listField = scriptableType.GetField(char.ToLower(name[0]) + name.Substring(1), BindingFlags.Public | BindingFlags.Instance);
            IList listInstance = null;

            if (listField != null && typeof(IList).IsAssignableFrom(listField.FieldType))
            {
                listInstance = listField.GetValue(scriptableObject) as IList;
                if (listInstance == null)
                {
                    listInstance = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(classType));
                    listField.SetValue(scriptableObject, listInstance);
                }
            }

            foreach (JObject jObject in jArray)
            {
                object var = jObject.ToObject(classType);
                listInstance.Add(var);
            }
        }
        else if(jToken is JObject)
        {
            JObject jObject = (JObject)jToken;
            var instance = jObject.ToObject(scriptableType);
            scriptableObject = instance as ScriptableObject;
        }

        AssetDatabase.CreateAsset(scriptableObject, dataPath);
    }

    void GenerateScriptableManager(List<string> fileNames)
    {
        AddDirectory(scriptableManagerPath);

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("using System.Collections.Generic;");
        stringBuilder.AppendLine("using UnityEditor;");
        stringBuilder.AppendLine("using UnityEngine;");
        stringBuilder.AppendLine();

        stringBuilder.AppendLine("public enum ScriptableType");
        stringBuilder.AppendLine("{");

        for (int i = 0; i < fileNames.Count; i++)
        {
            if (i < fileNames.Count -1)
            {
                stringBuilder.AppendLine("    " + fileNames[i] + ",");
            }
            else
            {
                stringBuilder.AppendLine("    " + fileNames[i]);
            }
        }

        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();

        stringBuilder.AppendLine("public class ScriptableManager : MonoBehaviour");
        stringBuilder.AppendLine("{");

        stringBuilder.AppendLine("    static ScriptableManager instance = null;");

        stringBuilder.AppendLine("    public static ScriptableManager Instance");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        get");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            if (instance == null)");
        stringBuilder.AppendLine("            {");
        stringBuilder.AppendLine("                instance = FindObjectOfType<ScriptableManager>(true);");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("                if (instance == null)");
        stringBuilder.AppendLine("                {");
        stringBuilder.AppendLine("                    instance = new GameObject().AddComponent<ScriptableManager>();");
        stringBuilder.AppendLine("                    instance.gameObject.name = \"ScriptableManager\";");
        stringBuilder.AppendLine("                    DontDestroyOnLoad(instance.gameObject);");
        stringBuilder.AppendLine("                }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("            }");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("            return instance;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine();

        stringBuilder.AppendLine("    Dictionary<ScriptableType, ScriptableObject> scriptableObjects = new Dictionary<ScriptableType, ScriptableObject>();");
        stringBuilder.AppendLine();

        stringBuilder.AppendLine("    void Awake()");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        scriptableObjects.Clear();");
        stringBuilder.AppendLine("        LoadAllScriptableObjects();");
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine();

        stringBuilder.AppendLine("    void LoadAllScriptableObjects()");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        foreach (ScriptableType type in System.Enum.GetValues(typeof(ScriptableType)))");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            var asset = Resources.Load<ScriptableObject>($\"ScriptableObject/{type}Scriptable\");");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("            if(asset != null)");
        stringBuilder.AppendLine("            {");
        stringBuilder.AppendLine("                scriptableObjects[type] = asset;");
        stringBuilder.AppendLine("            }");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine();

        stringBuilder.AppendLine("    public T Get<T>(ScriptableType type) where T : ScriptableObject");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("#if UNITY_EDITOR");
        stringBuilder.AppendLine("        if (!EditorApplication.isPlaying)");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            scriptableObjects.Clear();");
        stringBuilder.AppendLine("            LoadAllScriptableObjects();");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("#endif");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        if (scriptableObjects.TryGetValue(type, out ScriptableObject obj))");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendLine("            return obj as T;");
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("        Debug.LogError($\"ScriptableObject '{type}'을(를) 찾을 수 없습니다.\");");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("        return null;");
        stringBuilder.AppendLine("    }");

        stringBuilder.AppendLine("}");

        File.WriteAllText(scriptableManagerPath + "/ScriptableManager.cs", stringBuilder.ToString());
    }

    string GenerateClassFromJson(JObject jsonObject, string className, bool isArray = true, bool isFirst = true, bool isFunction = false)
    {
        className = char.ToUpper(className[0]) + className.Substring(1);
        StringBuilder stringBuilder = new StringBuilder();
        List<JProperty> properties = new List<JProperty>();

        if (isFirst)
        {
            stringBuilder.AppendLine("using System.Collections.Generic;");
            stringBuilder.AppendLine("using UnityEngine;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("namespace JsonClass");
            stringBuilder.AppendLine("{");

            if (!isFunction)
            {
                stringBuilder.AppendLine("    public partial class " + className + "Scriptable" + " : ScriptableObject");
            }
            else
            {
                stringBuilder.AppendLine("    public partial class " + className + "Scriptable" + " // This Class is a functional Class.");
            }

            if (isArray)
            {
                stringBuilder.AppendLine("    {");

                if (!isFunction)
                {
                    string propertyName = char.ToLower(className[0]) + className.Substring(1);

                    stringBuilder.AppendLine($"        public List<{className}> {propertyName} = new List<{className}>();");
                }

                stringBuilder.AppendLine("    }");
                stringBuilder.AppendLine();
            }
        }

        if (!isFunction && (isArray || (!isArray && !isFirst)))
        {
            stringBuilder.AppendLine("    [System.Serializable]");
        }

        if (isArray || (!isArray && !isFirst))
        {
            stringBuilder.AppendLine("    public partial class " + className + (isFunction ? " // This Class is a functional Class." :""));
        }

        stringBuilder.AppendLine("    {");

        foreach (var property in jsonObject.Properties())
        {
            string propertyName = char.ToLower(property.Name[0]) + property.Name.Substring(1);
            string propertyType;

            if (property.Value.Type == JTokenType.Object)
            {
                propertyType = char.ToUpper(property.Name[0]) + property.Name.Substring(1);

                if (!isFunction)
                {
                    stringBuilder.AppendLine($"        public {propertyType} {propertyName};");
                }
                properties.Add(property);
            }
            else if (property.Value.Type == JTokenType.Array && ((JArray)property.Value)[0].Type == JTokenType.Object)
            {
                propertyType = char.ToUpper(property.Name[0]) + property.Name.Substring(1);

                JArray datasArray = (JArray)property.Value;
                if (!isFunction)
                {
                    stringBuilder.AppendLine($"        public List<{propertyType}> {propertyName};");
                }
                properties.Add(property);
            }
            else
            {
                propertyType = GetVariableTypeFromJson(property.Value);
                if (!isFunction)
                {
                    stringBuilder.AppendLine($"        public {propertyType} {propertyName};");
                }
            }

        }

        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine();

        foreach (var property in properties)
        {
            string propertyName = char.ToLower(property.Name[0]) + property.Name.Substring(1);
            string propertyType;

            if (property.Value.Type == JTokenType.Object)
            {
                propertyType = char.ToLower(property.Name[0]) + property.Name.Substring(1);
                stringBuilder.Append(GenerateClassFromJson((JObject)property.Value, propertyName, isArray, false, isFunction));
            }
            else if (property.Value.Type == JTokenType.Array && ((JArray)property.Value)[0].Type == JTokenType.Object)
            {
                propertyType = char.ToUpper(property.Name[0]) + property.Name.Substring(1);
                JArray datasArray = (JArray)property.Value;

                stringBuilder.Append(GenerateClassFromJson((JObject)datasArray[0], propertyName, isArray, false, isFunction));
            }
        }

        if (isFirst)
        {
            stringBuilder.AppendLine("}");
        }

        return stringBuilder.ToString();
    }

    string GetVariableTypeFromJson(JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Integer:
                return "int";
            case JTokenType.Float:
                return "float";
            case JTokenType.String:
                return "string";
            case JTokenType.Boolean:
                return "bool";
            case JTokenType.Array:
                return "List<" + GetVariableTypeFromJson(((JArray)token)[0]) + ">";
            case JTokenType.Object:
                return "object";
            default:
                return "string";
        }
    }

#else

    private static ListRequest _listRequest;
    private const string TargetPackageName = "com.unity.nuget.newtonsoft-json";
    private const string DefineSymbol = "JsonLoader";

    [InitializeOnLoadMethod]
    private static void CheckAndAddDefineSymbol()
    {
        _listRequest = Client.List();
        EditorApplication.update += Progress;
    }

    private static void Progress()
    {
        if (!_listRequest.IsCompleted) return;

        if (_listRequest.Status == StatusCode.Success)
        {
            bool packageExists = false;

            foreach (var package in _listRequest.Result)
            {
                if (package.name == TargetPackageName)
                {
                    packageExists = true;
                    break;
                }
            }

            if (packageExists)
            {
                AddDefineSymbol(DefineSymbol);
            }
            else
            {
                Debug.LogWarning($"Package '{TargetPackageName}' not found. Add Package '{TargetPackageName}'.");
            }
        }
        else if (_listRequest.Status >= StatusCode.Failure)
        {
            Debug.LogError($"Failed to list packages: {_listRequest.Error.message}");
        }

        EditorApplication.update -= Progress; 
    }

    private static void AddDefineSymbol(string symbol)
    {
        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        AddDirectory(jsonFilePath);
        AddDirectory(scriptableObjectPath);
        AddDirectory(scriptableFunctionPath);
        AddDirectory(scriptableDataPath);
        AddDirectory(scriptableManagerPath);

        if (!currentSymbols.Contains(symbol))
        {
            currentSymbols = string.IsNullOrEmpty(currentSymbols)
                ? symbol
                : $"{currentSymbols};{symbol}";

            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, currentSymbols);
            Debug.Log($"Define symbol '{symbol}' added.");
        }
        else
        {
            Debug.Log($"Define symbol '{symbol}' already exists.");
        }
    }

#endif
    private static void AddDirectory(string path)
    {
        string[] files = path.Split("/");
        string directory = string.Empty;

        foreach (string file in files)
        {
            directory += file;

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            directory += "/";
        }
    }
}
#endif