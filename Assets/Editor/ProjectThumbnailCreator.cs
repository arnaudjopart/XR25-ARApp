using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Task = System.Threading.Tasks.Task;

public class ProjectThumbnailCreator : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private RenderTexture _renderTexture;
    private Image _renderer;
    [FormerlySerializedAs("prefabs")] public FurnitureDataSO[] m_furnitureData;
    private VisualElement _root;
    [MenuItem("Window/AR App Tools/Project Thumbnail Creator")]
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

        button.clicked += ProcessSnapshot;

                
        var serializedObject = new SerializedObject(this);
        var property = serializedObject.FindProperty(nameof(m_furnitureData));

        var field = new PropertyField(property);
        field.Bind(serializedObject);

        rootVisualElement.Add(field);
        
        _root.Add(PathLabel);
        _root.Add(textField);

        button.text = "Click me!";

        _root.Add(button);
    }

    private async void ProcessSnapshot()
    {
        try
        {
            await DoSnapshotAsync();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private async Task DoSnapshotAsync()
    {
        foreach (var VARIABLE in m_furnitureData)
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
        Camera camera = FindFirstObjectByType<Camera>();
        if(camera != null) Debug.Log(camera.name);
            
        camera.targetTexture = _renderTexture;
        camera.Render();
        camera.targetTexture = null;
            
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
        foreach (var VARIABLE in m_furnitureData)
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