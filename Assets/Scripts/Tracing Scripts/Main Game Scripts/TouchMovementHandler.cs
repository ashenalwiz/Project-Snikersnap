using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TracingScripts
{
    public class TouchMovementHandler : MonoBehaviour
    {
        public static TouchMovementHandler Instance;

        [HideInInspector] public GameObject PointerGO;
        public GameObject PointerPrefab;
        private Vector3 PointerPosition;
        private Plane newPlane;
        private float CalcRayDistance;
        public bool isAligned = false;
        private float startingPoint_Pointer_CalcDistance;
        public float maxPointsDistance;
        public int currentNumPath, currentPathPointToHit = 0;
        public bool[] hasHitPathPoints;



        private void Awake()
        {
            Instance = this;
        }
        // Start is called before the first frame update
        void Start()
        {
            newPlane = new Plane(Camera.main.transform.forward * 0.1f, this.transform.position);
            currentPathPointToHit = 0;

            if (PathGenerateHandler.Instance == null || PathGenerateHandler.Instance.myListOfPaths.Count == 0)
            {
                Debug.LogError("PathGenerateHandler instance or paths list is null!");
                return;
            }

            var pathDrawer = PathGenerateHandler.Instance.myListOfPaths[0].GetComponent<PathDrawer>();
            if (pathDrawer == null)
            {
                Debug.LogError("PathDrawer component missing on path object!");
                return;
            }

            hasHitPathPoints = new bool[pathDrawer.path.points.Count];
        }

        private void Update()
        {
            if (!Game_Manager.Instance.isGameEnded) PointerHandle();
        }

        void PointerHandle()
        {
            if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
            {
                SoundManager.Instance.WriteFX.SetActive(true);
                Ray newRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (newPlane.Raycast(newRay, out CalcRayDistance))
                {
                    PointerPosition = newRay.GetPoint(CalcRayDistance);
                    PointerGO = Instantiate(PointerPrefab, PointerPosition, Quaternion.identity);
                    Vector2 startingPointPosition = PathGenerateHandler.Instance.myListOfPaths[currentNumPath].GetComponent<PathDrawer>().path.points[0];
                    startingPoint_Pointer_CalcDistance = Vector2.Distance(PointerGO.transform.position, startingPointPosition);

                    if (startingPoint_Pointer_CalcDistance > maxPointsDistance)
                    {
                        DestroyPointer();
                    }
                    else
                    {
                        if (!hasHitPathPoints[0])
                        {
                            hasHitPathPoints[0] = true;
                            currentPathPointToHit = 1;
                            HandGuideHandler.Instance.DefaultHandGuide();
                        }
                    }
                }
            }
            else if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) || Input.GetMouseButton(0))
            {
                Ray newRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (newPlane.Raycast(newRay, out CalcRayDistance))
                {
                    if (PointerGO != null)
                    {
                        PointerGO.transform.position = newRay.GetPoint(CalcRayDistance);

                        Vector2 currentPointPosition = PathGenerateHandler.Instance.myListOfPaths[currentNumPath].GetComponent<PathDrawer>().path.points[currentPathPointToHit];
                        float currentPoint_Pointer_Distance = Vector2.Distance(PointerGO.transform.position, currentPointPosition);

                        if (currentPoint_Pointer_Distance < maxPointsDistance)
                        {
                            if (!hasHitPathPoints[currentPathPointToHit])
                            {
                                hasHitPathPoints[currentPathPointToHit] = true;
                                currentPathPointToHit += 1;

                                if (hasHitPathPoints.All(x => x))
                                {
                                    if (currentNumPath == PathGenerateHandler.Instance.myListOfPaths.Count - 1)
                                    {
                                        GameEnd();
                                    }
                                    else
                                    {
                                        NextPath();
                                    }
                                }
                            }
                        }
                        else if (currentPoint_Pointer_Distance > 1f)
                        {
                            DestroyPointer();
                        }
                    }
                }
            }
        }

        public void DestroyPointer()
        {
            SoundManager.Instance.WriteFX.SetActive(false);
            currentPathPointToHit = 0;
            hasHitPathPoints = new bool[0];
            Destroy(PointerGO.gameObject);
            PlayHandGuideAgain();
        }

        void NextPath()
        {
            hasHitPathPoints = new bool[0];
            DestroyPointer();
            currentNumPath += 1;
            PlayHandGuideAgain();
        }
        void PlayHandGuideAgain()
        {
            if (currentNumPath < PathGenerateHandler.Instance.myListOfPaths.Count)
            {
                hasHitPathPoints = new bool[PathGenerateHandler.Instance.myListOfPaths[currentNumPath].GetComponent<PathDrawer>().path.points.Count];
                HandGuideHandler.Instance.isShowGuide = true;
            }
        }

        void GameEnd()
        {
            DestroyPointer();
            currentNumPath += 1;
            Game_Manager.Instance.isGameEnded = true;
        }
    }
}