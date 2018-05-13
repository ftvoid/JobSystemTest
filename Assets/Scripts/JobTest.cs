﻿using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using System;

/// <summary>
/// C# Job Systemテスト用
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
    /// 更新ジョブ
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

    private void Start() {
        _objectArray = AddObject();
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

    private void LateUpdate() {
        _velocityUpdateJobHandle.Complete();
        _positionUpdateJobHandle.Complete();
    }

    private void OnDestroy() {
        _velocityArray.Dispose();
        _transformsAccessArray.Dispose();
    }

    private GameObject[] AddObject() {
        var cubes = new GameObject[_count];
        var cubeToCopy = MakeStrippedCube();

        for ( int i = 0 ; i < _count ; i++ ) {
            var cube = GameObject.Instantiate(cubeToCopy);
            //cube.transform.position = UnityEngine.Random.insideUnitSphere * _radius;
            cube.transform.position = new Vector3(
                UnityEngine.Random.Range(_minPos.x, _maxPos.x),
                UnityEngine.Random.Range(_minPos.y, _maxPos.y),
                UnityEngine.Random.Range(_minPos.z, _maxPos.z));
            cubes[i] = cube;
        }

        GameObject.Destroy(cubeToCopy);

        return cubes;
    }

    public static GameObject MakeStrippedCube() {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        //turn off shadows entirely
        var renderer = cube.GetComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        // disable collision
        var collider = cube.GetComponent<Collider>();
        collider.enabled = false;

        return cube;
    }
}
