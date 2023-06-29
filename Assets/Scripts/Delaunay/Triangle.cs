using System.Collections.Generic;
using UnityEngine;

public class Triangle {
    private Vector2 CircumCentre;
    private Delaunay delaunay;
    private float CircumRadius;
    public List<int> vertices { get; } = new List<int>(3){-1, -1, -1};
    public Vector2 circumCentre { get { return CircumCentre; } }
    public float circumRadius { get { return CircumRadius; } }

    public Triangle(int a, int b, int c, Delaunay del) {
        delaunay = del;
        bool isCounterClockwise = delaunay.IsCounterClockwise(a, b, c);
        vertices[0] = a;
        vertices[1] = isCounterClockwise ? b : c;
        vertices[2] = isCounterClockwise ? c : b;

        CircumCentre = delaunay.FindCircumcenter(a, b, c);
        CircumRadius = delaunay.Distance(CircumCentre, a);
    }

    public override string ToString() {
        return "{" + vertices[0] + ", " + vertices[1] + ", " + vertices[2] + "}";
    }
}