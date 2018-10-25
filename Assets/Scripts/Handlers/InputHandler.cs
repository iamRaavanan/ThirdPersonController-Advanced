using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Raavanan.CameraWorks;
using System;

namespace Raavanan
{
    public class InputHandler : MonoBehaviour
    {
        private StateManager mStateManager;
        [HideInInspector]
        public CameraManager _CameraManager;
        private MovementHandler mMovementHandler;

        private float mHorizontal;
        private float mVertical;

        private void Start()
        {
            gameObject.AddComponent<MovementHandler>();

            // Getting Singleton & References
            _CameraManager = CameraManager._Instance;
            mStateManager = GetComponent<StateManager>();
            mMovementHandler = GetComponent<MovementHandler>();

            _CameraManager._Target = this.transform;

            // Initialize in order
            mStateManager._IsPlayer = true;
            mStateManager.Init();
            mMovementHandler.Init(mStateManager, this);

            FixPlayerMeshes();
        }

        private void FixPlayerMeshes()
        {
            SkinnedMeshRenderer[] InSMRender = GetComponentsInChildren<SkinnedMeshRenderer>();
            int InLength = InSMRender.Length;
            for (int i = 0; i < InLength; i++)
            {
                InSMRender[i].updateWhenOffscreen = true;
            }
        }

        private void FixedUpdate()
        {
            mStateManager.FixedTick();
            UpdateStatesFromInput();
            mMovementHandler.Tick();
        }

        private void Update()
        {
            mStateManager.UpdateTick();
        }

        private void UpdateStatesFromInput()
        {
            mHorizontal = Input.GetAxis(GAMEVAR.Horizontal);
            mVertical = Input.GetAxis(GAMEVAR.Vertical);

            Vector3 InHorizontal = _CameraManager.transform.right * mHorizontal;
            Vector3 InVertical = _CameraManager.transform.forward * mVertical;

            InHorizontal.y = InVertical.y = 0;

            mStateManager._Horizontal = mHorizontal;
            mStateManager._Vertical = mVertical;

            Vector3 InMoveDirection = (InHorizontal + InVertical).normalized;
            mStateManager._MoveDirection = InMoveDirection;
            mStateManager._InAngleMoveDirection = InAngle(mStateManager._MoveDirection, 25);
            if (mStateManager._Walk && (mHorizontal != 0 || mVertical != 0))
            {
                mStateManager._InAngleMoveDirection = true;
            }
            mStateManager._OnLocomotion = mStateManager.mAnimator.GetBool(GAMEVAR.onLocomotion);
            HandleRun();
            mStateManager._JumpInput = Input.GetButton(GAMEVAR.Jump);
        }

        private bool InAngle(Vector3 pTargetDir, int pThreshold)
        {
            bool IsInAngle = false;
            float InAngle = Vector3.Angle(transform.forward, pTargetDir);
            if (InAngle < pThreshold)
            {
                IsInAngle = true;
            }
            return IsInAngle;
        }

        private void HandleRun()
        {
            bool InRunInput = Input.GetButton(GAMEVAR.Fire3);
            if (InRunInput)
            {
                mStateManager._Walk = false;
                mStateManager._Run = true;
            }
            else
            {
                mStateManager._Walk = true;
                mStateManager._Run = false;
            }

            if (mHorizontal != 0 || mVertical != 0)
            {
                mStateManager._Run = InRunInput;
                mStateManager.mAnimator.SetInteger(GAMEVAR.specialType, GAMEVAR.GetAnimSpecialType(AnimSpecials.E_Run));
            }
            else
            {
                if (mStateManager._Run)
                {
                    mStateManager._Run = false;
                }
            }
            if (!mStateManager._InAngleMoveDirection && mMovementHandler._DoAngleCheck)
            {
                mStateManager._Run = false;
            }
            if (mStateManager._ObstacleForward)
            {
                mStateManager._Run = false;
            }
            if (mStateManager._Run == false)
            {
                mStateManager.mAnimator.SetInteger(GAMEVAR.specialType, GAMEVAR.GetAnimSpecialType(AnimSpecials.E_RunToStop));
            }
        }
    }
}