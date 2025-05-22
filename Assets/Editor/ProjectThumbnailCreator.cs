using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;
using Task = System.Threading.Tasks.Task;

public class ProjectThumbnailCreator : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private RenderTexture _renderTexture;
    private Image _renderer;
    public FurnitureDataSO[] prefabs;
    private VisualElement _root;
    [MenuItem("Window/UI Toolkit/ProjectThumbnailCreator")]
    public static void ShowExample()
    {
        ProjectThumbnailCreator wnd = GetWindow<ProjectThumbnailCreator>();
        wnd.titleContent = new GUIContent("ProjectThumbnailCreator");
        

    }

    public void CreateGUI()
    {
        _renderTexture = new RenderTexture(512, 512, 24); // Adjust resolution as needed
        _renderTexture.name = "SceneViewRenderTexture";
        
        // Each editor window contains a root VisualElement object
        _root = rootVisualElement;
        
        TextField textField = new TextField();
        textField.SetValueWithoutNotify("Project Thumbnail Creator");
        Label PathLabel = new Label("Path");


        Button button = new Button();
        Button connect = new Button();
        button.clicked += Test;
        connect.clicked += ConnectToAsset;
                
        var serializedObject = new SerializedObject(this);
        var property = serializedObject.FindProperty(nameof(prefabs));

        var field = new PropertyField(property);
        field.Bind(serializedObject);

        rootVisualElement.Add(field);
        
        _root.Add(PathLabel);
        _root.Add(textField);

        button.text = "Click me!";
        connect.text = "Connect me!";

        _root.Add(button);
        _root.Add(connect);
        
    }

    private async void Test()
    {
        try
        {
            await PrintData();
            
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private async Task PrintData()
    {
        
        foreach (var VARIABLE in prefabs)
        {
            Debug.Log(VARIABLE.name);
            GameObject obj = Instantiate(VARIABLE.m_prefab);
            CreateSnapshot(VARIABLE);
            await Task.Delay(1000);
            DestroyImmediate(obj);
            
            
        }

        ConnectToAsset();
    }

    private void CreateSnapshot(FurnitureDataSO data)
    {
        Camera _camera = FindObjectOfType<Camera>();
        if(_camera != null) Debug.Log(_camera.name);
            
        _camera.targetTexture = _renderTexture;
        _camera.Render();
        _camera.targetTexture = null;
            
        if (_renderTexture != null)
        {
            Debug.Log("Button Clicked");

            if(_renderer == null) _renderer = new Image();
            _renderer.name = "Image";
            _renderer.sourceRect = new Rect(0, 0, _renderTexture.width, _renderTexture.height);
            _renderer.image = _renderTexture;
            _root.Add(_renderer);
            RenderTexture.active = _renderTexture;
            var tex = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, _renderTexture.width,_renderTexture.height), 0, 0);
            tex.Apply();
            File.WriteAllBytes(Application.dataPath + "/Thumbnails/"+data.m_prefab+".jpg", tex.EncodeToJPG());
            AssetDatabase.Refresh();
            DestroyImmediate(tex);
            
        }
    }

    private void ConnectToAsset()
    {
        foreach (var VARIABLE in prefabs)
        {
            Sprite textureObject = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Thumbnails/"+VARIABLE.m_prefab+".jpg", typeof(Sprite));
            if (textureObject != null)
            {
                Debug.Log(textureObject.name);
                VARIABLE.m_thumbnail = textureObject;
                EditorUtility.SetDirty(VARIABLE);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.Log(VARIABLE.m_prefab + " not found");
            }
            
            
        }
        
    }
}


public class PostProcessImportAsset : AssetPostprocessor
{
    //Based on this example, the output from this function should be:
    //  OnPostprocessAllAssets
    //  Imported: Assets/Artifacts/test_file01.txt
    //
    //test_file02.txt should not even show up on the Project Browser
    //until a refresh happens.
    /*static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        Debug.Log("OnPostprocessAllAssets");

        foreach (var imported in importedAssets)
            Debug.Log("Imported: " + imported);

        foreach (var deleted in deletedAssets)
            Debug.Log("Deleted: " + deleted);

        foreach (var moved in movedAssets)
            Debug.Log("Moved: " + moved);

        foreach (var movedFromAsset in movedFromAssetPaths)
            Debug.Log("Moved from Asset: " + movedFromAsset);
    }*/

    void OnPreprocessTexture()
    {
        
        string lowerCaseAssetPath = assetPath.ToLower();
        Debug.Log("OnPreprocessTexture: "+lowerCaseAssetPath);
        if (!lowerCaseAssetPath.Contains("thumbnails"))
            return;
        
        TextureImporter textureImporter  = (TextureImporter)assetImporter;
        textureImporter.convertToNormalmap = true;
        
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Single;
        
        Debug.Log(lowerCaseAssetPath);
    }
}