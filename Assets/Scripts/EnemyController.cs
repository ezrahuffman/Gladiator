using StarterAssets;
using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using UnityEngine.Animations.Rigging;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class EnemyController : ThirdPersonController
{
    HealthSystem healthSystem;

    [SerializeField] private HealthBar healthBar;
    [SerializeField] private TwoBoneIKConstraint[] iKConstraints;
    [SerializeField] private float _reactionLerpTime = 0.1f;


    [SerializeField] private float _forceFactor = 0.1f;

    private Vector2 _inputMove;
    private bool _inputSprint;
    private bool _inputCrouch;
    private bool _inputJump;
    private bool _inputPunchLeft;
    private bool _inputPunchRight;
    private bool _inputFlipJump;
    private bool _inputRoll;
    private bool _inputLockOn;
    private bool _inputIsModified;
    private float _inputLookX;

    //Inspector variables
    [Header("ML Agent Settings")]
    [SerializeField] private float _lookScale = 300f;
    [Tooltip("The reward the agent gets every second for existing")]
    [SerializeField] private float _existentialReward = 0.1f;
    [SerializeField] private float _killReward = 1f;

    private Vector3 _startPosition = Vector3.zero;
    private Quaternion _startRotation;
   

    public override void ClassStart()
    {
        _useUpdate = false; // fortraing we don't want to call the update loop from Unities update loop
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;

        base.ClassStart();

        healthSystem = GetComponent<HealthSystem>();
        if(healthSystem != null)
        {
            healthSystem.onTakeDmg += OnTakeDamage;
            healthSystem.onReducedToNoHealth += OnReducedToNoHealth;
            healthSystem.onHealthChanged += OnHealthChanged;
        }

        //_hasAnimator = TryGetComponent(out _animator);
        //if (_hasAnimator)
        //{
        //    _animIDHitTrigger = Animator.StringToHash("HitTrigger");
        //}

        _input = new InputWrapper();
    }

    public override void ClassUpdate()
    {
        base.ClassUpdate();
        AddReward(_existentialReward * Time.deltaTime);
    }

    public override void PunchHit(Collision collision, Vector3 attackingPos, Weapon weapon)
    {
        base.PunchHit(collision, attackingPos, weapon);
        if(collision.gameObject.TryGetComponent<HealthSystem>(out HealthSystem enemyHealthSystem))
        {
            if (enemyHealthSystem.Health <= 0)
            {
                AddReward(_killReward);
                EndEpisode();
            }
        }
        AddReward(punchDmg);
    }

    // Set the input for the controller
    // This is done through the input system in the player character
    public override void SetInputs()
    {
        //TODO: fix the lock on for the enemy (10 lines bellow)

        _input.Move = _inputMove; // float_x, float_y | 2 continuous values
        _input.Sprint = _inputSprint;// bool | 1 discrete value
        _input.Crouch = _inputCrouch;// bool | 1 discrete value
        _input.Jump = _inputJump;    // bool | 1 discrete value
        _input.PunchLeft = _inputPunchLeft;// bool | 1 discrete value
        _input.PunchRight = _inputPunchRight;// bool | 1 discrete value
        _input.FlipJump = _inputFlipJump;// bool | 1 discrete value
        _input.Roll = _inputRoll;// bool | 1 discrete value
        // _input.LockOn = _inputLockOn; 
        _input.IsModified = _inputIsModified;// bool | 1 discrete value
        _input.Look = new Vector2(_inputLookX, 0f);// float_x | 1 continuous value
        _input.analogMovement = false;   // this allows the magnitude of the input to be used
        _input.cursorLocked = false;     // Not sure if this changes anything
        _input.cursorInputForLook = false ; // Not sure if this changes anything
    }

    protected virtual void OnTakeDamage(float dmg, GameObject dmgSource, Vector3 forceDir, Vector3 impactPoint)
    {
        AddReward(-dmg);

        TwoBoneIKConstraint closestContraint = GetClosestConstraint(impactPoint);

        if (closestContraint == null)
        {
            Debug.LogWarning("No closest constraint found. Make sure constraints are set for this object");
            return;
        }

        Vector3 targetPos = closestContraint.data.target.position;
        Ik_Maxs ik_Max = closestContraint.gameObject.GetComponent<Ik_Maxs>();
        Vector3 displacement = forceDir * _forceFactor * dmg;
        displacement = ik_Max == null ? displacement : GetLimitedDisplacement(displacement, ik_Max);
        Debug.DrawLine(targetPos, targetPos + displacement, Color.red, 10f);
        Debug.Log($"Took ${dmg} from ${dmgSource}, targetPos = {targetPos}, forceDir: {forceDir}]");


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

    protected virtual void OnReducedToNoHealth(HealthSystem healthSystem)
    {
        SetReward(-1f); // losing is bad
        EndEpisode();
    }

    protected virtual void OnHealthChanged(float maxHealth, float currHealth)
    {
        healthBar.UpateHealthbar(maxHealth, currHealth);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //// Set floats/Vector2s
        _inputMove =  new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        _inputLookX = actions.ContinuousActions[2] * _lookScale;

        // Set bools
        _inputSprint = actions.DiscreteActions[0] == 1;
        _inputCrouch = actions.DiscreteActions[1] == 1;
        _inputJump = actions.DiscreteActions[2] == 1;
        _inputPunchLeft = actions.DiscreteActions[3] == 1;
        _inputPunchRight = actions.DiscreteActions[4] == 1;
        _inputFlipJump = actions.DiscreteActions[5] == 1;
        _inputIsModified = actions.DiscreteActions[6] == 1;
        _inputRoll = actions.DiscreteActions[7] == 1;

        ClassUpdate();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(LockOnTarget.transform.localRotation); // 4
        sensor.AddObservation(LockOnTarget.transform.localPosition); // 3
        sensor.AddObservation(LockOnTarget.GetComponent<HealthSystem>().Health); // 1
        sensor.AddObservation(transform.localPosition); // 3
        sensor.AddObservation(transform.localRotation); // 4
        sensor.AddObservation(healthSystem.Health); // 1
        if (_hasAnimator) { 
            sensor.AddObservation(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash); // 1
        }
        if (LockOnTarget.TryGetComponent<Animator>(out var oponentAnimator))
        {
            sensor.AddObservation(oponentAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash); // 1
        }

        // the sum of the commented out observations is 18
    }

    public override void OnEpisodeBegin()
    {
        //  reset player position
        transform.localPosition = _startPosition;

        // reset player health
        healthSystem.ResetHealth();

        // reset rotation
        transform.localRotation = _startRotation;

    }



}
