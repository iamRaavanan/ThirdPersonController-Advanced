using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class MovementHandler : MonoBehaviour
    {
        private StateManager mStateManager;
        private Rigidbody mRigidBody;

        public bool _DoAngleCheck = true;
        [SerializeField]
        private float mDegreesRunThreshold = 8;
        [SerializeField]
        private bool mUseDot = true;

        private bool mOverrideForce;
        private bool mInAngle;

        private float mRotateTimer;
        private float mVelocityChange = 4;
        private bool mApplyJumpForce;

        private Vector3 mStoreDirection;
        private InputHandler mInputHandler;

        private Vector3 mCurrentVelocity;
        private Vector3 mTargetVelocity;
        private float mPrevAngle;
        private Vector3 mPrevDirection;

        private Vector3 mOverrideDirection;
        private float mOverrideSpeed;
        private float mForceOverrideTimer;
        private float mForceOverLife;
        private bool mStopVelocity;

        public void Init (StateManager pStateManager, InputHandler pInputHandler)
        {
            mInputHandler = pInputHandler;
            mStateManager = pStateManager;
            mRigidBody = mStateManager.mRigidBody;
            mStateManager.mAnimator.applyRootMotion = false;
        }

        public void Tick ()
        {
            if (!mOverrideForce)
            {
                HandleDrag();
                if (mStateManager._OnLocomotion)
                {
                    MovementNormal();
                }
                HandleJump();
            }
            else
            {
                mStateManager._Horizontal = mStateManager._Vertical = 0;
                OverrideLogic();
            }
        }

        private void MovementNormal()
        {
            mInAngle = mStateManager._InAngleMoveDirection;

            Vector3 InHorizontal = mInputHandler._CameraManager.transform.right * mStateManager._Horizontal;
            Vector3 InVertical = mInputHandler._CameraManager.transform.forward * mStateManager._Vertical;

            InHorizontal.y = InVertical.y = 0;

            if (mStateManager._OnGround)
            {
                if (mStateManager._OnLocomotion)
                {
                    HandleRotation_Normal(InHorizontal, InVertical);
                }
                float InTargetSpeed = mStateManager._WalkSpeed;
                if (mStateManager._Run && mStateManager._GroundAngle == 0)
                {
                    InTargetSpeed = mStateManager._RunSpeed;
                }
                if (mInAngle)
                {
                    HandleVelocity_Normal(InHorizontal, InVertical, InTargetSpeed);
                }
                else
                {
                    mRigidBody.velocity = Vector3.zero;
                }
            }
            HandleAnimations_Normal();
        }

        private void HandleVelocity_Normal(Vector3 pHorizontal, Vector3 pVertical, float pTargetSpeed)
        {
            Vector3 InCurrentVelocity = mRigidBody.velocity;
            if (mStateManager._Horizontal != 0 || mStateManager._Vertical != 0)
            {
                mTargetVelocity = (pHorizontal + pVertical).normalized * pTargetSpeed;
                mVelocityChange = 3;
            }
            else
            {
                mVelocityChange = 2;
                mTargetVelocity = Vector3.zero;
            }
            Vector3 InVelocity = Vector3.Lerp(InCurrentVelocity, mTargetVelocity, Time.deltaTime * mVelocityChange);
            mRigidBody.velocity = InVelocity;

            if (mStateManager._ObstacleForward)
            {
                mRigidBody.velocity = Vector3.zero;
            }
        }

        private void HandleRotation_Normal(Vector3 pHorizontal, Vector3 pVertical)
        {
            if (Mathf.Abs (mStateManager._Horizontal) > 0 || Mathf.Abs (mStateManager._Vertical) > 0)
            {
                mStoreDirection = (pHorizontal + pVertical).normalized;
                float InTargetAngle = Mathf.Atan2(mStoreDirection.x, mStoreDirection.z) * Mathf.Rad2Deg;

                if (mStateManager._Run && _DoAngleCheck)
                {
                    if (!mUseDot)
                    {
                        if (Mathf.Abs(mPrevAngle - InTargetAngle) > mDegreesRunThreshold)
                        {
                            mPrevAngle = InTargetAngle;
                            PlayAnimSpecial(AnimSpecials.E_RunToStop, false);
                            return;
                        }
                    }
                    else
                    {
                        float InDot = Vector3.Dot(mPrevDirection, mStateManager._MoveDirection);
                        if (InDot < 0)
                        {
                            mPrevDirection = mStateManager._MoveDirection;
                            PlayAnimSpecial(AnimSpecials.E_RunToStop, false);
                            return;
                        }
                    }
                }
                mPrevDirection = mStateManager._MoveDirection;
                mPrevAngle = InTargetAngle;
                mStoreDirection += transform.position;
                Vector3 InTargetDirection = (mStoreDirection - transform.position).normalized;
                InTargetDirection.y = 0;
                if (InTargetDirection == Vector3.zero)
                {
                    InTargetDirection = transform.forward;
                }
                Quaternion InTargetRotation = Quaternion.LookRotation(InTargetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, InTargetRotation, mVelocityChange * Time.deltaTime);
            }
        }

        public void PlayAnimSpecial (AnimSpecials pAnimSpecial, bool pIsSpecial = true)
        {
            int InType = GAMEVAR.GetAnimSpecialType(pAnimSpecial);
            mStateManager.mAnimator.SetBool(GAMEVAR.special, pIsSpecial);
            mStateManager.mAnimator.SetInteger(GAMEVAR.specialType, InType);
            StartCoroutine(CloseSpecialOnAnim(0.4f));
        }

        private IEnumerator CloseSpecialOnAnim(float t)
        {
            yield return new WaitForSeconds(t);
            mStateManager.mAnimator.SetBool(GAMEVAR.special, false);
        }

        private void HandleAnimations_Normal()
        {
            Vector3 InRelativeDirection = transform.InverseTransformDirection(mStateManager._MoveDirection);
            float InHorizontal = InRelativeDirection.x;
            float InVertical = InRelativeDirection.z;

            if (mStateManager._ObstacleForward)
            {
                InVertical = 0;
            }

            mStateManager.mAnimator.SetFloat(GAMEVAR.vertical, InVertical, 0.2f, Time.deltaTime);
            mStateManager.mAnimator.SetFloat(GAMEVAR.horizontal, InHorizontal, 0.2f, Time.deltaTime);
        }

        private void HandleJump()
        {
            if (mStateManager._OnGround && mStateManager._CanJump)
            {
                if (mStateManager._JumpInput && !mStateManager._Jumping && mStateManager._OnLocomotion &&
                    mStateManager._CurrentState != StateManager.CharStates.E_Hold && mStateManager._CurrentState != StateManager.CharStates.E_OnAir)
                {
                    if (mStateManager._CurrentState == StateManager.CharStates.E_Idle)
                    {
                        mStateManager.mAnimator.SetBool(GAMEVAR.special, true);
                        mStateManager.mAnimator.SetInteger(GAMEVAR.specialType, GAMEVAR.GetAnimSpecialType(AnimSpecials.E_Jump_Idle));
                    }
                    if (mStateManager._CurrentState == StateManager.CharStates.E_Moving)
                    {
                        mStateManager.LegFront();
                        mStateManager._Jumping = true;
                        mStateManager.mAnimator.SetBool(GAMEVAR.special, true);
                        mStateManager.mAnimator.SetInteger(GAMEVAR.specialType, GAMEVAR.GetAnimSpecialType(AnimSpecials.E_Run_Jump));
                        mStateManager._CurrentState = StateManager.CharStates.E_Hold;
                        mStateManager.mAnimator.SetBool(GAMEVAR.onAir, true);
                        mStateManager._CanJump = false;
                    }
                }
            }

            if (mStateManager._Jumping)
            {
                if (mStateManager._OnGround)
                {
                    if (!mApplyJumpForce)
                    {
                        StartCoroutine(AddJumpForce(0));
                        mApplyJumpForce = true;
                    }
                }
                else
                {
                    mStateManager._Jumping = false;
                }
            }
        }

        private IEnumerator AddJumpForce (float pDelay)
        {
            yield return new WaitForSeconds(pDelay);
            mRigidBody.drag = 0;
            Vector3 InVelocity = mRigidBody.velocity;
            Vector3 InForward = transform.forward;
            InVelocity = InForward * 3;
            InVelocity.y = mStateManager._JumpForce;
            mRigidBody.velocity = InVelocity;
            StartCoroutine(CloseJump());
        }

        private IEnumerator CloseJump ()
        {
            yield return new WaitForSeconds(0.3f);
            mStateManager._CurrentState = StateManager.CharStates.E_OnAir;
            mStateManager._Jumping = false;
            mApplyJumpForce = false;
            mStateManager._CanJump = false;
            StartCoroutine(EnableJump());
        }

        private IEnumerator EnableJump ()
        {
            yield return new WaitForSeconds(1.3f);
            mStateManager._CanJump = true;
        }

        private void HandleDrag()
        {
            mRigidBody.drag = (mStateManager._Horizontal != 0 || mStateManager._Vertical != 0 || !mStateManager._OnGround) ? 0 : 4;
        }

        public void AddVelocity(Vector3 pDirection, float pTime, float pForce, bool pClamp)
        {
            mForceOverLife = pTime;
            mOverrideSpeed = pForce;
            mOverrideForce = false;
            mForceOverrideTimer = 0;
            mOverrideDirection = pDirection;
            mRigidBody.velocity = Vector3.zero;
            mStopVelocity = pClamp;
        }

        private void OverrideLogic()
        {
            mRigidBody.drag = 0;
            mRigidBody.velocity = mOverrideDirection * mOverrideSpeed;
            mForceOverrideTimer += Time.deltaTime;
            if (mForceOverrideTimer > mForceOverLife)
            {
                if (mStopVelocity)
                {
                    mRigidBody.velocity = Vector3.zero;
                }
                mStopVelocity = mOverrideForce = false;
            }
        }
    }
}
