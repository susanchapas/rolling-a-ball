using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-200)]
[RequireComponent(typeof(NavMeshAgent))]
public class PetFollow : MonoBehaviour
{
    [Header("Tracking Settings")]
    public Transform playerTransform; // Drag your Player object here
    public float stopDistance = 2.0f; // Distance to keep from the player

    private NavMeshAgent agent;
    private Animation legacyAnimation;
    private NavMeshData navMeshData;
    private NavMeshDataInstance navMeshInstance;

    void Awake()
    {
        navMeshData = new NavMeshData();
        BakeNavMesh();
        navMeshInstance = NavMesh.AddNavMeshData(navMeshData);
        foreach (NavMeshSurface surface in FindObjectsByType<NavMeshSurface>())
        {
            surface.enabled = false;
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        legacyAnimation = GetComponentInChildren<Animation>();
        agent.stoppingDistance = stopDistance;
    }

    void OnDestroy()
    {
        navMeshInstance.Remove();
    }

    void Update()
    {
        if (playerTransform == null || !agent.isOnNavMesh)
        {
            return;
        }

        if (!agent.pathPending)
        {
            if (agent.pathStatus == NavMeshPathStatus.PathPartial
                && NavMesh.SamplePosition(playerTransform.position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            agent.SetDestination(playerTransform.position);
        }

        if (legacyAnimation != null)
        {
            legacyAnimation.CrossFade(agent.velocity.magnitude > 0.1f ? "walk" : "idle");
        }
    }

    /// <summary>Rebuilds one NavMesh over all static colliders in the scene.</summary>
    public void BakeNavMesh()
    {
        var sources = new List<NavMeshBuildSource>();
        var everywhere = new Bounds(Vector3.zero, Vector3.one * 10000.0f);
        NavMeshBuilder.CollectSources(everywhere, ~0, NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sources);
        sources.RemoveAll(s => s.component is Collider c && (c.isTrigger || c.attachedRigidbody != null));

        var bounds = new Bounds(transform.position, Vector3.zero);
        foreach (NavMeshBuildSource source in sources)
        {
            if (source.component is Collider c)
            {
                bounds.Encapsulate(c.bounds);
            }
        }
        bounds.Expand(1.0f);

        NavMeshBuilder.UpdateNavMeshData(navMeshData, NavMesh.GetSettingsByID(0), sources, bounds);
    }
}
