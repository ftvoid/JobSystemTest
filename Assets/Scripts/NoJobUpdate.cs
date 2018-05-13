using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoJobUpdate : MonoBehaviour {
    private Vector3 _velocity;
    private Vector3 _minPos;
    private Vector3 _maxPos;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="minPos"></param>
    /// <param name="maxPos"></param>
    public void Init(Vector3 velocity, Vector3 minPos, Vector3 maxPos) {
        _velocity = velocity;
        _minPos = minPos;
        _maxPos = maxPos;
    }

    /// <summary>
    /// フレーム更新
    /// </summary>
    private void Update() {
        UpdateVelocity();
        UpdatePosition();
    }

    /// <summary>
    /// 速度更新
    /// </summary>
    private void UpdateVelocity() {
        var vel = _velocity;
        var pos = transform.position;

        if ( vel.x < 0 && pos.x < _minPos.x )
            vel.x = -vel.x;
        if ( vel.y < 0 && pos.y < _minPos.y )
            vel.y = -vel.y;
        if ( vel.z < 0 && pos.z < _minPos.z )
            vel.z = -vel.z;
        if ( vel.x > 0 && pos.x > _maxPos.x )
            vel.x = -vel.x;
        if ( vel.y > 0 && pos.y > _maxPos.y )
            vel.y = -vel.y;
        if ( vel.z > 0 && pos.z > _maxPos.z )
            vel.z = -vel.z;

        _velocity = vel;
    }

    /// <summary>
    /// 位置更新
    /// </summary>
    private void UpdatePosition() {
        transform.position += _velocity * Time.deltaTime;
    }
}
