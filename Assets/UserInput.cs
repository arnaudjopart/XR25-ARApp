using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;


public class UserInput : MonoBehaviour
{
    [SerializeField] ARRaycastManager _arRaycastManager;
    [SerializeField] private FurnitureDataSO _furnitureData;
    private Camera _camera;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private GameObject _portraitPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        //Gestion input avec souris
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            SpawnObject(ray);
        }
        //
        
        //Gestion Input Mobile
        foreach (var touch in Touch.activeTouches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                var touchPosition = touch.screenPosition;
                Ray ray = _camera.ScreenPointToRay(touchPosition);

                SpawnObject(ray);
               
            }
        }
    }

    private void SpawnObject(Ray ray)
    {
        // Vérifier si on clique sur un GameObject (exemple, Gaming Chair)
        if (CheckIfClickOnObject(ray, out GameObject selectedObject) == true)
        {
            Destroy(selectedObject);
        }
        else
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (_arRaycastManager.Raycast(ray, hits)==false) return;
        
        
            ARRaycastHit firstHit = hits[0];
        
            Instantiate(_furnitureData.m_prefab,firstHit.pose.position, Quaternion.identity);     
        }
        
    }

    private bool CheckIfClickOnObject(Ray ray, out GameObject selectedObject)
    {
        if (Physics.Raycast(ray, out RaycastHit hit,float.PositiveInfinity, _layerMask))
        {
            selectedObject = hit.collider.gameObject;
            return true;
        }
        selectedObject = null;
        return false;
    }
}


/*if (firstHit.trackable is ARPlane plane)
            {
                if (Vector3.Dot(plane.normal, Vector3.up) > .5f)
                {
                         
                }
                else
                {
                    Instantiate(_portraitPrefab, firstHit.pose.position, Quaternion.LookRotation(plane.normal));
                }
                            
            }*/