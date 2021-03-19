using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceAnimation : StateMachineBehaviour {

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
    int dir = 1;
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       // Debug.Log(stateInfo.normalizedTime);
        if (stateInfo.normalizedTime > 0.75f && dir > 0)
        {
            animator.SetFloat("bounce", -1f);
            dir = -1;
        }

        if (stateInfo.normalizedTime < 0.0015f && dir < 0 && animator.GetBool("fixing tube"))
        {
            animator.SetFloat("bounce", 1f);
            dir = 1;
        }
        //Debug.Log(stateInfo.normalizedTime);

        if(!animator.GetBool("fixing tube"))
        {
            if (dir > 0)
            {
                dir = -1;
                animator.SetFloat("bounce", -1f);
            }

            if (stateInfo.normalizedTime <= 0.0015f)
            {
             //   Debug.Log("Triggered transition");
                animator.SetBool("reset", true);
                animator.SetFloat("bounce", 1f);
                dir = 1;
            }
        }

    }
}
