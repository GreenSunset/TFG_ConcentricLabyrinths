using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleMazeNavigator : MonoBehaviour
{
    public SimpleMultilevelMaze maze;
    public UnityEvent OnExitReached;
    private SimpleMaze currentMaze;
    public int currentPointIndex { get; private set;} = -1;
    public int nextPointIndex { get; private set;} = -1;
    public float current2nextRatio { get; private set;} = 0;



    void Start()
    {
        OnExitReached.AddListener(() => {
            Message("You win!");
        });
        Initialize();
    }

    void Initialize() {
        if (maze == null || maze.currentMaze == null) {
            Debug.LogError("Maze not set");
            return;
        }
        currentMaze = maze.currentMaze;
        SnapToMaze(currentMaze.Coordinates2Index(currentMaze.entryIndex));
    }

    [ContextMenu("Snap to Maze")]
    void SnapToMaze() {
        float minDistance = float.MaxValue;
        Vector3 closestPoint = Vector3.zero;
        int closestPointIndex = -1;
        for (int i = 0; i < currentMaze.points.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, currentMaze.points[i] + maze.transform.position);
            if (distance < minDistance) {
                minDistance = distance;
                closestPoint = currentMaze.points[i];
                closestPointIndex = i;
            }
        }
        Debug.Log("Closest point: " + closestPointIndex);
        if (currentPointIndex != -1) return;
        Debug.Log("Snapped to maze");
        currentPointIndex = closestPointIndex;
        nextPointIndex = -1;
        current2nextRatio = 0;
        transform.parent = maze.transform;
        transform.localPosition = closestPoint;
    }
    
    void SnapToMaze(int index) {
        currentPointIndex = index;
        if (currentPointIndex != -1) return;
        Debug.Log("Snapped to maze");
        nextPointIndex = -1;
        current2nextRatio = 0;
        transform.parent = maze.transform;
        transform.localPosition = currentMaze.points[currentPointIndex];
    }

    protected int? NeighbourInAngle(float minInclusive, float maxExclusive) {
        Camera camera = Camera.main;
        List<int> neighbours = new List<int>(currentMaze.maze[currentPointIndex]);
        while (minInclusive < 0) minInclusive += 360;
        while (maxExclusive < 0) maxExclusive += 360;
        minInclusive %= 360;
        maxExclusive %= 360;
        bool invert = minInclusive > maxExclusive;
        for (int i = 0; i < neighbours.Count; i++)
        {
            Vector3 neighbourDirection = currentMaze.points[neighbours[i]] - currentMaze.points[currentPointIndex];
            float angle = Vector3.SignedAngle(-camera.transform.right, neighbourDirection, camera.transform.forward) + 180;
            if (invert) {
                if (angle >= minInclusive || angle < maxExclusive) {
                    return neighbours[i];
                }
            } else {
                if (angle >= minInclusive && angle < maxExclusive) {
                    return neighbours[i];
                }
            }
        }
        return null;
    }
    public int? NeighbourInDirection(Vector3 direction, ref float resultAngle) {
        List<int> neighbours = new List<int>(currentMaze.maze[currentPointIndex]);
        int? neighbour = null;
        float minAngle = 90;
        if (nextPointIndex != -1) {
            Vector3 neighbourDirection = currentMaze.points[nextPointIndex] - currentMaze.points[currentPointIndex];
            float angle = Vector3.Angle(neighbourDirection, direction);
            if (angle > 90) {
                minAngle = 180 - angle;
                neighbour = currentPointIndex;
            } else {
                minAngle = angle;
                neighbour = nextPointIndex;
            }
        }
        if (current2nextRatio < .25) {
            for (int i = 0; i < neighbours.Count; i++)
            {
                Vector3 neighbourDirection = currentMaze.points[neighbours[i]] - currentMaze.points[currentPointIndex];
                float angle = Vector3.Angle(neighbourDirection, direction);
                if (angle < minAngle) {
                    minAngle = angle;
                    neighbour = neighbours[i];
                }
            }
        }
        // Debug.Log("Neighbour in direction " + direction + " is " + neighbour + " with angle " + minAngle);
        resultAngle = minAngle;
        return neighbour;
    }

    public void Message(string message) {
        Debug.Log(message);
    }

    public void Ascend() {
        if (currentPointIndex != currentMaze.Coordinates2Index(currentMaze.entryIndex)) {
            // Debug.Log("Can't ascend from here");
            return;
        }
        Vector3Int ascentCoordinates = currentMaze.Index2Coordinates(currentPointIndex) + new Vector3Int(1, 1, 0);
        SimpleMaze nextMaze = maze.PeekMaze(maze.currentLevel + 1);
        if (nextMaze == null) {
            Debug.Log("Nothing above");
            return;
        }
        maze.AscendLevel();
        currentMaze = nextMaze;
        SnapToMaze(nextMaze.Coordinates2Index(ascentCoordinates));
    }

    public void Descend() {
        if (currentPointIndex != currentMaze.Coordinates2Index(currentMaze.exitIndex)) {
            // Debug.Log("Can't descend from here");
            return;
        }
        Vector3Int descentCoordinates = currentMaze.Index2Coordinates(currentPointIndex) - new Vector3Int(1, 1, 0);
        SimpleMaze nextMaze = maze.PeekMaze(maze.currentLevel - 1);
        if (nextMaze == null || !nextMaze.AreValidCoordinates(descentCoordinates)) {
            Debug.Log("Nothing below");
            return;
        }
        maze.DescendLevel();
        currentMaze = nextMaze;
        SnapToMaze(nextMaze.Coordinates2Index(descentCoordinates));
    }

    public void MoveTowards(int index, float progress) {
        if (currentPointIndex == index) {
            current2nextRatio -= progress;
            if (current2nextRatio <= 0) {
                current2nextRatio = 0;
                nextPointIndex = -1;
            }
        } else if (nextPointIndex == -1) {
            nextPointIndex = index;
            current2nextRatio = progress;
        } else if (nextPointIndex == index) {
            current2nextRatio += progress;
            if (current2nextRatio > .5) {
                current2nextRatio = 1 - current2nextRatio;
                nextPointIndex = currentPointIndex;
                currentPointIndex = index;
                if (current2nextRatio < 0) {
                    current2nextRatio = 0;
                    nextPointIndex = -1;
                }
            }
        } else {
            current2nextRatio -= progress;
            if (current2nextRatio < 0) {
                current2nextRatio = -current2nextRatio;
                nextPointIndex = index;
            }
        }
        if (nextPointIndex == -1) {
            transform.localPosition = currentMaze.points[currentPointIndex];
            transform.up = currentMaze.GetNormal(currentPointIndex);
        } else {
            transform.localPosition = Vector3.Lerp(currentMaze.points[currentPointIndex], currentMaze.points[nextPointIndex], current2nextRatio);
            transform.up = Vector3.Lerp(currentMaze.GetNormal(currentPointIndex), currentMaze.GetNormal(nextPointIndex), current2nextRatio);
        }
        if (currentPointIndex == currentMaze.Coordinates2Index(currentMaze.exitIndex) && current2nextRatio < .3) {
            OnExitReached.Invoke();
        }
        
    }
}
