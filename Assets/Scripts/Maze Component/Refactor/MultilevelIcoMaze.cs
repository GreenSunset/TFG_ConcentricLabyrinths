using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultilevelIcoMaze : MultilevelTetraMaze
{
    // 138.1896851042214
    // 41.8103148957786
    // 63.434948822922
    // 37.3773681406497
    // 142.6226318593503
    // 79.1876830364283
    // 100.8123169635717
    override public Quaternion[] faces { get; } = new Quaternion[] {
        new Quaternion(0, 0, -0.3204259f, 0.9472736f),
        new Quaternion(0, 0, 0.9472736f, 0.3204259f),
        new Quaternion(0.1883416f, -0.5567934f, -0.25923f, 0.7663604f),
        new Quaternion(-0.5567934f, -0.1883416f, 0.7663604f, 0.25923f),
        new Quaternion(0.3047431f, -0.9009107f, -0.0990171f, 0.2927236f),
        new Quaternion(-0.9009107f, -0.3047431f, 0.2927236f, 0.0990171f),
        new Quaternion(-0.3047431f, 0.9009107f, -0.0990171f, 0.2927236f),
        new Quaternion(0.9009107f, 0.3047431f, 0.2927236f, 0.0990171f),
        new Quaternion(-0.1883416f, 0.5567934f, -0.25923f, 0.7663604f),
        new Quaternion(0.5567934f, 0.1883416f, 0.7663604f, 0.25923f),
        new Quaternion(0.6373412f, 0.7705818f, 0, 0) * Quaternion.Euler(0, -120,0),
        new Quaternion(-0.7705818f, 0.6373412f, 0, 0) * Quaternion.Euler(0, 120,0),
        new Quaternion(0.5156198f, 0.6234137f, 0.3746197f, 0.4529366f) * Quaternion.Euler(0, -120,0),
        new Quaternion(-0.6234137f, 0.5156198f, -0.4529366f, 0.3746197f) * Quaternion.Euler(0, 120,0),
        new Quaternion(0.1969493f, 0.2381229f, 0.6061475f, 0.7328668f) * Quaternion.Euler(0, -120,0),
        new Quaternion(-0.2381229f, 0.1969493f, -0.7328668f, 0.6061475f) * Quaternion.Euler(0, 120,0),
        new Quaternion(-0.1969493f, -0.2381229f, 0.6061475f, 0.7328668f) * Quaternion.Euler(0, -120,0),
        new Quaternion(0.2381229f, -0.1969493f, -0.7328668f, 0.6061475f) * Quaternion.Euler(0, 120,0),
        new Quaternion(-0.5156198f, -0.6234137f, 0.3746197f, 0.4529366f) * Quaternion.Euler(0, -120,0),
        new Quaternion(0.6234137f, -0.5156198f, -0.4529366f, 0.3746197f) * Quaternion.Euler(0, 120,0),
    };
    override public List<List<int>> faceAdj { get; } = new List<List<int>> {
        new List<int> {2,10,-1,8},
        new List<int> {9,-1,11,3},
        new List<int> {4,12,-1,0},
        new List<int> {1,-1,13,5},
        new List<int> {6,14,-1,2},
        new List<int> {3,-1,15,7},
        new List<int> {8,16,-1,4},
        new List<int> {5,-1,17,9},
        new List<int> {0,18,-1,6},
        new List<int> {7,-1,19,1},
        new List<int> {17,15,-1,0},
        new List<int> {1,-1,14,16},
        new List<int> {19,17,-1,2},
        new List<int> {3,-1,16,18},
        new List<int> {11,19,-1,4},
        new List<int> {5,-1,18,10},
        new List<int> {13,11,-1,6},
        new List<int> {7,-1,10,12},
        new List<int> {15,13,-1,8},
        new List<int> {9,-1,12,14},
    };
    override protected float distanceFromCenter { get; } = Mathf.Sqrt(3) * (3 + Mathf.Sqrt(5)) / 12;
    override protected float distanceAdd { get; } = 3 / (2 * Mathf.Sin(54 * Mathf.Deg2Rad));
}
