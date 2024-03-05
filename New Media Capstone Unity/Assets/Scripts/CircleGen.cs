using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CircleGen : MonoBehaviour {
    float minRadius = 0.2f;
    float maxRadius = 2.5f;
    int circleCount = 100;
    public CircleTile circlePrefab;
    public List<CircleTile> sCircles = new List<CircleTile>();

    [Range(0.001f, 0.1f)]
    public float dampening = 0.0174f;
    public float radiusAdjust = 0.31f;

    public void Start() {
    List<CircleTile> circles = new List<CircleTile>();
        for (int i = 0; i < circleCount; i++) {
            CircleTile c = Instantiate(circlePrefab, Random.insideUnitCircle, Quaternion.identity);
            c.Radius = Random.Range(minRadius, maxRadius);
            c.transform.localScale = 2 * c.Radius * Vector3.one;
            c.name = $"Circle {i}";
            c.transform.SetParent(transform, true);
            circles.Add(c);
        }

        var sortedCircles = from c in circles
                            orderby c.Radius descending
                            select c;
        sCircles = sortedCircles.ToList();
    }

    public void Update() {
        if (Time.realtimeSinceStartup > 15f) return;
        Vector2 v;

        foreach (CircleTile c1 in sCircles) {
            foreach (CircleTile c2 in sCircles) {
                if (c1 == c2) continue;
                //Debug.Log($"Comparing {c1.name} and {c2.name}");
                float dx = c2.transform.position.x - c1.transform.position.x;
                float dy = c2.transform.position.y - c1.transform.position.y;
                float r = c1.Radius * 0.88f + c2.Radius * 0.88f;
                float d = (dx * dx) + (dy * dy);
                if (d < (r * r) - 0.001) {
                    v = new Vector3(dx, dy);
                    v.Normalize();
                    v *= (float)((r - (float)Mathf.Sqrt(d)) * radiusAdjust);

                    //Debug.Log($"Adjusting positions by {v}");
                    c2.transform.Translate(v);
                    c1.transform.Translate(-v);
                }
            }
        }

        foreach (CircleTile c in sCircles) {
            v = c.transform.position - transform.position;
            v *= dampening;
            c.transform.Translate(-v);
        }
    }
}