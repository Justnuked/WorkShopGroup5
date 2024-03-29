﻿using Panda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    // Link to the Panda BT Documentation
    //http://www.pandabehaviour.com/?page_id=23
    //=========================================

    public Transform player;
    public Transform bulletSpawn;
    public GameObject bulletPrefab;

    private NavMeshAgent agent;
    public Vector3 destination; // The movement destination.
    public Vector3 target;      // The position to aim to.
    public float health = 100.0f;
    public float rotSpeed = 5.0f;

    private float visibleRange = 80.0f;
    private float shotRange = 40.0f;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = shotRange - 5;
        InvokeRepeating("Heal", 5, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Bullet")
        {
            health -= 10;
        }
    }

    private void Heal()
    {
        if (health < 100)
        {
            health ++;
        }
    }


    [Task]
    public void SetDestination(float x, float z)
    {
        Vector3 dest = new Vector3(x, 0, z);
        agent.SetDestination(dest);
        Task.current.Succeed();
    }

    [Task]
    public void SetRandomDestination()
    {
        Vector3 dest = new Vector3(Random.Range(-50, 50), 0, Random.Range(-50, 50));
        agent.SetDestination(dest);
        Task.current.Succeed();
    }

    [Task]
    public void MoveToDestination()
    {
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);

        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Task.current.Succeed();
        }
    }

    [Task]
    public void TargetPlayer()
    {
        target = player.transform.position;
        Task.current.Succeed();
    }

    [Task]
    bool Turn(float angle)
    {
        var p = this.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * this.transform.forward;
        target = p;
        return true;
    }

    [Task]
    public void LookAtTarget()
    {
        Vector3 direction = target - this.transform.position;

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
                                                Quaternion.LookRotation(direction),
                                                Time.deltaTime * rotSpeed);

        if (Task.isInspected)
            Task.current.debugInfo = string.Format("angle={0}",
                Vector3.Angle(this.transform.forward, direction));

        if (Vector3.Angle(this.transform.forward, direction) < 5.0f)
        {
            Task.current.Succeed();
        }
    }

    [Task]
    public bool Fire()
    {
        GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position,
                                                           bulletSpawn.transform.rotation);
        Destroy(bullet, 3f);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2000);
        return true;
    }

    [Task]
    bool SeePlayer()
    {
        Vector3 distance = player.transform.position - this.transform.position;

        RaycastHit hit;
        bool seeWall = false;

        Debug.DrawRay(this.transform.position, distance, Color.red);

        if (Physics.Raycast(this.transform.position, distance, out hit))
        {
            if (hit.collider.gameObject.tag == "Wall")
            {
                seeWall = true;
            }
        }

        if (Task.isInspected)
            Task.current.debugInfo = string.Format("wall={0}", seeWall);

        if (distance.magnitude < visibleRange && !seeWall)
            return true;
        else
            return false;
    }

    [Task]
    public bool IsHealthLessThan(float health)
    {
        return this.health < health;
    }

    [Task]
    public bool InDanger(float minDist)
    {
        Vector3 distance = player.transform.position - this.transform.position;
        return (distance.magnitude < minDist);
    }

    [Task]
    public void TakeCover()
    {
        Vector3 awayFromPlayer = this.transform.position - player.transform.position;
        Vector3 dest = this.transform.position + awayFromPlayer * 2;
        agent.SetDestination(dest);
        Task.current.Succeed();
    }

    [Task]
    public bool Explode()
    {
        Destroy(this.gameObject);
        return true;
    }

    [Task]
    public void SetTargetDestination()
    {
        agent.SetDestination(target);
        Task.current.Succeed();
    }

    [Task]
    bool ShotLinedUp()
    {
        Vector3 distance = target - this.transform.position;
        if (distance.magnitude < shotRange &&
            Vector3.Angle(this.transform.forward, distance) < 1.0f)
            return true;
        else
            return false;
    }
}
