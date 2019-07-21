using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static S_Data;

public class S_HBHandler : StateMachineBehaviour
{
    [SerializeField] public ActionType action;
    Move data;
    HBData[] frames;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        data = animator.gameObject.GetComponent<S_Player>().moveset[action];
        frames = data.hitbox;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        for (int i = 0; i < frames.Length; i++)
        {
            if (stateInfo.normalizedTime >= frames[i].start / data.duration)
            {
                animator.gameObject.GetComponent<S_Player>().GetSensors.Find(
                    x => x.GetInstance == action).GetSensors[i].getColliderState = ColliderState.hit;

            }
            else if (stateInfo.normalizedTime >= frames[i].end / data.duration)
            {
                animator.gameObject.GetComponent<S_Player>().GetSensors.Find(
                    x => x.GetInstance == action).GetSensors[i].getColliderState = ColliderState.none;
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        for (int i = 0; i < frames.Length; i++)
        {
            animator.gameObject.GetComponent<S_Player>().GetSensors.Find(
                x => x.GetInstance == action).GetSensors[i].getColliderState = ColliderState.none;
        }

    }
    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}