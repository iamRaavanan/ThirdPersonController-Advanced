using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public static class GAMEVAR
    {
        #region Animator Hash
        public static string horizontal = "horizontal";
        public static string vertical = "vertical";
        public static string special = "special";
        public static string specialType = "specialType";
        public static string onLocomotion = "onLocomotion";
        public static string Horizontal = "Horizontal";
        public static string Vertical = "Vertical";
        public static string jumpType = "jumpType";
        public static string Jump = "Jump";
        public static string onAir = "onAir";
        public static string mirrorJump = "mirronJump";
        public static string incline = "incline";
        public static string Fire3 = "Fire3";
        #endregion

        #region Functions
        public static int GetAnimSpecialType (AnimSpecials pSpecialAnim)
        {
            int InID = 0;
            switch (pSpecialAnim)
            {
                case AnimSpecials.E_RunToStop:
                    InID = 11;
                    break;
                case AnimSpecials.E_Run:
                    InID = 10;
                    break;
                case AnimSpecials.E_Jump_Idle:
                    InID = 21;
                    break;
                case AnimSpecials.E_Run_Jump:
                    InID = 22;
                    break;
                default:
                    break;
            }
            return InID;
        }
        #endregion
    }

    public enum AnimSpecials
    {
        E_Run, E_RunToStop, E_Jump_Idle, E_Run_Jump
    }
}