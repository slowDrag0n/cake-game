using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintArrow : MonoBehaviour
{
    public Transform visual;
    public Transform target1; // First target position
    public Transform target2; // Second target position
    public float speed = 2f;  // Movement speed

    private Vector3 currentTarget; // Keeps track of the current target

    void Start()
    {
        // Start moving towards target1 initially
        if(target1 != null)
            currentTarget = target1.position;
    }

    void Update()
    {
        if(target1 == null || target2 == null)
        {
            Debug.LogWarning("Please assign both target1 and target2!");
            return;
        }

        // Move the GameObject towards the current target
        visual.position = Vector3.MoveTowards(visual.position, currentTarget, speed * Time.deltaTime);

        // Check if the GameObject has reached the current target
        if(Vector3.Distance(visual.position, currentTarget) < 0.01f)
        {
            // Switch to the other target
            currentTarget = currentTarget == target1.position ? target2.position : target1.position;
        }
    }
}
