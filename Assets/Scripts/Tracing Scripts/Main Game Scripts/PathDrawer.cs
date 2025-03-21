using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace TracingScripts
{
    public class PathDrawer : MonoBehaviour
    {
        public Path path;
        private LineRenderer myLineRenderer;
        public int MyCurrentNumber;
        [SerializeField] private float[] pointsDistance;
        public bool isCicrcle;

        private void Start()
        {
            GetComponent<LineRenderer>().enabled = false;
        }

        public void CreatePath()
        {
            path = new Path(transform.position);

            myLineRenderer = this.AddComponent<LineRenderer>();
            myLineRenderer.widthMultiplier = 0.2f;
        }

        public void DrawPath(List<Vector2> points)
        {
            CalculateDistancePoints();
            if (this.GetComponent<EdgeCollider2D>() == null)
            {
                this.gameObject.AddComponent<EdgeCollider2D>();
            }
            this.GetComponent<EdgeCollider2D>().offset = new Vector2(0f, 0f);
            this.GetComponent<EdgeCollider2D>().points = points.ToArray();

            this.GetComponent<LineRenderer>().positionCount = points.Count;

            for (int i = 0; i < points.Count; i++)
            {
                this.GetComponent<LineRenderer>().SetPosition(i, points[i]);
                this.GetComponent<EdgeCollider2D>().points[i] = new Vector2(points[i].x - 90f, points[i].y - 90f);
            }
        }

        private void CalculateDistancePoints()
        {
            pointsDistance = new float[path.points.Count - 1];

            for (int i = 0; i < pointsDistance.Length; i++)
            {
                Vector2 point_A = path.points[i];
                Vector2 point_B = path.points[i + 1];

                pointsDistance[i] = Vector2.Distance(point_A, point_B);
            }
        }
    }
}