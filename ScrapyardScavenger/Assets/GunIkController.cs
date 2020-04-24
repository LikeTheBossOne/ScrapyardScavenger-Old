﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GunIkController : MonoBehaviour
{

    public bool IkActive;
    private Transform LeftTarget;
    private Transform RightTarget;
    public Transform Guns;
    private Transform current;
    protected Animator animator;
    private int slot;
    private InGameDataManager manager;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        manager = GetComponentInParent<PlayerControllerLoader>().inGameDataManager;
        manager.OnEquipmentSwitched += EquipmentSwitched;
        slot = 0;
        if (Guns.GetChild(slot))
        {
            current = Guns.GetChild(slot);
        }
    }

    private void OnDestroy()
    {
        manager.OnEquipmentSwitched -= EquipmentSwitched;
    }

    void OnAnimatorIK()
    {
        if (animator)
        {
            if (IkActive)
            {
                //right hand
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand,1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, current.Find("Anchor").GetChild(0).position);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand,1);
                animator.SetIKRotation(AvatarIKGoal.RightHand,current.Find("Anchor").GetChild(0).rotation);
                //left hand
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, current.Find("Anchor").GetChild(1).position);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,1);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, current.Find("Anchor").GetChild(1).rotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand,0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand,0);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,0);
            }
        }
    }

    [PunRPC]
    public void EquipWeapon(int index)
    {
        Debug.Log("GunIK Equip RPC");
        if (index == 0 || index == 1)
        {
            slot = index;
            if (Guns.GetChild(slot))
            {
                current = Guns.GetChild(slot);
            }
        }
    }

    void EquipmentSwitched()
    {
        foreach (Transform gun in Guns)
        {
            if (gun.gameObject.activeSelf)
            {
                current = gun;
            }
        }
    }
}
