using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class DynamicSphereMesh : MonoBehaviour {
    [SerializeField]
    private Material _mat;

    [SerializeField]
    private float _radius;

    [SerializeField]
    private int _density = 5; // odd num only

    private List<Vector3> test;

    void Start() {
        var tri = createSphereTriangles(_density);
        //var mesh = new Mesh();
        //mesh.vertices = createSphereVertices(_density).ToArray();
        //mesh.triangles = createSphereTriangles(_density).ToArray();

        //var filter = GetComponent<MeshFilter>();
        //filter.sharedMesh = mesh;

        //var renderer = GetComponent<MeshRenderer>();
        //renderer.material = _mat;
    }

    void Update() {

    }

    private Vector3 fromSphereCoord(float radius, float pitch_deg, float yaw_deg) {
        return new Vector3(radius * Mathf.Cos(pitch_deg * Mathf.Deg2Rad)*Mathf.Cos(yaw_deg * Mathf.Deg2Rad),
                           radius * Mathf.Sin(pitch_deg * Mathf.Deg2Rad),
                           -radius * Mathf.Cos(pitch_deg * Mathf.Deg2Rad) * Mathf.Sin(yaw_deg * Mathf.Deg2Rad));
    }


    private List<int> createSphereTriangles(int density) {
        Func<int[], List<int>> divide = (int[] square) => {
            return new List<int>() {
                square[0], square[2], square[3], square[0], square[3], square[1]
            };
        };
        List<int> triangels = new List<int>();
        for (int layer = 1; layer < density; layer++) {
            int sq = Mathf.Min(2 * layer + 1, 2 * (density - layer - 1) + 1);
            int sq_prev = Mathf.Min(2 * (layer - 1) + 1, 2 * (density - (layer - 1) - 1) + 1);
            for (int i = sq_prev * sq_prev; i < sq * sq; i++) {
                if ((i - (sq - 1) / 2 - sq_prev * sq_prev) % (sq - 1) == 0) {
                    continue;
                }
                print(i + ", " + (i + 1) + ", " + (i - sq_prev * sq_prev + 1) + ", " + (i - sq_prev * sq_prev + 2));
                triangels.AddRange(divide(new int[] { i, i + 1, i - sq_prev * sq_prev - 1, i - sq_prev * sq_prev }));
            }
        }
        return triangels;
    }

    private List<Vector3> createSphereVertices(int density) {
        List<Vector3> vertices = new List<Vector3>();
        for (int layer = 0; layer < density; layer++) {
            float phi = 90 - layer * (180 / (density - 1));
            int sq = Mathf.Min(2 * layer + 1, 2 * (density - layer - 1) + 1);
            float thetaStep = 360 / (sq * sq - (sq - 1) * (sq - 1));
            for (float theta = 0; theta < 360; theta += thetaStep) {
                vertices.Add(fromSphereCoord(_radius, phi, theta));
            }
        }
        return vertices;
    }

    //void OnValidate() {
    //    test = createSphereVertices(_density);
    //}

    //void OnDrawGizmos() {
    //    Gizmos.color = Color.blue;
    //    foreach (var v in test) {
    //        Gizmos.DrawSphere(v, 0.1f);
    //    }
    //}
}
