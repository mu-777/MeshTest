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
    [SerializeField]
    private float _interval = 0.1f;

    private Mesh _mesh = new Mesh();
    private MeshFilter _filter = new MeshFilter();
    private MeshRenderer _renderer = new MeshRenderer();

    void Start() {
        _mesh.vertices = createSphereVertices(_density).ToArray();
        _mesh.triangles = createTriangles(_density).ToArray();
        _mesh.normals = createSphereVertices(_density).ToArray();

        _filter = GetComponent<MeshFilter>();
        _filter.sharedMesh = _mesh;

        _renderer = GetComponent<MeshRenderer>();
        _renderer.material = _mat;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            var updated = _filter.sharedMesh.vertices;
            int size = _density * 2;
            int h = UnityEngine.Random.Range(0, _density - 1);
            int w = UnityEngine.Random.Range(0, _density - 1);
            bool isInv = UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f;
            float radiusExtendRate = 1.0f + UnityEngine.Random.Range(0.5f, 1.0f);
            int iLeftUpper = (2 * h + 1) * size + (2 * w + 1) + (isInv ? size* size : 0);
            int[] idx = new int[] { iLeftUpper, iLeftUpper + 1, iLeftUpper + size, iLeftUpper + size + 1 };
            int[] hs = new int[] { h, h, h + 1, h + 1 };
            int[] ws = new int[] { w, w + 1, w, w + 1 };
            for (int i = 0; i < 4; i++) {
                float phi = 90f - (180f / (_density - 1)) * hs[i];
                float theta = !isInv ?
                              180f / (_density - 1) * (_density - 1 - ws[i]) :
                              180f + 180f / (_density - 1) * ws[i];
                updated[idx[i]] = fromSphereCoord(_radius * radiusExtendRate, phi, theta);
            }
            _filter.sharedMesh.vertices = updated;
        }
    }

    private Vector3 fromSphereCoord(float radius, float pitch_deg, float yaw_deg) {
        return new Vector3(radius * Mathf.Cos(pitch_deg * Mathf.Deg2Rad)*Mathf.Cos(yaw_deg * Mathf.Deg2Rad),
                           radius * Mathf.Sin(pitch_deg * Mathf.Deg2Rad),
                           radius * Mathf.Cos(pitch_deg * Mathf.Deg2Rad) * Mathf.Sin(yaw_deg * Mathf.Deg2Rad));
    }

    private List<int> divideSq2Tri(int[] square, bool isInv = false) {
        if (!isInv) {
            return new List<int>() {
                square[0], square[2], square[3], square[0], square[3], square[1]
            };
        } else {
            return new List<int>() {
                square[0], square[3], square[2], square[0], square[1], square[3]
            };
        }
    }

    private List<int> createTriangles(int density) {
        List<int> triangels = new List<int>();
        int size = 2 * density;
        foreach (var isInv in new bool[] { false, true }) {
            for (int h = 0; h < size - 1; h++) {
                for (int w = 0; w < size - 1; w++) {
                    int i = h * size + w + (isInv ? size* size : 0);
                    triangels.AddRange(divideSq2Tri(new int[] { i, i + 1, i + size, i + size + 1 }, isInv));
                }
            }
        }
        return triangels;
    }
    private List<Vector3> createSphereVertices(int density) {
        List<Vector3> vertices = new List<Vector3>();
        foreach (var isInv in new bool[] { false, true }) {
            for (int h = 0; h < density; h++) {
                for (int d = 0; d < 2; d++) {
                    for (int w = 0; w < density; w++) {
                        for (int d2 = 0; d2 < 2; d2++) {
                            float phi = 90f - (180f / (density - 1)) * h;
                            float theta = !isInv ?
                                          180f / (density - 1) * (density - 1 - w) :
                                          180f + 180f / (density - 1) * w;
                            vertices.Add(fromSphereCoord(_radius, phi, theta));
                        }
                    }
                }
            }
        }
        return vertices;
    }

    private List<Vector3> createPlaneVertices(int density) {
        List<Vector3> vertices = new List<Vector3>();
        foreach (var isInv in new bool[] { false, true }) {
            for (int h = 0; h < density; h++) {
                for (int w = 0; w < density; w++) {
                    vertices.Add(new Vector3((-(density - 1) / 2 + w) * _interval,
                                             ((density - 1) / 2 - h) * _interval,
                                             0));
                }
            }
        }
        return vertices;
    }
}
