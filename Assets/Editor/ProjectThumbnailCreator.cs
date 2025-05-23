#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    [SerializeField] private FurnitureDataSO[] m_furnitureData;
    private VisualElement _root;
    private Camera _camera;
    private float _distance = 2;
    private Label _pathLabel;
    private string _lastOpenDirectory;
    private Button _bulkSnapshotButton;
    private GameObject _background;
    private Toggle _jpgToggle;
    private Toggle _pngToggle;
    private List<GameObject> _prefabs;

    [MenuItem("Window/AR App Tools/Project Thumbnail Creator")]
    public static void ShowExample()
    {
        ProjectThumbnailCreator wnd = GetWindow<ProjectThumbnailCreator>();
        wnd.titleContent = new GUIContent("Furniture Thumbnail Creator");
    }

    public void CreateGUI()
    {
        _renderTexture = new RenderTexture(512, 512, 24); // Adjust resolution as needed
        _renderTexture.name = "SceneViewRenderTexture";

        _root = rootVisualElement;
        
        TextField textField = new TextField();
        textField.SetValueWithoutNotify("Project Thumbnail Creator");
        
        _pathLabel = new Label("Path");
        Button button = new Button();
        Button addFolderButton = new Button();
        _bulkSnapshotButton = new Button();
        _bulkSnapshotButton.text = "Bulk Snapshot";
        _bulkSnapshotButton.clicked += OnPrefabBulkSnapshot;
        
        addFolderButton.text = "Add Folder";
        addFolderButton.clicked += AddFolder;
        button.clicked += ProcessSnapshot;
        
        var serializedObject = new SerializedObject(this);
        var property = serializedObject.FindProperty(nameof(m_furnitureData));

        var field = new PropertyField(property);
        field.Bind(serializedObject);

        rootVisualElement.Add(field);
        
        
        //_root.Add(textField);

        VisualElement options = new VisualElement();
        options.style.flexDirection = FlexDirection.Row;

        options.Add(new Label("Options"));
        ToggleButtonGroup toggleButtonGroup = new ToggleButtonGroup();
        toggleButtonGroup.style.alignSelf = Align.Center;    
        _pngToggle = new Toggle();
        _pngToggle.text = "PNG";
        
        _jpgToggle = new Toggle();
        _jpgToggle.text = "JPG";
        toggleButtonGroup.Add(_pngToggle);
        toggleButtonGroup.Add(_jpgToggle);
        options.Add(toggleButtonGroup);
        button.text = "Click me!";
        _bulkSnapshotButton.enabledSelf = false;
        _root.Add(button);
        _root.Add(addFolderButton);
        _root.Add(_pathLabel);
        _root.Add(options);
        _root.Add(_bulkSnapshotButton);
        
    }

    private void AddFolder()
    {
        _bulkSnapshotButton.enabledSelf = true;
        if(_lastOpenDirectory == string.Empty) _lastOpenDirectory = Application.dataPath;
        string directory = EditorUtility.OpenFolderPanel("Select Directory", _lastOpenDirectory, "");
        _lastOpenDirectory = directory;
        _pathLabel.text = directory ?? string.Empty;
        
        DirectoryInfo di = new DirectoryInfo(_pathLabel.text);
        var prefabFiles = di.GetFiles("*.prefab", SearchOption.AllDirectories);
        
        //var files = Directory.GetFiles(_pathLabel.text).Where(name => !name.EndsWith(".meta"));
        var assetPath = Application.dataPath;
        var length = assetPath.Length;
        _prefabs = new List<GameObject>();

        foreach (var VARIABLE in prefabFiles)
        {
            var path = VARIABLE.FullName;
            var relativePath = "Assets"+path.Remove(0, length);
            relativePath = relativePath.Replace("\\", "/");
            
            var prefab = (GameObject)AssetDatabase.LoadAssetAtPath(relativePath,typeof(GameObject));
            if (prefab != null) _prefabs.Add(prefab);
            else Debug.LogWarning("Not a prefab: "+path);
        }
        Debug.Log("Prefabs: "+_prefabs.Count);
    }

    private void OnPrefabBulkSnapshot()
    {
        /*DirectoryInfo di = new DirectoryInfo(_pathLabel.text);
        var prefabFiles = di.GetFiles("*.prefab", SearchOption.AllDirectories);
        
        var files = Directory.GetFiles(_pathLabel.text).Where(name => !name.EndsWith(".meta"));
        var assetPath = Application.dataPath;
        var length = assetPath.Length;
        List<GameObject> prefabs = new List<GameObject>();

        foreach (var VARIABLE in prefabFiles)
        {
            var path = VARIABLE.FullName;
            var relativePath = "Assets"+path.Remove(0, length);
            relativePath = relativePath.Replace("\\", "/");
            Debug.Log(relativePath);
            var prefab = (GameObject)AssetDatabase.LoadAssetAtPath(relativePath,typeof(GameObject));
            if (prefab != null) prefabs.Add(prefab);
            else Debug.LogWarning("Not a prefab: "+path);
        }
        foreach (var path in files)
        {
            var relativePath = "Assets"+path.Remove(0, length);
            relativePath = relativePath.Replace("\\", "/");
            //Debug.Log(relativePath);
                
            var prefab = (GameObject)AssetDatabase.LoadAssetAtPath(relativePath,typeof(GameObject));
            if (prefab != null) prefabs.Add(prefab);
            else Debug.LogWarning("Not a prefab: "+path);
        }*/
        TakeBulkSnapshot(_prefabs);

    }

    private async void TakeBulkSnapshot(List<GameObject> prefabs)
    {
        try
        {
            await DoBulkSnapshotAsync(prefabs);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private async Task DoBulkSnapshotAsync(List<GameObject> prefabs)
    {
        _camera = FindFirstObjectByType<Camera>();
        _background = GameObject.FindGameObjectWithTag($"background");
     
        foreach (var gameObject in prefabs)
        {
            await TakeSnapshot(gameObject);
        }
        _background.SetActive(true);
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
        _camera = FindFirstObjectByType<Camera>();
        
        foreach (var VARIABLE in m_furnitureData)
        {
            await TakeSnapshot(VARIABLE.m_prefab);
        }

        ConnectToAsset();
    }

    private async Task TakeSnapshot(GameObject variableMPrefab)
    {
        GameObject obj = Instantiate(variableMPrefab);
        PlaceCamera(obj);
        if(_jpgToggle.value) CreateSnapshotJPG(variableMPrefab.name);
        if(_pngToggle.value) CreateSnapshotPNG(variableMPrefab.name);
        await Task.Delay(100);
        
        DestroyImmediate(obj);
    }

    private void PlaceCamera(GameObject gameObject)
    {
        var bounds = gameObject.GetComponent<MeshRenderer>().bounds;
        var position = bounds.center+ _camera.transform.forward*-_distance;
        _camera.transform.position = position;
        /*var planes = GeometryUtility.CalculateFrustumPlanes(_camera);
        if (GeometryUtility.TestPlanesAABB(planes, bounds))
        {
            Debug.Log("YES");
        }
        else
        {
            Debug.Log("NO");
        }
        bounds.center = position;*/
    }

    private void CreateSnapshot(string name)
    {
        if (_camera == null) return;
        Debug.Log(_camera.name);
        
        _camera.targetTexture = _renderTexture;
        _camera.Render();
        _camera.targetTexture = null;
            
        if (_renderTexture != null)
        {
            if(_renderer == null) _renderer = new Image();
            _renderer.name = "Image";
            _renderer.sourceRect = new Rect(0, 0, _renderTexture.width, _renderTexture.height);
            _renderer.image = _renderTexture;
            _root.Add(_renderer);
            RenderTexture.active = _renderTexture;
            var tex = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, _renderTexture.width,_renderTexture.height), 0, 0);
            tex.Apply();
            File.WriteAllBytes(Application.dataPath + "/Thumbnails/png/"+name+".png", tex.EncodeToPNG());
            AssetDatabase.Refresh();
            DestroyImmediate(tex);
            
        }
    }

    private void CreateSnapshotJPG(string name)
    {
        if (_camera == null) return;
        if (_background == null) return;
        _background.SetActive(true);
        
        _camera.targetTexture = _renderTexture;
        _camera.Render();
        _camera.targetTexture = null;
            
        if (_renderTexture != null)
        {
            if(_renderer == null) _renderer = new Image();
            _renderer.name = "Image";
            _renderer.sourceRect = new Rect(0, 0, _renderTexture.width, _renderTexture.height);
            _renderer.image = _renderTexture;
            _root.Add(_renderer);
            RenderTexture.active = _renderTexture;
            var tex = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, _renderTexture.width,_renderTexture.height), 0, 0);
            tex.Apply();
            File.WriteAllBytes(Application.dataPath + "/Thumbnails/jpg/"+name+".jpg", tex.EncodeToJPG());
            AssetDatabase.Refresh();
            DestroyImmediate(tex);
            
        }
    }
    
    private void CreateSnapshotPNG(string name)
    {
        if (_camera == null) return;
        if (_background == null) return;
        _background.SetActive(false);
        
        _camera.targetTexture = _renderTexture;
        _camera.Render();
        _camera.targetTexture = null;
            
        if (_renderTexture != null)
        {
            if(_renderer == null) _renderer = new Image();
            _renderer.name = "Image";
            _renderer.sourceRect = new Rect(0, 0, _renderTexture.width, _renderTexture.height);
            _renderer.image = _renderTexture;
            _root.Add(_renderer);
            RenderTexture.active = _renderTexture;
            var tex = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, _renderTexture.width,_renderTexture.height), 0, 0);
            tex.Apply();
            File.WriteAllBytes(Application.dataPath + "/Thumbnails/png/"+name+".png", tex.EncodeToPNG());
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
                VARIABLE.m_thumbnailPNG = textureObject;
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
        if (!lowerCaseAssetPath.Contains("thumbnails"))
            return;
        
        TextureImporter textureImporter  = (TextureImporter)assetImporter;
        textureImporter.convertToNormalmap = true;
        
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Single;
    }
}