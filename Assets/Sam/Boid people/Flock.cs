using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public FlockAgent agentPrefab;
    List<FlockAgent> agents = new List<FlockAgent>();
    public FlockBehavior behavior;

    [SerializeField]
    private bool setManagerAsParent = false;

    [Space(10f)]
    [Header("Settings")]
    [Range(10, 500)]
    public int startingCount = 50;
    const float AgentDensity = 2f;

    [Range(1f, 100f)]
    public float drivefactor = 50f;
    [Range(1f, 100f)]
    public float maxspeed = 100f;
    [Range(1f, 20f)]
    public float neighborRadius = 10f;
    [Range(0f, 1f)]
    public float avoidenceRadiusMultiplier = 0.5f;

    [Space(15f)]
    [SerializeField]
    private float flockUpdateTime = 2f;

    private float flockUpdateTime_now = 0;
    
    float squareMaxSpeed;
    float squareNaighborRadius;
    float squareAvoidenceRadius;
    public float SquareAvoidenceRadius { get { return squareAvoidenceRadius; } }

    // Start is called before the first frame update
    void Start()
    {
        flockUpdateTime_now = Random.Range(0, flockUpdateTime);
        squareMaxSpeed = maxspeed * maxspeed;
        squareNaighborRadius = neighborRadius * neighborRadius;
        squareAvoidenceRadius = squareNaighborRadius * avoidenceRadiusMultiplier * avoidenceRadiusMultiplier;
        Transform parent;
        if (setManagerAsParent&&FlockManager.current)
        {
            parent = FlockManager.current.transform;
        }
        else
        {
            parent = transform;
        }

        for (int i=0; i < startingCount; i++)
        {
            FlockAgent newAgent = Instantiate(
                agentPrefab,
                (Random.insideUnitSphere * startingCount * AgentDensity)+transform.position, 
                Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)),
                parent
                );
            newAgent.name = "Agent " + i;
            newAgent.Initialize(this);
            agents.Add(newAgent);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        flockUpdateTime_now -= Time.deltaTime;
        if (flockUpdateTime_now < 0)
        {
            flockUpdateTime_now = flockUpdateTime;
            foreach (FlockAgent agent in agents)
            {
                List<Transform> context = GetNearbyObjects(agent);


                //FOR DEMO ONLY
                //agent.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.red, context.Count / 6f);

                Vector3 move = behavior.CalculateMove(agent, context, this);
                move *= drivefactor;
                if (move.sqrMagnitude > squareMaxSpeed)
                {
                    move = move.normalized * maxspeed;
                }
                agent.Move(move);
            }
        }
    }

    List<Transform> GetNearbyObjects(FlockAgent agent)
    {
        List<Transform> context = new List<Transform>();
        Collider[] contextConlliders = Physics.OverlapSphere(agent.transform.position, neighborRadius);
        foreach (Collider c in contextConlliders)
        {
            if (c != agent.AgentCollider)
            {
                context.Add(c.transform);
            }
        }
        return context;
    }
}
