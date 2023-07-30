using UnityEngine;

public class TrainingTarget : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 5f;
    Vector2 dir = Vector2.left;
    private Rigidbody _rb;
    private bool _block = false;
    private bool _blocking =false;

    int _frameCount = 0;
    int _frames = 10;
    private float _changeDirTimer;
    private float _changeBlockTimer;
    private float _blockTimeoutTimer;
    [SerializeField] float _changeDirTime = 1.5f;
    [SerializeField] float _changeBlockTime = 1.5f;
    [SerializeField] float _blockTimeoutTime = 0.5f;

    private Vector2 _storedDir = Vector2.zero;

    private HealthSystem _healthSystem;
    public HealthSystem Health { get { return _healthSystem; } }

    [SerializeField] Material _mat;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _changeDirTimer = Time.time + _changeDirTime;

        TryGetComponent(out _healthSystem);
        _mat = GetComponent<MeshRenderer>().material;
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

        if(!_blocking && Time.time > _changeBlockTimer)
        {
            _block = Random.Range(0, 2) == 0;
            _changeBlockTimer = Time.time + _changeBlockTime;
        }

        if(_block && Time.time > _blockTimeoutTimer)
        {
            _block = false;
            _blockTimeoutTimer = Time.time + _blockTimeoutTime;
            SetBlocking(true);
        }

        if (Time.time < _blockTimeoutTimer)
        {
            dir = Vector2.zero;
        }
        else if(_blocking)
        {
            dir = _storedDir;
            SetBlocking(false);
        }


        Vector3 delta = new Vector3(dir.x, 0, dir.y).normalized * _moveSpeed * Time.deltaTime;
        //_rb.MovePosition((transform.position + delta) * Time.deltaTime);
        transform.Translate(delta);
    }

    private void SetBlocking(bool blocking)
    {
        _blocking = blocking;
        _healthSystem?.SetBlock(_blocking);

        _mat.color = _blocking ? Color.red : Color.white;
    }
}
