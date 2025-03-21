using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

namespace TracingScripts
{
    public class HandGuideHandler : MonoBehaviour
    {
        public static HandGuideHandler Instance;
        public GameObject handGuideGO;
        public Vector3 currentPos;
        public Vector2 nextPos;
        [SerializeField] private int currentCount, countRrepeatGuideHand;
        [SerializeField] private float speed;
        public bool isShowGuide;
        PathDrawer _pathDrawer;


        private void Awake()
        {
            Instance = this;
        }
        // Start is called before the first frame update
        void Start()
        {
            handGuideGO.SetActive(false);
            isShowGuide = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!Game_Manager.Instance.isGameEnded)
            {
                if (isShowGuide) ShowHandGuide();
            }
        }

        public void ShowHandGuide()
        {
            handGuideGO.SetActive(true);
            _pathDrawer = PathGenerateHandler.Instance.myListOfPaths[TouchMovementHandler.Instance.currentNumPath].GetComponent<PathDrawer>();

            if (currentPos == handGuideGO.transform.position)
            {
                if (currentCount < _pathDrawer.path.points.Count)
                {
                    currentCount += 1;
                    SetSpeed();

                    nextPos = _pathDrawer.path.points[currentCount - 1];
                    currentPos = (Vector3)nextPos;
                    handGuideGO.transform.position = Vector3.MoveTowards(handGuideGO.transform.position, currentPos, speed * Time.deltaTime);
                }
                else
                {
                    ResetGuide();
                }
            }
            else
            {
                if (currentCount > 1)
                {
                    handGuideGO.transform.position = Vector3.MoveTowards(handGuideGO.transform.position, currentPos, speed * Time.deltaTime);
                }
                else
                {
                    nextPos = _pathDrawer.path.points[0];
                    currentPos = (Vector3)nextPos;
                    handGuideGO.transform.position = currentPos;
                }
            }
        }

        private void ResetGuide()
        {
            if (countRrepeatGuideHand < 2)
            {
                isShowGuide = true;
                countRrepeatGuideHand += 1;
                currentCount = 0;
            }
            else
            {
                DefaultHandGuide();
            }
        }

        public void DefaultHandGuide()
        {
            countRrepeatGuideHand = 0;
            isShowGuide = false;
            currentCount = 0;
            handGuideGO.SetActive(false);
        }

        private void SetSpeed()
        {
            if (_pathDrawer.isCicrcle)
            {
                speed = Vector2.Distance(_pathDrawer.path.points[0], _pathDrawer.path.points[_pathDrawer.path.points.Count - 1]) + 5f;
            }
            else
            {
                speed = Vector2.Distance(_pathDrawer.path.points[0], _pathDrawer.path.points[_pathDrawer.path.points.Count - 1]) + 0.54f;
            }
        }
    }
}