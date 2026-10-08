using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent navMeshAgent;    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       navMeshAgent = GetComponent<NavMeshAgent>(); 
    }

    private float timer = 0f;
    private float updateInterval = 0.1f;

    // Update is called once per frame
    void Update()
    {
       if (player != null)
       {
           timer += Time.deltaTime;
           if (timer >= updateInterval)
           {
               navMeshAgent.SetDestination(player.position);
               timer = 0f;
           }
       } 
    }
}
