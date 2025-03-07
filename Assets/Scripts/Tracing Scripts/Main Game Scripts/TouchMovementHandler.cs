using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchMovementHandler : MonoBehaviour
{
    public static TouchMovementHandler Instance;

    [HideInInspector] public GameObject PointerGO;
    public GameObject PointerPrefab;
    private Vector3 PointerPosition;
    private Plane newPlane;
    private float CalcRayDistance;
    public bool isAlignet = false;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        newPlane = new Plane(Camera.main.transform.forward * 0.1f, this.transform.position);
    }

    // Update is called once per frame
    private void Update()
    {
        PointerHandle();
    }

    void PointerHandle()
    {
        if((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
        {
            Ray newRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(newPlane.Raycast(newRay, out CalcRayDistance))
            {
                PointerPosition = newRay.GetPoint(CalcRayDistance);
                PointerGO = Instantiate(PointerPrefab, PointerPosition, Quaternion.identity);
            }
        }
        else if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) || Input.GetMouseButton(0))
        {
            Ray newRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(newPlane.Raycast(newRay, out CalcRayDistance))
            {
                if(PointerGO != null)
                {
                    PointerGO.transform.position = newRay.GetPoint(CalcRayDistance);
                }
            }
        }    
    }
}

