using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PrototypeNavigator : MonoBehaviour
{
    public MultilevelMaze maze;
    public UnityEvent OnExitReached;
    public int currentPointIndex { get; private set;} = -1;
    public int nextPointIndex { get; private set;} = -1;
    public float current2nextRatio { get; private set;} = 0;

    public int target;
    public float speed = 1;

    void Start()
    {
        target = -1;
        OnExitReached.AddListener(() => {
            Message("You win!");
        });
        SnapToMaze(maze.entryIndex);
    }

    [ContextMenu("Snap to Maze")]
    void SnapToMaze() {
        float minDistance = float.MaxValue;
        Vector3 closestPoint = Vector3.zero;
        int closestPointIndex = -1;
        for (int i = 0; i < maze.points.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, maze.points[i] + maze.transform.position);
            if (distance < minDistance) {
                minDistance = distance;
                closestPoint = maze.points[i];
                closestPointIndex = i;
            }
        }
        Debug.Log("Closest point: " + closestPointIndex);
        // if (currentPointIndex != -1) return;
        Debug.Log("Snapped to maze");
        currentPointIndex = closestPointIndex;
        nextPointIndex = -1;
        current2nextRatio = 0;
        transform.position = maze.transform.TransformPoint(closestPoint);
        // transform.localPosition = closestPoint;
    }
    
    void SnapToMaze(int index) {
        currentPointIndex = index;
        // if (currentPointIndex != -1) return;
        Debug.Log("Snapped to maze");
        nextPointIndex = -1;
        current2nextRatio = 0;
        transform.position = maze.transform.TransformPoint(maze.points[currentPointIndex]);
        // transform.parent = maze.transform;
        // transform.localPosition = maze.points[currentPointIndex];
    }


    public int? NeighbourInDirection(Vector3 direction, ref float resultAngle) {
        List<int> candidates = new List<int>();
        if (current2nextRatio < .15f) {
            candidates.AddRange(maze.maze[currentPointIndex]);
            if (current2nextRatio > 0) {
                candidates.Add(currentPointIndex);
            }
        } else {
            candidates.Add(currentPointIndex);
            candidates.Add(nextPointIndex);
        }
        int? neighbour = null;
        float minAngle = 90;
        for (int i = 0; i < candidates.Count; i++) {
            Vector3 candidateDirection = maze.points[candidates[i]] - transform.position;
            float angle = Vector3.Angle(candidateDirection, direction);
            if (angle < minAngle) {
                minAngle = angle;
                neighbour = candidates[i];
            }
        }
        resultAngle = minAngle;
        return neighbour;
        // if (nextPointIndex != -1) {
        //     Vector3 neighbourDirection = maze.points[nextPointIndex] - maze.points[currentPointIndex];
        //     float angle = Vector3.Angle(neighbourDirection, direction);
        //     if (angle > 90) {
        //         minAngle = 180 - angle;
        //         neighbour = currentPointIndex;
        //     } else {
        //         minAngle = angle;
        //         neighbour = nextPointIndex;
        //     }
        // }
        // if (current2nextRatio < .25) {
        //     for (int i = 0; i < candidates.Count; i++)
        //     {
        //         Vector3 neighbourDirection = maze.points[candidates[i]] - maze.points[currentPointIndex];
        //         float angle = Vector3.Angle(neighbourDirection, direction);
        //         if (angle < minAngle) {
        //             minAngle = angle;
        //             neighbour = candidates[i];
        //         }
        //     }
        // }
        // // Debug.Log("Neighbour in direction " + direction + " is " + neighbour + " with angle " + minAngle);
        // resultAngle = minAngle;
        // return neighbour;
    }

    public void Message(string message) {
        Debug.Log(message);
    }

    private float Advance(float progress) {
        // Debug.Log("Before: " + currentPointIndex + " " + nextPointIndex + " " + current2nextRatio + " " + progress);
        current2nextRatio += progress;
        if (current2nextRatio > .5f) {
            current2nextRatio = 1 - current2nextRatio;
            int swap = currentPointIndex;
            currentPointIndex = nextPointIndex;
            nextPointIndex = swap;
        }
        if (current2nextRatio < 0) {
            progress = -current2nextRatio;
            current2nextRatio = 0;
            nextPointIndex = -1;
        } else {
            progress = 0;
        }
        // Debug.Log("After: " + currentPointIndex + " " + nextPointIndex + " " + current2nextRatio + " " + progress);
        return progress;
    }

    public float MoveTowards(int index, float progress) {
        int it = 0;
        while (Mathf.Abs(progress) > 0) {
            if (it > 100) {
                Debug.LogError("Infinite loop detected: " + progress + " " + current2nextRatio + " " + currentPointIndex + " " + nextPointIndex);
                break;
            }
            it++;
            if (current2nextRatio > 0) {
                if (index == nextPointIndex) {
                    progress = Advance(progress);
                } else {
                    progress = Advance(-progress);
                }
            } else {
                if (index != currentPointIndex) {
                    nextPointIndex = index;
                    progress = Advance(progress);
                }
                else {
                    break;
                }
            }
            // progress = 0;
        }
        return progress;
        // if (currentPointIndex == index) {
        //     current2nextRatio -= progress;
        //     if (current2nextRatio <= 0) {
        //         current2nextRatio = 0;
        //         nextPointIndex = -1;
        //     }
        // } else if (nextPointIndex == -1) {
        //     nextPointIndex = index;
        //     current2nextRatio = progress;
        // } else if (nextPointIndex == index) {
        //     current2nextRatio += progress;
        //     if (current2nextRatio > .5) {
        //         current2nextRatio = 1 - current2nextRatio;
        //         nextPointIndex = currentPointIndex;
        //         currentPointIndex = index;
        //         if (current2nextRatio < 0) {
        //             current2nextRatio = 0;
        //             nextPointIndex = -1;
        //         }
        //     }
        // } else {
        //     current2nextRatio -= progress;
        //     if (current2nextRatio < 0) {
        //         current2nextRatio = -current2nextRatio;
        //         nextPointIndex = index;
        //     }
        // }
        // if (nextPointIndex == -1) {
        //     transform.localPosition = maze.points[currentPointIndex];
        // } else {
        //     transform.localPosition = Vector3.Lerp(maze.points[currentPointIndex], maze.points[nextPointIndex], current2nextRatio);
        // }
    }

    void Update() {
        if (currentPointIndex == -1) return;
        if (nextPointIndex == -1) {
            transform.position = maze.transform.TransformPoint(maze.points[currentPointIndex]);
        } else {
            transform.position = Vector3.Lerp(maze.transform.TransformPoint(maze.points[currentPointIndex]), maze.transform.TransformPoint(maze.points[nextPointIndex]), current2nextRatio);
        }
        maze.GetComponent<MazeVisualizer>().currentLevel = maze.GetDepth(currentPointIndex);
        // Debug Movement
        // if (target == -1 || target == currentPointIndex) {
        //     target = new List<int>(maze.maze[currentPointIndex])[Random.Range(0, maze.maze[currentPointIndex].Count)];
        // }
        // MoveTowards(target, Time.deltaTime * speed);
    }
}
