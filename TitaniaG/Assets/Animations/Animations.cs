using UnityEngine;
using System.Collections.Generic;
public class Animations : MonoBehaviour
{
    Animator animator;
    private string currentState;

    void Start()
    {
        animator = GetComponent<Animator>();
    }


    void ChangeAnimationState(string newState)
    {
        //stop the animation from interrupting itself
        if (currentState == newState) return;

        //play the animation 
        animator.Play(newState);

        //reaassign the current state
        currentState = newState;

    }
}
