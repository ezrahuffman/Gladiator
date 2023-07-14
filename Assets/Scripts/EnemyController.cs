using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;


public class EnemyController : MonoBehaviour
{
    HealthSystem healthSystem;

    [SerializeField] private HealthBar healthBar;
    [SerializeField] private TwoBoneIKConstraint[] iKConstraints;
    [SerializeField] private float _reactionLerpTime = 0.1f;



    private Animator _animator;
    private bool _hasAnimator;
    private int _animIDHitTrigger; 
    [SerializeField] private float _forceFactor = 0.1f;

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        if(healthSystem != null)
        {
            healthSystem.onTakeDmg += OnTakeDamage;
            healthSystem.onReducedToNoHealth += OnReducedToNoHealth;
            healthSystem.onHealthChanged += OnHealthChanged;
        }

        _hasAnimator = TryGetComponent(out _animator);
        if (_hasAnimator)
        {
            _animIDHitTrigger = Animator.StringToHash("HitTrigger");
        }
    }

    protected virtual void OnTakeDamage(float dmg, GameObject dmgSource, Vector3 forceDir, Vector3 impactPoint)
    {
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
        //if(_hasAnimator)
        //{
        //    _animator.SetTrigger(_animIDHitTrigger);
        //}
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
        Debug.Log($"{gameObject} has been reduced to no health");
    }

    protected virtual void OnHealthChanged(float maxHealth, float currHealth)
    {
        healthBar.UpateHealthbar(maxHealth, currHealth);
    }
}
