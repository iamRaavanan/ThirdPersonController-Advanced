using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class UpdateBoolASB : StateMachineBehaviour
    {
        public string _BoolName;
        public bool _Status;
        public bool _ResetOnExit;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(_BoolName, _Status);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_ResetOnExit)
            {
                animator.SetBool(_BoolName, !_Status);
            }
        }
    }
}
