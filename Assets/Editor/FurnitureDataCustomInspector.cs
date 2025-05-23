using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(FurnitureDataSO))]
public class FurnitureDataCustomInspector : Editor
{
    private FurnitureDataSO _furnitureDataSO;
    private Image _previewImage;

    private void OnEnable()
    {
        _furnitureDataSO = (FurnitureDataSO)target;
    }

    public override VisualElement CreateInspectorGUI()
    {
        GameObject furnitureObject = _furnitureDataSO.m_prefab;
        
        
        VisualElement root = new VisualElement();
        Foldout inspector = new Foldout();
        InspectorElement.FillDefaultInspector(inspector, serializedObject, this);
        Button button = new Button();
        button.text = "Look for Thumbnail";
        button.clicked += LookForThumbnail;
        DropdownField dropdownField = new DropdownField();

        string[] layers = Enumerable.Range(0, 31).Select(index => LayerMask.LayerToName(index))
            .Where(l => !string.IsNullOrEmpty(l)).ToArray();

        var sortingLayers = layers;
        dropdownField.choices = sortingLayers.ToList();
        if (furnitureObject != null)
        {
            var layer = furnitureObject.layer;
            dropdownField.SetValueWithoutNotify(LayerMask.LayerToName(layer));
        }
        
        dropdownField.RegisterValueChangedCallback(evt =>
        {
            _furnitureDataSO.m_prefab.layer = LayerMask.NameToLayer(evt.newValue);
            AssetDatabase.SaveAssets();
        });
        root.Add(inspector);
        root.Add(button);
        root.Add(dropdownField);

        _previewImage = new Image();
        root.Add(_previewImage);
        if (_furnitureDataSO.m_thumbnailJPG != null)
        {
            _previewImage.sprite = _furnitureDataSO.m_thumbnailJPG;
        }
        else
        {
            _previewImage.sprite = null;
            _previewImage.enabledSelf = false;
        }
        return root;
    }

    private void LookForThumbnail()
    {
        if (_furnitureDataSO.m_prefab == null)
        {
            Debug.LogWarning("No prefab");
            return;
        }

        Debug.Log("Looking for prefab");
        var thumbnailPNG =
            (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Thumbnails/png/" + _furnitureDataSO.m_prefab.name + ".png",
                typeof(Sprite));
        if (thumbnailPNG != null)
        {
            _furnitureDataSO.m_thumbnailPNG = thumbnailPNG;
        }
        var thumbnailJPG =
            (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Thumbnails/jpg/" + _furnitureDataSO.m_prefab.name + ".jpg",
                typeof(Sprite));
        if (thumbnailJPG != null)
        {
            _furnitureDataSO.m_thumbnailJPG = thumbnailJPG;
        }

        if (_furnitureDataSO.m_thumbnailJPG != null)
        {
            _previewImage.sourceRect = new Rect(0, 0,250, 250);
            _previewImage.sprite = _furnitureDataSO.m_thumbnailJPG;
        }
    }

}