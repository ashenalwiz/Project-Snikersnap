using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path 
{
    
    public List<Vector2> points;

    public Path(Vector2 centre)
    {
        points = new List<Vector2>
        {
            centre + Vector2.left,
            centre + Vector2.left * 0.35f,
            centre + Vector2.right * 0.35f,
            centre + Vector2.right,
        };
    }

    public Vector2 this[int i] => points[i];

    public int NumPoints => points.Count;

    public int NumSegments => (points.Count - 4) / 3 + 1;

    public void AddSegment(Vector2 anchorPos)
    {
        points.Add(anchorPos);
    }

    public Vector2[] GetPointsInSegment(int i) =>
        new Vector2[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[i * 3 + 3] };

    public void MovePoint(int i, Vector2 pos) => points[i] = pos;    
}
