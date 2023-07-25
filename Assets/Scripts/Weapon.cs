using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Weapon : MonoBehaviour
{
    public delegate void WeaponCollision(Collision collision, Vector3 handPos, Weapon weapon);
    public WeaponCollision OnWeaponCollision;

    //public TwoBoneIKConstraint IK;
    public float animRecoveryTime = .7f;

    //[Tooltip("The original position of the IK target relative to player pos")]
    //public Vector3 IK_TargetOriginalPos; // The original position of the IK target
    /// <summary>
    /// [SerializeField] Transform playerTrans;
    /// </summary>
    /// 
    string _targetTag = "Target";
    [SerializeField] Transform _parentTrans;


    private void Start()
    {
        //Invoke(nameof(SetPos), .1f);

        //_targetTag = gameObject.tag == "Player" ? "Enemy" : "Player";
    }

    //private void SetPos()
    //{
    //    IK_TargetOriginalPos = transform.position - playerTrans.position;

    //    Debug.Log($"IK_TargetOriginalPos {IK_TargetOriginalPos} = transform.position {transform.position} - playerTrans.position{playerTrans.position}");
    //}

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.transform.IsChildOf(_parentTrans))
        {
            return;
        }

        if (!collision.gameObject.CompareTag(_targetTag))
        {
            return;
        }

        OnWeaponCollision?.Invoke(collision, transform.position, this);
    }
}
