using StarterAssets;
using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using UnityEngine.Animations.Rigging;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using TMPro;

public enum AttackType
{
    PunchRight, PunchLeft, KickRight, KickLeft
}

public class EnemyController : Agent
{
    HealthSystem healthSystem;

    [SerializeField] private HealthBar healthBar;
    [SerializeField] private TwoBoneIKConstraint[] iKConstraints;
    [SerializeField] private float _reactionLerpTime = 0.1f;


    [SerializeField] private float _forceFactor = 0.1f;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _attackTimeOut = 1f;
    private float _attackTimer;

    [SerializeField]private Vector2 _inputMove;
    [SerializeField] private bool _inputAttack;
   
    [SerializeField, Tooltip("Disables some resets while testing behaviour")]
    private bool _testing = false;

    //Inspector variables
    [Header("ML Agent Settings")]
    [SerializeField] private float _lookScale = 300f;
    [Tooltip("The reward the agent gets every second for existing")]
    [SerializeField] private float _existentialReward = 0.1f;
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

    [SerializeField] private Weapon _rightHand;

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

        _rightHand.OnWeaponCollision += OnWeaponCollision;

        //StarterAssetsInputs starterAssetsInputs = gameObject.GetComponent<StarterAssetsInputs>();
        //Debug.Log($"EnemyController starterAssetsInputs {starterAssetsInputs}");
        //_input = new InputWrapper(starterAssetsInputs);
        /*healthSystem = GetComponent<HealthSystem>();
        if(healthSystem != null)
        {
            healthSystem.onTakeDmg += OnTakeDamage;
            //healthSystem.onReducedToNoHealth += OnReducedToNoHealth;
            healthSystem.onHealthChanged += OnHealthChanged;
        }*/

        _targetHealthSystem = LockOnTarget.GetComponent<HealthSystem>();
        

        _input = GetComponent<StarterAssetsInputs>();
    }
    private void OnCollisionEnter(Collision colliision)
    {
        //Debug.Log($"EnemyController OnControllerColliderHit {hit.gameObject.name}");
        if (colliision.gameObject.CompareTag("Wall") && !_testing)
        {
            AddReward(-50);
            EndEpisode();
        }
    }

    private void OnWeaponCollision(Collision collision, Vector3 handPos, Weapon weapon)
    {
        if(!_attacking)
        {
            return;
        }

        float dmg = 10f;


        if(collision.gameObject.TryGetComponent(out HealthSystem healthSystem))
        {
            float trueDmg = healthSystem.RecieveDmg(dmg, gameObject, Vector3.forward, Vector3.zero);

            AddReward(trueDmg * 2);

            _attackHit = trueDmg != 0f; // Blocked hits are treated as misses

            if(healthSystem.Health <= 0)
            {
                AddReward(_killReward);
                EndEpisode();
            }
        }

        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        float x = 0;
        float y = 0;
        bool punchRight = false;
        if(_input != null)
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

    /*
    protected virtual void OnTakeDamage(float dmg, GameObject dmgSource, Vector3 forceDir, Vector3 impactPoint)
    {
        //AddReward(-dmg);

        TwoBoneIKConstraint closestContraint = GetClosestConstraint(impactPoint);

        if (closestContraint == null)
        {
            //Debug.LogWarning("No closest constraint found. Make sure constraints are set for this object");
            return;
        }

        Vector3 targetPos = closestContraint.data.target.position;
        Ik_Maxs ik_Max = closestContraint.gameObject.GetComponent<Ik_Maxs>();
        Vector3 displacement = forceDir * _forceFactor * dmg;
        displacement = ik_Max == null ? displacement : GetLimitedDisplacement(displacement, ik_Max);
        //Debug.DrawLine(targetPos, targetPos + displacement, Color.red, 10f);
        //Debug.Log($"Took ${dmg} from ${dmgSource}, targetPos = {targetPos}, forceDir: {forceDir}]");


        // This probably needs to be smoothed instead of instantanious 
        StartCoroutine(LerpToTargetAndBack(closestContraint.data.target, targetPos + displacement, _reactionLerpTime));
    }

    protected Vector3 GetLimitedDisplacement(Vector3 originalDisplacement, Ik_Maxs ikMax)
    {
        DisplacementHelper(ref originalDisplacement.x, ikMax.max_x, ikMax.min_x);
        DisplacementHelper(ref originalDisplacement.y, ikMax.max_y, ikMax.min_y);
        DisplacementHelper(ref originalDisplacement.z, ikMax.max_z, ikMax.min_z);

        return originalDisplacement;
    }

    protected void DisplacementHelper(ref float original, float max, float min)
    {
        if (original > 0)
        {
            original = Mathf.Min(original, max);
        }
        else
        {
            original = Mathf.Max(original, min);
        }
    }

    IEnumerator LerpToTargetAndBack(Transform transToMove, Vector3 targetPos, float lerpTime)
    {
        float time = 0;
        Vector3 startPos = transToMove.position;
        while (time < lerpTime)
        {
            transToMove.position = Vector3.Lerp(startPos, targetPos, time / lerpTime);
            time += Time.deltaTime;
            yield return null;
        }
        time = 0;
        while (time < lerpTime)
        {
            transToMove.position = Vector3.Lerp(targetPos, startPos, time / lerpTime);
            time += Time.deltaTime;
            yield return null;
        }
        transToMove.position = startPos;
    }

    //TODO: this is likely not optimal, but should be fine for now
    protected TwoBoneIKConstraint GetClosestConstraint(Vector3 point)
    {
        float minDist = float.MaxValue;
        TwoBoneIKConstraint closestConstraint = null;
        foreach (var iKContraint in iKConstraints)
        {
            Vector3 targetPos = iKContraint.data.target.position;
            float dist = Vector3.Distance(targetPos, point);
            if (dist < minDist)
            {
                minDist = dist;
                closestConstraint = iKContraint;
            }
        }

        return closestConstraint;
    }

    //protected virtual void OnReducedToNoHealth(HealthSystem healthSystem)
    //{
    //    AddReward(_deathPenalty); // losing is bad
    //    EndEpisode();
    //}

    protected virtual void OnHealthChanged(float maxHealth, float currHealth)
    {
        healthBar.UpateHealthbar(maxHealth, currHealth);
    }*/

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Set floats/Vector2s (continuous actions)
        _inputMove = new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        transform.Translate(_inputMove.x * Time.deltaTime * _moveSpeed, 0f, _inputMove.y * Time.deltaTime * _moveSpeed);

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
        if(_attackTimer > Time.time)
        {
            _inputMove = Vector2.zero;
        }
        else
        {
            // we just finished attacking
            if(_attacking)
            {
                if (!_attackHit) { AddReward(_missedPenalty); }
                _attacking = false;
                _attackHit = false;
            }
            Vector3 lookDir = LockOnTarget.localPosition - transform.localPosition;
            transform.rotation = Quaternion.LookRotation(new Vector3(lookDir.x, 0f, lookDir.z));
        }

        // Add Rewards
        float dist = 10 - Vector3.Distance(transform.localPosition, LockOnTarget.localPosition);
        AddReward(dist * 0.1f * Time.deltaTime);
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
        //  reset player position
        transform.localPosition = _startPosition;

        // reset player health
        if(LockOnTarget.TryGetComponent(out HealthSystem healthSystem))
        {
            healthSystem.ResetHealth();
        }

        //reset the position of the target
        LockOnTarget.localPosition = new Vector3( Random.Range(-4f, 4f), LockOnTarget.localPosition.y, Random.Range(-4f, 3.8f));

        // reset rotation
        transform.localRotation = _startRotation;

        _attacking = false;
        _attackTimer = Time.time;
        _attackHit = false;

    }



}
