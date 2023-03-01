using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Flock/Behavior/Avoidance")]
public class AvoidanceBehaviour : FilteredFlockBehavior
{
    public override Vector3 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock)
    {
        //if no neighbors, return no adjustment
        if (context.Count == 0)
            return Vector3.zero;

        //add all points together and average
        Vector3 avoidnaceMove = Vector3.zero;
        int nAvoid = 0;
        List<Transform> filteredContext = (filter == null) ? context : filter.Filter(agent, context);
        foreach (Transform item in filteredContext)
        {
            if (Vector3.SqrMagnitude(item.position - agent.transform.position) < flock.SquareAvoidenceRadius)
                avoidnaceMove += (Vector3)(agent.transform.position - item.position);
        }

        if (nAvoid > 0)
            avoidnaceMove /= nAvoid;

        return avoidnaceMove;
    }

}

//RaycastHit hit;
//if (Physics.Raycast(agent.transform.position, (item.position - agent.transform.position).normalized, out hit, Mathf.Infinity))
//{
//    Debug.DrawRay(agent.transform.position, (item.position - agent.transform.position).normalized * hit.distance, Color.yellow);
//    if (hit.distance > flock.SquareAvoidenceRadius)
//        avoidnaceMove += (Vector3)(agent.transform.position - item.position);
//}