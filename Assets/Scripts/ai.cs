using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;
    public float searchRadius = 5.0f; // How far to look for a valid floor near the player

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.Warp(transform.position);
        }
    }

    void Update()
    {
        if (player != null && agent != null && agent.isOnNavMesh)
        {
            NavMeshHit hit;
            // This looks for the closest valid NavMesh point within 'searchRadius' of the player
            if (NavMesh.SamplePosition(player.position, out hit, searchRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                // If the player is WAY off the mesh, the agent will still try to get as close as possible
                agent.SetDestination(player.position);
            }
        }
    }
}