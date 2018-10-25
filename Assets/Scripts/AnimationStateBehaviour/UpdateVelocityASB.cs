using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class UpdateVelocityASB : StateMachineBehaviour
    {
        public float _Life = 0.4f;
        public float _Force = 6f;
        public Vector3 _Direction;

        [Header("This will override the direction")]
        public bool _UseTransformForward;
        public bool _Additive;
        public bool _OnEnter;
        public bool _OnExit;

        [Header("When Applying Velocity")]
        public bool _OnEndClampVelocity;

        private StateManager mStateManager;
        private MovementHandler mMovementHandler;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_OnEnter)
            {
                if (_UseTransformForward && !_Additive)
                {
                    _Direction = animator.transform.forward;
                }
                if (_UseTransformForward && _Additive)
                {
                    _Direction += animator.transform.forward;
                }
                if (mStateManager == null)
                {
                    mStateManager = animator.transform.GetComponent<StateManager>();
                }
                if (!mStateManager._IsPlayer)
                {
                    return;
                }
                if (mMovementHandler == null)
                {
                    mMovementHandler = animator.transform.GetComponent<MovementHandler>();
                }
                mMovementHandler.AddVelocity(_Direction, _Life, _Force, _OnEndClampVelocity);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_OnExit)
            {
                if (_UseTransformForward && !_Additive)
                {
                    _Direction = animator.transform.forward;
                }
                if (_UseTransformForward && _Additive)
                {
                    _Direction += animator.transform.forward;
                }
                if (mStateManager == null)
                {
                    mStateManager = animator.transform.GetComponent<StateManager>();
                }
                if (!mStateManager._IsPlayer)
                {
                    return;
                }
                if (mMovementHandler == null)
                {
                    mMovementHandler = animator.transform.GetComponent<MovementHandler>();
                }
                mMovementHandler.AddVelocity(_Direction, _Life, _Force, _OnEndClampVelocity);
            }
        }
    }
}
