using UnityEngine;

public class TestCameraFrustrum : MonoBehaviour
{
    private Camera _camera;
    private MeshRenderer _meshRenderer;
    private Bounds _bounds;
    [SerializeField] private float _distance=2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _bounds = _meshRenderer.bounds;
        _camera = Camera.main;
        
    }

    // Update is called once per frame
    void Update()
    {
        _bounds = _meshRenderer.bounds;
        var position = _bounds.center+_camera.transform.forward*-_distance;
        _camera.transform.position = position;
        var planes = GeometryUtility.CalculateFrustumPlanes(_camera);
        if (GeometryUtility.TestPlanesAABB(planes, _bounds))
        {
            Debug.Log("YES");
        }
        else
        {
            Debug.Log("NO");
        }
        _bounds.center = position;
    }
}
