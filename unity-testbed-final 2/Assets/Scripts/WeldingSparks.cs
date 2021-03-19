using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class WeldingSparks : StateMachineBehaviour
{


    ParticleSystem sys;
    // This will be called when the animator first transitions to this state.
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // If the particle system already exists then exit the function.
        sys = animator.GetComponentInChildren<ParticleSystem>();

        if (!sys.isPlaying)
        {
            //if (stateInfo.normalizedTime > 1)
                sys.Play();
        }
    }


    // This will be called once the animator has transitioned out of the state.
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // When leaving the special move state, stop the particles.
        sys.Stop();
    }


    // This will be called every frame whilst in the state.
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       
    }
}
