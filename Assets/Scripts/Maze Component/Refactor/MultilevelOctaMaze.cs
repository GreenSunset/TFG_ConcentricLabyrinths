using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultilevelOctaMaze : MultilevelTetraMaze
{
    // 125.2643896827547
    // 109.4712206344907
    // 70.5287793655093
    // 54.7356103172454
    override public Quaternion[] faces { get; } = new Quaternion[] {
        new Quaternion(0, 0, -0.4597008f, 0.8880738f),
        new Quaternion(0, 0, 0.8880738f, 0.4597008f),
        new Quaternion(0.3250576f, -0.627963f, -0.3250576f, 0.627963f),
        new Quaternion(-0.627963f, -0.3250576f, 0.627963f, 0.3250576f),
        new Quaternion(-0.4597008f, 0.8880738f, 0, 0),
        new Quaternion(0.8880738f, 0.4597008f, 0, 0),
        new Quaternion(-0.3250576f, 0.627963f, -0.3250576f, 0.627963f),
        new Quaternion(0.627963f, 0.3250576f, 0.627963f, 0.3250576f),
    };

    override public List<List<int>> faceAdj { get; } = new List<List<int>> {
        new List<int> {2,5,-1,6},
        new List<int> {7,-1,4,3},
        new List<int> {4,7,-1,0},
        new List<int> {1,-1,6,5},
        new List<int> {6,1,-1,2},
        new List<int> {3,-1,0,7},
        new List<int> {0,3,-1,4},
        new List<int> {5,-1,2,1},
    };
    
    override protected float distanceFromCenter { get; } = 1 / Mathf.Sqrt(6f);
    override protected float distanceAdd { get; } = 3 / Mathf.Sqrt(2f);
}
