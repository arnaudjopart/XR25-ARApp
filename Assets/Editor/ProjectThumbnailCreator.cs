using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ProjectThumbnailCreator : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private RenderTexture _renderTexture;
    private Image _renderer;

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
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        Button button = new Button(() =>
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
                root.Add(_renderer);
                var tex = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false);
                tex.ReadPixels(new Rect(0, 0, 500,500), 0, 0);
                tex.Apply();
                File.WriteAllBytes(Application.dataPath + "/image.png", tex.EncodeToPNG());
            }
        });
        button.text = "Click me!";
        //button.style.height = new Length(50, LengthUnit.Pixel);
        root.Add(label);
        root.Add(button);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
    }
}
