﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
//note: enemeyController may be useful to look at
public class ShamblerAI : MonoBehaviourPun
{
    public enum State
    {
        idle,
        wander,
        chase,
        spit,
        bite,
        Invalid
   }

    public State lastState;
    public State currentState;
    public Vector3 moveTo;
    public NavMeshAgent nav;
    public InGamePlayerManager pManager;
    //intent, second based countdown
    //public int resetDelay = 600;
    //private int timer;
    public float aggroTimeLimit;
    public float wandOffset;
    public float toPlayerOffset;
    public float wandAngle;
    public float wandRad;
    //public float playerOffset;
    public ShamblerDetection senses;
    public ShamblerAttacks weapons;
    public Animator animator;
    public Transform extractionTruck;
    // Start is called before the first frame update
    private void OnEnable()
    {
        //timer = 0;
        aggroTimeLimit = 10;
        senses = GetComponent<ShamblerDetection>();
        nav = GetComponentInParent<NavMeshAgent>();
        pManager = FindObjectOfType<InGamePlayerManager>();
        wandOffset = 10;
        toPlayerOffset = 20;
        wandAngle = 60;
        wandRad = 10;
        weapons = GetComponent<ShamblerAttacks>();
        animator = GetComponentInChildren<Animator>();
        if (animator)
        {
            Debug.Log(animator.parameters);
        }
        extractionTruck = GameObject.Find("ExtractionTruck").GetComponent<Transform>();
        //playerOffset = 5;
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ChangeState();
            HandleState();
        }
    }

    public void ChangeState()
    {
        lastState = currentState;
        if (senses.PlayersExist())
        {
            if (senses.VisionCheck())
            {
                if (DistanceToOther(senses.detected) <= weapons.meleeRange)
                {
                    //&& !weapons.meleeOnCoolDown()
                    currentState = State.bite;

                }
                else if (DistanceToOther(senses.detected) <= weapons.spitRange)
                {
                    // target in attack range/
                    //currentState = State.attack;
                    //&& !weapons.spitOnCoolDown()
                    currentState = State.spit;

                }
                else
                {
                    currentState = State.chase;

                }
                //currentState = State.chase;
            }
            else
            {
                currentState = State.wander;

            }
        }
        else
        {
            currentState = State.idle;

        }

    }
    // Update is called once per frame
    public void HandleState()
    {
        if (currentState == State.chase)
        {
            //set target destination to detected/aggressing player or use follow command if there is one
            //Need to add reorientation/ "lockon camera" for enemy
            //System.Console.WriteLine("Player seen.");
            transform.LookAt(senses.detected.position, transform.up);
            SetDestination(senses.detected.position);
            if (animator && currentState != lastState)
            {
                photonView.RPC("Walk", RpcTarget.All);
                //animator.SetBool("walking", true);
            }
        }
        if (currentState == State.wander)
        {
            //wander in the direction of closest player
            //need to rethink this with unity in mind
            //need to rethink transform.rotate()
            Vector3 wandDir = (transform.forward - transform.position).normalized;
            //project the center of the imaginary circle
            Vector3 center = wandDir * wandOffset;
            center = center + transform.position;
            //orient
            float angle = Random.Range(-1, 1) * wandAngle;
            //transform.Rotate(transform.up, angle);
            float curOrient = transform.eulerAngles.y;
            float newOrient = curOrient + angle;
            Vector3 dir = transform.forward.normalized;
            dir.x = Mathf.Sin(newOrient);
            dir.z = Mathf.Cos(newOrient);
            //project target spot on circle
            Vector3 moveTarg = center + wandRad * dir;
            //correct towards closest player
            Transform close = FindClosestPlayer();

            Vector3 toPlayer = close.position - moveTarg;
            toPlayer = toPlayer.normalized;
            if (DistanceToOther(close) < toPlayerOffset)
            {
                toPlayer = toPlayer * (float)DistanceToOther(close);
            }
            else
            {
                toPlayer = toPlayer * toPlayerOffset;
            }

            moveTarg = moveTarg + toPlayer;
            //this line is why the blue sphere warps around
            //close.position = moveTarg;
            //transform.LookAt(moveTarg, transform.up);
            moveTo = moveTarg;
            SetDestination(moveTo);
            if (animator && currentState != lastState)
            {
                photonView.RPC("Walk", RpcTarget.All);
                //animator.SetBool("walking", true);
            }
        }
        if (currentState == State.spit)
        {
            transform.LookAt(senses.detected.position, transform.up);
            SetDestination(senses.detected.position);
            if (animator && currentState != lastState)
            {
                photonView.RPC("Walk", RpcTarget.All);
                //animator.SetBool("walking", true);
            }
            weapons.Spit(senses.detected.gameObject);
        }
        if (currentState == State.bite)
        {
            SetDestination(GetComponentInParent<Transform>().position);
            gameObject.transform.LookAt(senses.detected, gameObject.transform.up);
            weapons.Bite(senses.detected.gameObject);
            if (animator && currentState != lastState)
            {
                photonView.RPC("Idle", RpcTarget.All);
                //animator.SetBool("walking", false);
            }
        }
        if (currentState == State.idle)
        {
            SetDestination(gameObject.transform.position);
            if (animator && currentState != lastState)
            {
                photonView.RPC("Idle", RpcTarget.All);
                //animator.SetBool("walking", false);
            }
        }

    }

    Transform FindClosestPlayer()
    {
        Transform closest = null;
        double cDist = Mathf.Infinity;

        // Find closest player or vehicle
        foreach (GameObject obj in pManager.players)
        {
            Transform player = obj.GetComponent<Transform>();

            double dist = DistanceToOther(player);
            if (dist < cDist)
            {
                closest = player;
                cDist = dist;
            }
        }
        if (DistanceToOther(extractionTruck) < cDist)
        {
            return extractionTruck;
        }
        return closest;
    }

    void SetDestination(Vector3 destination)
    {
        nav.SetDestination(destination);
    }

    double DistanceToOther(Transform other)
    {
        return Mathf.Sqrt(Mathf.Pow(other.position.x - transform.position.x, 2) + Mathf.Pow(other.position.y - transform.position.y, 2) + Mathf.Pow(other.position.z - transform.position.z, 2));
    }

    [PunRPC]
    public void Walk()
    {
        animator.SetBool("walking", true);
    }

    [PunRPC]
    public void Idle()
    {
        animator.SetBool("walking", true);
    }
}
