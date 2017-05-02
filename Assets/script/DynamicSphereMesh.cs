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
    private float _period_sec = 4;
    [SerializeField]
    private float _radius_rate = 0.3f;

    private int size;
    private Mesh _mesh = new Mesh();
    private MeshFilter _filter = new MeshFilter();
    private MeshRenderer _renderer = new MeshRenderer();
    private float startTime = -1f;
    private bool isEnableVertexAnimetor = false;
    private List<float> radiusExtendRates = new List<float>();
    private List<float> periods = new List<float>();

    void Awake() {
        size = 2 * _density;

        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        float phi, theta;
        int idx;
        foreach (var isInv in new bool[] { false, true }) {
            for (int h = 0; h < size; h++) {
                for (int w = 0; w < size; w++) {
                    phi = 90f - (180f / (_density - 1)) * (int)(h / 2);
                    theta = !isInv ?
                            180f / (_density - 1) * (_density - 1 - (int)(w / 2)) :
                            180f + 180f / (_density - 1) * (int)(w / 2);
                    vertices.Add(SphereCoord.ToVector3(_radius, phi, theta));

                    if (h >= size - 1 || w >= size - 1) {
                        continue;
                    }
                    idx = h * size + w + (isInv ? size* size : 0);
                    triangles.AddRange(divideSq2Tri(new int[] { idx, idx + 1, idx + size, idx + size + 1 },
                                                    isInv));
                }
            }
        }

        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.normals = vertices.ToArray();

        _filter = GetComponent<MeshFilter>();
        _filter.sharedMesh = _mesh;

        _renderer = GetComponent<MeshRenderer>();
        _renderer.material = _mat;
    }

    void Start() {

    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            isEnableVertexAnimetor = !isEnableVertexAnimetor;
            if (isActiveAndEnabled) {
                foreach (var isInv in new bool[] { false, true }) {
                    for (int h = 1; h < size - 2; h += 2) {
                        for (int w = 1; w < size - 2; w += 2) {
                            periods.Add(_period_sec * UnityEngine.Random.Range(0.5f, 1.5f));
                            radiusExtendRates.Add(UnityEngine.Random.Range(-1f, 1f) * _radius_rate);
                        }
                    }
                }
            } else {
                periods.Clear();
                radiusExtendRates.Clear();
            }
        }
    }

    void FixedUpdate() {
        if (!isEnableVertexAnimetor) {
            return;
        } else if (startTime < 0f) {
            startTime = Time.time;
        }

        var updated = _filter.sharedMesh.vertices;
        var passedTime = Time.time - startTime;
        var sphereCoord = new SphereCoord();
        var j = 0;
        foreach (var isInv in new bool[] { false, true }) {
            for (int h = 1; h < size - 2; h += 2) {
                for (int w = 1; w < size - 2; w += 2) {
                    var r = _radius * (1.0f + radiusExtendRates[j] * Mathf.Cos(2f * Mathf.PI * passedTime / periods[j]));
                    j++;
                    for (int i = 0; i < 4; i++) {
                        sphereCoord = sphereCoordFromHW(r, h + (int)(i / 2), w + (int)(i % 2), isInv);
                        updated[(h + (int)(i / 2))*size + (w + (int)(i % 2)) + (isInv ? size* size : 0)] = sphereCoord.ToVector3();
                    }
                }
            }
        }
        _filter.sharedMesh.vertices = updated;
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

    private SphereCoord sphereCoordFromHW(float radius, int h, int w, bool isInv) {
        var phi = 90f - (180f / (_density - 1)) * (int)(h / 2);
        var theta = !isInv ?
                    180f / (_density - 1) * (_density - 1 - (int)(w / 2)) :
                    180f + 180f / (_density - 1) * (int)(w / 2);
        return new SphereCoord(radius, phi, theta);
    }

    private class SphereCoord {
        private float _radius, _pitch, _yaw;
        public SphereCoord() { }
        public SphereCoord(float radius, float pitch_deg, float yaw_deg) {
            _radius = radius;
            _pitch = pitch_deg * Mathf.Deg2Rad;
            _yaw = yaw_deg * Mathf.Deg2Rad;
        }

        public float radius {
            get { return _radius; }
        }
        public float pitch {
            get { return _pitch; }
        }
        public float yaw {
            get { return _yaw; }
        }
        public float pitch_deg {
            get { return _pitch * Mathf.Rad2Deg; }
        }
        public float yaw_deg {
            get { return _yaw * Mathf.Rad2Deg; }
        }

        public Vector3 ToVector3() {
            return new Vector3(_radius * Mathf.Cos(pitch) * Mathf.Cos(yaw),
                               _radius * Mathf.Sin(pitch),
                               _radius * Mathf.Cos(pitch) * Mathf.Sin(yaw));
        }

        static public Vector3 ToVector3(float radius, float pitch_deg, float yaw_deg) {
            return new Vector3(radius * Mathf.Cos(pitch_deg * Mathf.Deg2Rad) * Mathf.Cos(yaw_deg * Mathf.Deg2Rad),
                               radius * Mathf.Sin(pitch_deg * Mathf.Deg2Rad),
                               radius * Mathf.Cos(pitch_deg * Mathf.Deg2Rad) * Mathf.Sin(yaw_deg * Mathf.Deg2Rad));
        }
    }

}
