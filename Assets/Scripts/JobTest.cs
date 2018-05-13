using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using System;

/// <summary>
/// C# Job Systemテスト用
/// 参考 : https://github.com/stella3d/job-system-cookbook
/// </summary>
public class JobTest : MonoBehaviour {
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

    private GameObject[] _objectArray;
    private Transform[] _transformArray;

    private NativeArray<Vector3> _velocityArray;
    TransformAccessArray _transformsAccessArray;

    private VelocityUpdateJob _velocitUpdateJob;
    private PositionUpdateJob _positionUpdateJob;

    private JobHandle _velocityUpdateJobHandle;
    private JobHandle _positionUpdateJobHandle;

    /// <summary>
    /// 速度更新ジョブ
    /// </summary>
    private struct VelocityUpdateJob : IJobParallelForTransform {
        public NativeArray<Vector3> velocity;
        public float deltaTime;
        public Vector3 minPos;
        public Vector3 maxPos;

        public void Execute(int index, TransformAccess transform) {
            var vel = velocity[index];
            var pos = transform.position;

            if ( vel.x < 0 && pos.x < minPos.x )
                vel.x = -vel.x;
            if ( vel.y < 0 && pos.y < minPos.y )
                vel.y = -vel.y;
            if ( vel.z < 0 && pos.z < minPos.z )
                vel.z = -vel.z;
            if ( vel.x > 0 && pos.x > maxPos.x )
                vel.x = -vel.x;
            if ( vel.y > 0 && pos.y > maxPos.y )
                vel.y = -vel.y;
            if ( vel.z > 0 && pos.z > maxPos.z )
                vel.z = -vel.z;

            velocity[index] = vel;
        }
    }

    /// <summary>
    /// 位置更新ジョブ
    /// </summary>
    private struct PositionUpdateJob : IJobParallelForTransform {
        [ReadOnly]
        public NativeArray<Vector3> velocity;
        public float deltaTime;

        public void Execute(int index, TransformAccess transform) {
            transform.position += velocity[index] * deltaTime;
        }
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private void Start() {
        _objectArray = CreateObjects();
        _transformArray = new Transform[_count];
        _velocityArray = new NativeArray<Vector3>(_count, Allocator.Persistent);

        for ( int i = 0 ; i < _count ; ++i ) {
            var obj = _objectArray[i];
            _transformArray[i] = obj.transform;

            _velocityArray[i] = new Vector3(
                UnityEngine.Random.Range(_minVel.x, _maxVel.x),
                UnityEngine.Random.Range(_minVel.y, _maxVel.y),
                UnityEngine.Random.Range(_minVel.z, _maxVel.z));
        }

        _transformsAccessArray = new TransformAccessArray(_transformArray);
    }

    /// <summary>
    /// フレーム更新
    /// </summary>
    private void Update() {
        _velocitUpdateJob = new VelocityUpdateJob {
            velocity = _velocityArray,
            deltaTime = Time.deltaTime,
            minPos = _minPos,
            maxPos = _maxPos,
        };

        _positionUpdateJob = new PositionUpdateJob {
            velocity = _velocityArray,
            deltaTime = Time.deltaTime,
        };

        _velocityUpdateJobHandle = _velocitUpdateJob.Schedule(_transformsAccessArray);
        _positionUpdateJobHandle = _positionUpdateJob.Schedule(_transformsAccessArray, _velocityUpdateJobHandle);
    }

    /// <summary>
    /// ジョブ終了
    /// </summary>
    private void LateUpdate() {
        _velocityUpdateJobHandle.Complete();
        _positionUpdateJobHandle.Complete();
    }

    /// <summary>
    /// 後処理
    /// </summary>
    private void OnDestroy() {
        _velocityArray.Dispose();
        _transformsAccessArray.Dispose();
    }

    /// <summary>
    /// 指定された個数のオブジェクトを生成
    /// </summary>
    /// <returns></returns>
    private GameObject[] CreateObjects() {
        var objs = new GameObject[_count];
        var objToCopy = CreateTemplate();

        for ( int i = 0 ; i < _count ; i++ ) {
            var cube = GameObject.Instantiate(objToCopy);
            cube.transform.position = new Vector3(
                UnityEngine.Random.Range(_minPos.x, _maxPos.x),
                UnityEngine.Random.Range(_minPos.y, _maxPos.y),
                UnityEngine.Random.Range(_minPos.z, _maxPos.z));
            objs[i] = cube;
        }

        GameObject.Destroy(objToCopy);

        return objs;
    }

    /// <summary>
    /// オブジェクト生成
    /// </summary>
    /// <returns></returns>
    public static GameObject CreateTemplate() {
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
