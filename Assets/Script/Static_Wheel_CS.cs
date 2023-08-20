using UnityEngine;
using System.Collections;

namespace ChobiAssets.PTM
{

    public class Static_Wheel_CS : MonoBehaviour
    {
        public bool Is_Left;
        public Static_Wheel_Parent_CS Parent_Script;

        Transform thisTransform;
        Vector3 currentAng;


        void Start()
        {
            thisTransform = transform;
        }


        void Update()
        {
            Rotate();
        }


        void Rotate()
        {
            currentAng = thisTransform.localEulerAngles;

            if (Is_Left)
            { // trái
                currentAng.y = Parent_Script.Left_Angle_Y;
            }
            else
            { // Phải
                currentAng.y = Parent_Script.Right_Angle_Y;
            }

            thisTransform.localEulerAngles = currentAng;
        }
    }

}