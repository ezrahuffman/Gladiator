using System.Collections;
using System.Collections.Generic;
using UnityEditor.Profiling;
using UnityEngine;

public class TrainingTarget : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 5f;
    Vector2 dir = Vector2.left;
    private Rigidbody _rb;

    int _frameCount = 0;
    int _frames = 10;
    private float _changeDirTimer;
    [SerializeField] float _changeDirTime = 1.5f;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _changeDirTimer = Time.time + _changeDirTime;
    }

    private void FixedUpdate()
    {
        _frameCount++;
        if(_frameCount % _frames != 0)
        {
            return;
        }

        if (Time.time > _changeDirTimer)
        {
            dir.x = Random.Range(-1f, 1f);
            dir.y = Random.Range(-1f, 1f);
            _changeDirTimer = Time.time + _changeDirTime;
        }
        Vector3 delta = new Vector3(dir.x, 0, dir.y).normalized * _moveSpeed * Time.deltaTime;
        _rb.MovePosition(transform.position + delta);
    }
}
