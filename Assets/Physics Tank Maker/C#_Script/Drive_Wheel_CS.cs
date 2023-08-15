using UnityEngine;
using System.Collections;

namespace ChobiAssets.PTM
{

    public class Drive_Wheel_CS : MonoBehaviour
    {

        public Rigidbody This_Rigidbody;
        public bool Is_Left;
        public Drive_Wheel_Parent_CS Parent_Script;

        void FixedUpdate()
        {
            Control_Rigidbody();
        }


        void Control_Rigidbody()
        {
            // Thiết lập "maxAngularVelocity" của rigidbody.
            This_Rigidbody.maxAngularVelocity = Parent_Script.Max_Angular_Velocity;

            // Thiết lập "angularDrag" của rigidbody và thêm mô-men xoắn vào nó.
            if (Is_Left)
            { // Left
                This_Rigidbody.angularDrag = Parent_Script.Left_Angular_Drag;
                This_Rigidbody.AddRelativeTorque(0.0f, Parent_Script.Left_Torque, 0.0f);
            }
            else
            { // Right
                This_Rigidbody.angularDrag = Parent_Script.Right_Angular_Drag;
                This_Rigidbody.AddRelativeTorque(0.0f, Parent_Script.Right_Torque, 0.0f);
            }
        }
    }

}