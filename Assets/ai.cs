using UnityEngine;
using UnityEngine.AI;

public class ai : MonoBehaviour // This MUST match your filename "ai"
{
    public Transform player;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // This snaps the cube to the blue NavMesh so it can move
        if (agent != null)
        {
            agent.Warp(transform.position);
        }
    }

    void Update()
    {
        // Only move if we have a player assigned and the agent is ready
        if (player != null && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }
}
