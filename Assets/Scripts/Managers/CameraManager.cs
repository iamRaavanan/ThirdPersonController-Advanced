using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan.CameraWorks
{
    public class CameraManager : MonoBehaviour
    {
        public bool _HoldCamera;
        public bool _AddDefaultAsNormal;
        public Transform _Target;

        #region Variables
        public string _ActiveStateID;
        public bool _LockCursor;
        [SerializeField]
        private float mMoveSpeed = 5;
        [SerializeField]
        private float mTurnSpeed = 1.5f;
        [SerializeField]
        private float mTurnSpeedController = 5.5f;
        [SerializeField]
        private float mTurnSmoothing = 0.1f;
        [SerializeField]
        private bool mIsController;
        #endregion

        #region References
        [HideInInspector]
        public Transform _PivotT;
        [HideInInspector]
        public Transform _CameraT;
        #endregion

        public static CameraManager _Instance;

        private Vector3 mTargetPosition;
        [HideInInspector]
        public Vector3 _TargetPositionOffset;

        #region Internal Vairables
        private float mX;
        private float mY;
        private float mLookAngle;
        private float mTiltAngle;
        private float mOffsetX;
        private float mOffsetY;
        private float mSmoothX;
        private float mSmoothY;
        private float mSmoothXVelocity;
        private float mSmoothYVelocity;
        #endregion

        [SerializeField]
        private List<CameraState> mCameraStates = new List<CameraState>();
        private CameraState mActiveState;
        private CameraState mDefaultState;

        private void Awake()
        {
            _Instance = this;
        }

        private void Start()
        {
            if (Camera.main.transform == null)
            {
                Debug.Log("There is no Camera with 'MainCamera' tag");
            }
            _CameraT = Camera.main.transform.parent;
            _PivotT = _CameraT.parent;

            CameraState InCameraState = new CameraState();
            InCameraState._Id = "Default";
            InCameraState._MinAngle = 35;
            InCameraState._MaxAngle = 35;
            InCameraState._CameraFOV = Camera.main.fieldOfView;
            InCameraState._CameraZ = _CameraT.localPosition.z;
            InCameraState._PivotPosition = _PivotT.localPosition;
            mDefaultState = InCameraState;

            if (_AddDefaultAsNormal)
            {
                mCameraStates.Add(mDefaultState);
                mDefaultState._Id = "Normal";
            }
            mActiveState = mDefaultState;
            _ActiveStateID = mActiveState._Id;

            FixPositions();

            if (_LockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void FixedUpdate()
        {
            if (_Target)
            {
                mTargetPosition = _Target.position + _TargetPositionOffset;
            }
            CameraFollow();
            if (!_HoldCamera)
            {
                HandleRotation();
            }
            FixPositions();
        }

        private void CameraFollow()
        {
            Vector3 InCameraTarget = Vector3.Lerp(transform.position, mTargetPosition, Time.deltaTime * mMoveSpeed);
            transform.position = InCameraTarget;
        }

        private void HandleRotation()
        {
            HandleOffsets();

            mX = Input.GetAxis("Mouse X") + mOffsetX;
            mY = Input.GetAxis("Mouse Y") + mOffsetY;

            float InTargetTurnSpeed = mTurnSpeed;
            if (mIsController)
            {
                InTargetTurnSpeed = mTurnSpeedController;
            }
            if (mTurnSmoothing > 0)
            {
                mSmoothX = Mathf.SmoothDamp(mSmoothX, mX, ref mSmoothXVelocity, mTurnSmoothing);
                mSmoothY = Mathf.SmoothDamp(mSmoothY, mY, ref mSmoothYVelocity, mTurnSmoothing);
            }
            else
            {
                mSmoothX = mX;
                mSmoothY = mY;
            }
            mLookAngle += mSmoothX * InTargetTurnSpeed;
            if (mLookAngle > 360)
                mLookAngle = 0;
            if (mLookAngle < -360)
                mLookAngle = 0;

            transform.rotation = Quaternion.Euler(0f, mLookAngle, 0f);
            mTiltAngle -= mSmoothY * InTargetTurnSpeed;
            mTiltAngle = Mathf.Clamp(mTiltAngle, -mActiveState._MinAngle, mActiveState._MaxAngle);
            _PivotT.localRotation = Quaternion.Euler(mTiltAngle, 0f, 0f);
        }

        private void HandleOffsets()
        {
            if (mOffsetX != 0)
            {
                mOffsetX = Mathf.MoveTowards(mOffsetX, 0, Time.deltaTime);
            }
            if (mOffsetY != 0)
            {
                mOffsetY = Mathf.MoveTowards(mOffsetY, 0, Time.deltaTime);
            }
        }

        private CameraState GetState (string pId)
        {
            CameraState InCameraState = null;
            int InCount = mCameraStates.Count;
            for (int i = 0; i < InCount; i++)
            {
                if (mCameraStates[i]._Id == pId)
                {
                    InCameraState = mCameraStates[i];
                    break;
                }
            }
            return InCameraState;
        }

        private void ChangeCameraState (string pId)
        {
            if (mActiveState._Id != pId)
            {
                CameraState InCameraState = GetState(pId);
                if (InCameraState != null)
                {
                    mActiveState = InCameraState;
                    _ActiveStateID = mActiveState._Id;  
                }
            }
        }

        private void FixPositions()
        {
            Vector3 InTargetPivotPosition = (mActiveState._UseDefaultPosition) ? mDefaultState._PivotPosition : mActiveState._PivotPosition;
            _PivotT.localPosition = Vector3.Lerp(_PivotT.localPosition, InTargetPivotPosition, Time.deltaTime * 5);

            float InTargetZ = (mActiveState._UseDefaultCameraZ) ? mDefaultState._CameraZ : mActiveState._CameraZ;
            Vector3 InTargetPosition = _CameraT.localPosition;
            InTargetPosition.z = Mathf.Lerp(InTargetPosition.z, InTargetZ, Time.deltaTime * 5);
            _CameraT.localPosition = InTargetPosition;

            float InTargetFOV = (mActiveState._UseDefaultFOV) ? mDefaultState._CameraFOV : mActiveState._CameraFOV;
            InTargetFOV = (InTargetFOV < 1) ? 2 : InTargetFOV;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, InTargetFOV, Time.deltaTime * 5);
        }
    }

    [System.Serializable]
    public class CameraState
    {
        [Header("Name of state")]
        public string                   _Id;
        [Header("Limits")]
        public float                    _MinAngle;
        public float                    _MaxAngle;
        [Header("Pivot Position")]
        public bool                     _UseDefaultPosition;
        public Vector3                  _PivotPosition;
        [Header("Camera Position")]
        public bool                     _UseDefaultCameraZ;
        public float                    _CameraZ;
        [Header("Camera FOV")]
        public bool                     _UseDefaultFOV;
        public float                    _CameraFOV;
    }
}