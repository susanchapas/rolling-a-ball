using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Transform player;
    public Transform platform;
    private NavMeshAgent navMeshAgent;

    void Start()
    {
       navMeshAgent = GetComponent<NavMeshAgent>();
       if (!navMeshAgent.isOnNavMesh
           && NavMesh.SamplePosition(transform.position, out NavMeshHit hit, navMeshAgent.height * 2.0f, NavMesh.AllAreas))
       {
           navMeshAgent.Warp(hit.position);
       }
    }

    private float timer = 0f;
    private float updateInterval = 0.1f;

    void Update()
    {
       if (player != null && navMeshAgent.isOnNavMesh && IsPlayerOnPlatform())
       {
           timer += Time.deltaTime;
           if (timer >= updateInterval)
           {
               navMeshAgent.SetDestination(player.position);
               timer = 0f;
           }
       }
    }

    private bool IsPlayerOnPlatform()
    {
        return Physics.Raycast(player.position, Vector3.down, out RaycastHit hit) && hit.transform.IsChildOf(platform);
    }
}
