using UnityEngine;
using System.Collections;

namespace ChobiAssets.PTM
{

    public class Static_Wheel_Parent_CS : MonoBehaviour
    {

        public float Wheel_Radius;// lưu trữ bán kính của bánh xe.

        // lưu trữ góc xoay.
        public float Left_Angle_Y;
        public float Right_Angle_Y;
        public bool Is_Visible;

        // lưu trữ tỷ lệ độ bám đường tĩnh bên trái và bên phải.
        float leftStaticTrackRate;
        float rightStaticTrackRate;


        Static_Track_Parent_CS staticTrackScript;
   
        public void Prepare_With_Static_Track(Static_Track_Parent_CS tempStaticTrackScript)
        { // Được gọi từ "Static_Track_Parent_CS".
            staticTrackScript = tempStaticTrackScript;

            // Thiết lập tỷ lệ xoay.
            leftStaticTrackRate = staticTrackScript.Reference_Radius_L / Wheel_Radius;
            rightStaticTrackRate = staticTrackScript.Reference_Radius_R / Wheel_Radius;
        }



        void Update()
        {

            if (staticTrackScript && staticTrackScript.isActiveAndEnabled)
            { //Bộ lộn tĩnh được bật và tắt bởi "Track_LOD_Control_CS" trong thời gian chạy.
                Work_With_Static_Track();
            }
        }


        void Work_With_Static_Track()
        {
            Left_Angle_Y -= staticTrackScript.Delta_Ang_L * leftStaticTrackRate;
            Right_Angle_Y -= staticTrackScript.Delta_Ang_R * rightStaticTrackRate;
        }

       
    }

}