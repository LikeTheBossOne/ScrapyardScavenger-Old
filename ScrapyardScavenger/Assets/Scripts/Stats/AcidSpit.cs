﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AcidSpit : MonoBehaviour
{
    public GameObject shooter { get; set; }
    public AIPlayerManager pManage { get; set; }
    public int maxExistTime = 10;
    public int maxV = 10;
    public Vector3 direction;
    private void OnEnable()
    {
        pManage = FindObjectOfType<AIPlayerManager>();
        Destroy(gameObject, maxExistTime);
    }

    public void Shoot(GameObject creator, Vector3 dir)
    {
        shooter = creator;
        direction = dir.normalized;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.bounds.Intersects(shooter.GetComponent<Collider>().bounds))
        {
            foreach (Transform player in pManage.players)
            {
                if (collision.collider.bounds.Contains(player.position))
                {
                    player.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, shooter.GetComponent<ShamblerAttacks>().spitDamage);
                }
            }
            

            
            Destroy(gameObject);
        }
    }
}
