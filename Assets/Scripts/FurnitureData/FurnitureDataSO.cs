using UnityEngine;

[CreateAssetMenu(fileName = "new furniture", menuName = "Catalogue/Furniture")]
public class FurnitureDataSO : ScriptableObject
{
    public Dimensions m_size;
    public float m_price;
    public string m_name;
    public string m_description;
    public FurnitureCategory m_category;
    public GameObject m_prefab;
    
}

[System.Serializable]
public class Dimensions
{
    public float m_width;
    public float m_height;
    public float m_depth;
}

public enum FurnitureCategory
{
    Other, // = 0
    Chair, // = 1
    Plant, // = 2
    Light // = 3
}
