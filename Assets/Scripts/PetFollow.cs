using UnityEngine;
using UnityEngine.AI;

public class PetFollow : MonoBehaviour
{
    [Header("Tracking Settings")]
    public Transform playerTransform; // Drag your Player object here
    public float stopDistance = 2.0f; // Distance to keep from the player

    private NavMeshAgent agent;
    private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Stop a comfortable distance away from the player
        agent.stoppingDistance = stopDistance; 
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTransform != null)
        {
            // Tell the NavMesh Agent to target the player
            agent.SetDestination(playerTransform.position);

            // Update animations based on movement speed
            UpdateAnimations();
        }
    }
    void UpdateAnimations()
    {
        if (animator != null)
        {
            // Check if the agent is actively moving faster than a threshold
            bool isMoving = agent.velocity.magnitude > 0.1f;
            
            // Set your animator parameter (assumes you have a boolean parameter named "IsMoving" or "Walk")
            animator.SetBool("IsMoving", isMoving);
        }
    }
}
