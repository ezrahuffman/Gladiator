using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Animations.Rigging;




public class SelfPlayEnemy : Agent
{
    [SerializeField] SelfPlayEnvController _envController;

    [HideInInspector] public HealthSystem healthSystem;

    [SerializeField] private HealthBar healthBar;
    [SerializeField] private TwoBoneIKConstraint[] iKConstraints;
    [SerializeField] private float _reactionLerpTime = 0.1f;


    [SerializeField] private float _turnSpeed = 0.3f;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _attackTimeOut = 1f;
    private float _attackTimer;

    [SerializeField] private Vector2 _inputMove;
    [SerializeField] private bool _inputAttack;

    [SerializeField, Tooltip("Disables some resets while testing behaviour")]
    private bool _testing = false;

    //Inspector variables
    [Header("ML Agent Settings")]
    [SerializeField] private float _killReward = 1f;
    [SerializeField] private float _missedPenalty = -1f;


    private Vector3 _startPosition = Vector3.zero;
    private Quaternion _startRotation;
    private StarterAssetsInputs _input;

    private int _animIDRightPunchTrigger;
    private int _animIDLeftPunchTrigger;
    private int _animIDRightKickTrigger;
    private int _animIDLeftKickTrigger;
    private bool _hasAnimator;
    private Animator _animator;

    [SerializeField] private Weapon[] _weapons;

    [SerializeField] private Transform LockOnTarget;

    private bool _attacking;
    private bool _attackHit;
    private AttackType _attackType;
    private HealthSystem _targetHealthSystem;


   

    public void Start()
    {
        //_useUpdate = false; // fortraing we don't want to call the update loop from Unities update loop
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;

        _hasAnimator = TryGetComponent(out _animator);
        if (_hasAnimator)
        {
            _animIDRightPunchTrigger = Animator.StringToHash("PunchRightTrigger");
            _animIDLeftPunchTrigger = Animator.StringToHash("PunchLeftTrigger");
            _animIDRightKickTrigger = Animator.StringToHash("KickRightTrigger");
            _animIDLeftKickTrigger = Animator.StringToHash("KickLeftTrigger");
        }

        _attackTimer = Time.time;

        foreach (var weapon in _weapons)
        {
            weapon.OnWeaponCollision += OnWeaponCollision;
        }

        //StarterAssetsInputs starterAssetsInputs = gameObject.GetComponent<StarterAssetsInputs>();
        //Debug.Log($"EnemyController starterAssetsInputs {starterAssetsInputs}");
        //_input = new InputWrapper(starterAssetsInputs);
        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.onTakeDmg += OnTakeDamage;
            healthSystem.onReducedToNoHealth += OnReducedToNoHealth;
            //healthSystem.onHealthChanged += OnHealthChanged;
        }

        _targetHealthSystem = LockOnTarget.GetComponent<HealthSystem>();


        _input = GetComponent<StarterAssetsInputs>();
    }
    private void OnTriggerEnter(Collider other)
    {
       
        Debug.Log($"collided with {other.gameObject}");
        if (other.gameObject.CompareTag("Wall") && !_testing)
        {

            //AddReward(-50);
            _envController.EndEpisode(this, lost: true);
        }
    }
    
    private void OnWeaponCollision(Collision collision, Vector3 handPos, Weapon weapon)
    {
        if (!_attacking)
        {
            return;
        }

        float dmg = 10f;


        if (collision.gameObject.TryGetComponent(out HealthSystem targetHealthSystem))
        {
            float trueDmg = targetHealthSystem.RecieveDmg(dmg, gameObject, Vector3.forward, Vector3.zero);

            //AddReward(trueDmg * 2);

            _attackHit = trueDmg != 0f; // Blocked hits are treated as misses

            //if (healthSystem.Health <= 0)
            //{
            //    AddReward(_killReward);
            //    EndEpisode();
            //}
        }


    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        float x = 0;
        float y = 0;
        bool punchRight = false;
        if (_input != null)
        {
            x = _input.move.x;
            y = _input.move.y;
            punchRight = _input.punchRight;
        }

        actionsOut.ContinuousActions.Array[0] = x;
        actionsOut.ContinuousActions.Array[1] = y;



        actionsOut.DiscreteActions.Array[0] = punchRight ? 1 : 0;

        Debug.Log($"Heuristic x: {x}, y: {y}");
    }

    
    protected virtual void OnTakeDamage(float dmg, GameObject dmgSource, Vector3 forceDir, Vector3 impactPoint)
    {
       _envController.TakeDmg(this, dmg);
    }

    protected void OnReducedToNoHealth(HealthSystem hS)
    {
        _envController.ReducedToNoHealth(this, _killReward);
    }

    protected virtual void OnHealthChanged(float maxHealth, float currHealth)
    {
        healthBar.UpateHealthbar(maxHealth, currHealth);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Set floats/Vector2s (continuous actions)
        _inputMove = new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        transform.Translate(_inputMove.x * Time.deltaTime * _moveSpeed, 0f, _inputMove.y * Time.deltaTime * _moveSpeed);
        float inputRot = actions.ContinuousActions[2];

        // Set bool (descrete actions)
        _inputAttack = actions.DiscreteActions[0] == 1;
        _attackType = (AttackType)actions.DiscreteActions[1];

        //Do attack (simple punch to start)
        if (_inputAttack && _attackTimer < Time.time)
        {
            _attacking = true;
            _attackHit = false;
            int trigger = _animIDRightPunchTrigger; // right punch is default attack
            switch (_attackType)
            {
                case AttackType.PunchLeft:
                    //Debug.Log("Punch left");
                    trigger = _animIDLeftPunchTrigger;
                    break;
                case AttackType.PunchRight:
                    //Debug.Log("Punch Right");
                    trigger = _animIDRightPunchTrigger;
                    break;
                case AttackType.KickRight:
                    //Debug.Log("Kick Right");
                    trigger = _animIDRightKickTrigger;
                    break;
                case AttackType.KickLeft:
                    //Debug.Log("Kick Left");
                    trigger = _animIDLeftKickTrigger;
                    break;
                default:
                    //Debug.LogError("Invalid attack type");
                    break;
            }

            if (_hasAnimator)
            {
                _animator.SetTrigger(trigger);
            }

            _attackTimer = _attackTimeOut + Time.time;

            _inputAttack = false;
        }


        // Don't move while attacking
        if (_attackTimer > Time.time)
        {
            _inputMove = Vector2.zero;
        }
        else
        {
            // we just finished attacking
            if (_attacking)
            {
                // if (!_attackHit) { AddReward(_missedPenalty); }
                _attacking = false;
                _attackHit = false;
            }
            //Vector3 lookDir = LockOnTarget.localPosition - transform.localPosition;
            float lookRot = Mathf.LerpAngle(transform.rotation.eulerAngles.y, inputRot, Time.deltaTime * _turnSpeed);
            transform.rotation = Quaternion.Euler(Vector3.up * inputRot);
        }

        // Add Rewards
        float dist = 10 - Vector3.Distance(transform.localPosition, LockOnTarget.localPosition);
        //AddReward(dist * 0.1f * Time.deltaTime);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(LockOnTarget.localPosition.x);
        sensor.AddObservation(LockOnTarget.localPosition.z);
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        sensor.AddObservation(_targetHealthSystem.IsBlocking);
    }

    public override void OnEpisodeBegin()
    {
        //  reset player position and add some random noise
        Vector3 diff = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-.7f, .7f));
        transform.localPosition = _startPosition + diff;

        // reset player health
        if (LockOnTarget.TryGetComponent(out HealthSystem healthSystem))
        {
            healthSystem.ResetHealth();
        }

        // reset rotation
        transform.localRotation = _startRotation;

        _attacking = false;
        _attackTimer = Time.time;
        _attackHit = false;

    }



}
