using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-200)]
public class PetFollow : MonoBehaviour
{
    [Header("Tracking Settings")]
    public Transform playerTransform; // Drag your Player object here
    public float headHeight = 1.1f; // Height above the player's pivot
    public float orbitRadius = 0.8f;
    public float orbitSpeed = 120.0f; // Degrees per second

    private float orbitAngle;
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

    void OnDestroy()
    {
        navMeshInstance.Remove();
    }

    void LateUpdate()
    {
        if (playerTransform == null)
        {
            return;
        }

        orbitAngle += orbitSpeed * Time.deltaTime;
        Vector3 offset = Quaternion.Euler(0.0f, orbitAngle, 0.0f) * Vector3.forward * orbitRadius;
        Vector3 target = playerTransform.position + Vector3.up * headHeight + offset;
        Vector3 direction = target - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        transform.position = target;
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
