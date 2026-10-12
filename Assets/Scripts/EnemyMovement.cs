using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Transform player;
    public Transform activationGround;
    private NavMeshAgent navMeshAgent;    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       navMeshAgent = GetComponent<NavMeshAgent>(); 
       if (!navMeshAgent.isOnNavMesh)
       {
           navMeshAgent.enabled = false;
           navMeshAgent.enabled = true;
       }
    }

    private float timer = 0f;
    private float updateInterval = 0.1f;

    // Update is called once per frame
    void Update()
    {
       if (player != null && IsActivated())
       {
           timer += Time.deltaTime;
           if (timer >= updateInterval)
           {
               navMeshAgent.SetDestination(player.position);
               timer = 0f;
           }
       } 
    }

    private bool IsActivated()
    {
        if (activationGround != null && Physics.Raycast(player.position, Vector3.down, out RaycastHit hit) && hit.transform == activationGround)
        {
            activationGround = null;
        }
        return activationGround == null;
    }
}
