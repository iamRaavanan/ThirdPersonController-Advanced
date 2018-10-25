using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class StateManager : MonoBehaviour
    {
        [Header ("Player Model")]
        public GameObject _ModelPrefab;
        public bool _InGame;
        public bool _IsPlayer;

        [Header("Stats")]
        public float _GroundDst;
        public float _GroundOffset;
        public float _DstToCheckForward;
        public float _RunSpeed;
        public float _WalkSpeed;
        public float _JumpForce;
        public float _AirTimeThreshold;

        [Header("User Inputs")]
        public float _Horizontal;
        public float _Vertical;
        public bool _JumpInput;

        [Header("States")]
        public bool _ObstacleForward;
        public bool _GroundForward;
        public float _GroundAngle;

        #region StateRequests
        [Header("State Requests")]
        public CharStates _CurrentState;
        public bool _OnGround;
        public bool _Run;
        public bool _Walk;
        public bool _OnLocomotion;
        public bool _InAngleMoveDirection;
        public bool _Jumping;
        public bool _CanJump;
        #endregion

        #region References
        private GameObject mActiveModel;
        [HideInInspector]
        public Animator mAnimator;
        [HideInInspector]
        public Rigidbody mRigidBody;
        #endregion

        #region Variables
        [HideInInspector]
        public Vector3 _MoveDirection;
        public float _AirTime;
        [HideInInspector]
        public bool _PrevGround;
        #endregion

        private LayerMask mIgnoreLayers;

        public enum CharStates
        {
            E_Idle, E_Moving, E_OnAir, E_Hold
        }

        #region Initialize
        public void Init ()
        {
            _InGame = true;
            CreateModel();
            SetupAnimator();
            AddControlReferences();
            _CanJump = true;

            gameObject.layer = 8;
            mIgnoreLayers = ~(1 << 3 | 1 << 8);
        }

        private void CreateModel()
        {
            mActiveModel = Instantiate(_ModelPrefab) as GameObject;
            mActiveModel.transform.parent = this.transform;
            mActiveModel.transform.localPosition = Vector3.zero;
            mActiveModel.transform.localEulerAngles = Vector3.zero;
            mActiveModel.transform.localScale = Vector3.one;
        }
        
        private void SetupAnimator()
        {
            mAnimator = GetComponent<Animator>();
            Animator InTempAnimator = mActiveModel.GetComponent<Animator>();
            mAnimator.avatar = InTempAnimator.avatar;
            Destroy(InTempAnimator);
        }

        private void AddControlReferences ()
        {
            mRigidBody = gameObject.AddComponent<Rigidbody>();
            mRigidBody.angularDrag = 999;
            mRigidBody.drag = 4;
            mRigidBody.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
        }
        #endregion

        public void FixedTick ()
        {
            _ObstacleForward = _GroundForward = false;
            _OnGround = OnGround();
            if (_OnGround)
            {
                Vector3 InOrigin = transform.position;
                // Is Forward is clear
                InOrigin += Vector3.up * 0.75f;
                IsClear(InOrigin, transform.forward, _DstToCheckForward, ref _ObstacleForward);
                if (!_ObstacleForward)
                {
                    // Is Ground forward is clear
                    InOrigin += transform.forward * 0.6f;
                    IsClear(InOrigin, -Vector3.up, _GroundDst * 3, ref _GroundForward);
                }
                else
                {
                    if (Vector3.Angle (transform.forward, _MoveDirection) > 30)
                    {
                        _ObstacleForward = false;
                    }
                }
            }
            UpdateState();
            MonitorAirTime();
        }

        private void UpdateState()
        {
            if (_CurrentState == CharStates.E_Hold)
                return;
            if (_Horizontal != 0 || _Vertical != 0)
            {
                _CurrentState = CharStates.E_Moving;
            }
            else
            {
                _CurrentState = CharStates.E_Idle;
            }
            if (!_OnGround)
                _CurrentState = CharStates.E_OnAir;
        }

        private void MonitorAirTime()
        {
            if (!_Jumping)
            {
                mAnimator.SetBool(GAMEVAR.onAir, !_OnGround);
            }
            if (_OnGround)
            {
                if (_PrevGround != _OnGround)
                {
                    mAnimator.SetInteger(GAMEVAR.jumpType, (_AirTime > _AirTimeThreshold) ? ((_Horizontal != 0 || _Vertical != 0) ? 2 : 1) : 0);
                }
                _AirTime = 0;
            }
            else
            {
                _AirTime += Time.deltaTime;
            }
            _PrevGround = _OnGround;
        }

        private void IsClear(Vector3 pOrigin, Vector3 pDirection, float pDistance, ref bool pIsHit)
        {
            RaycastHit InHit;
            Debug.DrawRay(pOrigin, pDirection * pDistance, Color.green);
            if (Physics.Raycast (pOrigin, pDirection, out InHit, pDistance, mIgnoreLayers))
            {
                pIsHit = true;
            }
            else
            {
                pIsHit = false;
            }

            if (_ObstacleForward)
            {
                Vector3 InComingVector = InHit.point - pOrigin;
                Vector3 InReflectVector = Vector3.Reflect(InComingVector, InHit.normal);

                float InAngle = Vector3.Angle(InComingVector, InReflectVector);
                if (InAngle < 70)
                {
                    pIsHit = false;
                }
            }

            if (_GroundForward)
            {
                if (_Horizontal != 0 || _Vertical != 0)
                {
                    Vector3 InPoint1 = transform.position;
                    Vector3 InPoint2 = InHit.point;
                    float InDiffY = InPoint1.y - InPoint2.y;
                    _GroundAngle = InDiffY;
                }
            }
            float InTargetInCline = 0;
            if (Mathf.Abs (_GroundAngle) > 0.3f)
            {
                InTargetInCline = (_GroundAngle < 0) ? 1 : -1;
            }
            if (_GroundAngle == 0)
            {
                InTargetInCline = 0;
            }
            mAnimator.SetFloat(GAMEVAR.incline, InTargetInCline, 0.3f, Time.deltaTime);
        }

        private bool OnGround()
        {
            bool InValue = false;
            if (_CurrentState == CharStates.E_Hold)
                return false;
            Vector3 InOrigin = transform.position + (Vector3.up * 0.55f);
            RaycastHit InHit = new RaycastHit();
            bool IsHit = false;
            FindGround(InOrigin, ref InHit, ref IsHit);

            if (IsHit)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector3 InNewOrigin = InOrigin;
                    switch (i)
                    {
                        case 0:
                            InNewOrigin += Vector3.forward / 3;
                            break;
                        case 1:
                            InNewOrigin -= Vector3.forward / 3;
                            break;
                        case 2:
                            InNewOrigin -= Vector3.right / 3;
                            break;
                        case 3:
                            InNewOrigin += Vector3.right / 3;
                            break;
                    }
                    FindGround(InNewOrigin, ref InHit, ref IsHit);
                    if (IsHit)
                        break;
                }
            }
            InValue = IsHit;
            if (InValue)
            {
                Vector3 InTargetPosition = transform.position;
                InTargetPosition.y = InHit.point.y + _GroundOffset;
                transform.position = InTargetPosition;
            }
            return InValue;
        }

        public void LegFront()
        {
            Vector3 InLeftLeg = mAnimator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
            Vector3 InRightLeg = mAnimator.GetBoneTransform(HumanBodyBones.RightFoot).position;
            Vector3 InRelativeLL = transform.InverseTransformPoint(InLeftLeg);
            Vector3 InRelativeRL = transform.InverseTransformPoint(InRightLeg);

            bool IsLeft = InRelativeLL.z > InRelativeRL.z;
            mAnimator.SetBool(GAMEVAR.mirrorJump, IsLeft);
        }

        private void FindGround(Vector3 pOrigin, ref RaycastHit pHit, ref bool pIsHit)
        {
            Debug.DrawRay(pOrigin, -Vector3.up * 0.5f, Color.red);
            if (Physics.Raycast (pOrigin, - Vector3.up, out pHit, _GroundDst, mIgnoreLayers))
            {
                pIsHit = true;
            }
            //return pHit;
        }

        public void UpdateTick ()
        {
            _OnGround = OnGround();
        }
    }
}