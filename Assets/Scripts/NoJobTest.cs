using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// C# Job Systemを使わない版
/// </summary>
public class NoJobTest : MonoBehaviour {
    [SerializeField]
    private int _count = 0;

    [SerializeField]
    private Vector3 _minPos;

    [SerializeField]
    private Vector3 _maxPos;

    [SerializeField]
    private Vector3 _minVel;

    [SerializeField]
    private Vector3 _maxVel;

    /// <summary>
    /// 初期化
    /// </summary>
    private void Start() {
        CreateObjects();
    }

    /// <summary>
    /// 指定された個数のオブジェクトを生成
    /// </summary>
    /// <returns></returns>
    private GameObject[] CreateObjects() {
        var objs = new GameObject[_count];
        var objToCopy = CreateTemplate();

        for ( int i = 0 ; i < _count ; i++ ) {
            var obj = GameObject.Instantiate(objToCopy);
            //cube.transform.position = UnityEngine.Random.insideUnitSphere * _radius;
            obj.transform.position = new Vector3(
                UnityEngine.Random.Range(_minPos.x, _maxPos.x),
                UnityEngine.Random.Range(_minPos.y, _maxPos.y),
                UnityEngine.Random.Range(_minPos.z, _maxPos.z));

            var updater = obj.AddComponent<NoJobUpdate>();

            var vel = new Vector3(
                UnityEngine.Random.Range(_minVel.x, _maxVel.x),
                UnityEngine.Random.Range(_minVel.y, _maxVel.y),
                UnityEngine.Random.Range(_minVel.z, _maxVel.z));

            updater.Init(vel, _minPos, _maxPos);

            objs[i] = obj;
        }

        GameObject.Destroy(objToCopy);

        return objs;
    }

    /// <summary>
    /// オブジェクト生成
    /// </summary>
    /// <returns></returns>
    public GameObject CreateTemplate() {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        //turn off shadows entirely
        var renderer = obj.GetComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        // disable collision
        var collider = obj.GetComponent<Collider>();
        collider.enabled = false;

        return obj;
    }
}
