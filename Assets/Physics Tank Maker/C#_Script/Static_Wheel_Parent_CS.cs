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

        // lưu trữ tỷ lệ cuộn theo đường bên trái và bên phải.
        float scrollTrackRateL;
        float scrollTrackRateR;


        Static_Track_Parent_CS staticTrackScript;
        Track_Scroll_CS leftScrollTrackScript;
        Track_Scroll_CS rightScrollTrackScript;
        MainBody_Setting_CS bodyScript;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            bodyScript = GetComponentInParent<MainBody_Setting_CS>();
        }


        public void Prepare_With_Static_Track(Static_Track_Parent_CS tempStaticTrackScript)
        { // Được gọi từ "Static_Track_Parent_CS".
            staticTrackScript = tempStaticTrackScript;

            // Thiết lập tỷ lệ xoay.
            leftStaticTrackRate = staticTrackScript.Reference_Radius_L / Wheel_Radius;
            rightStaticTrackRate = staticTrackScript.Reference_Radius_R / Wheel_Radius;
        }


        public void Prepare_With_Scroll_Track(Track_Scroll_CS tempScrollTrackScript)
        { // Được gọi từ "Track_Scroll_CS".
            if (tempScrollTrackScript.Is_Left)
            { // Trái 
                leftScrollTrackScript = tempScrollTrackScript;

                // Thiết lập tỷ lệ xoay.
                float referenceRadius = leftScrollTrackScript.Reference_Wheel.GetComponent<MeshFilter>().sharedMesh.bounds.extents.x;
                scrollTrackRateL = referenceRadius / Wheel_Radius;
            }
            else
            { // Phải
                rightScrollTrackScript = tempScrollTrackScript;

                // Thiết lập tỷ lệ xoay.
                float referenceRadius = rightScrollTrackScript.Reference_Wheel.GetComponent<MeshFilter>().sharedMesh.bounds.extents.x;
                scrollTrackRateR = referenceRadius / Wheel_Radius;
            }
        }


        void Update()
        {

            if (staticTrackScript && staticTrackScript.isActiveAndEnabled)
            { //Bộ lộn tĩnh được bật và tắt bởi "Track_LOD_Control_CS" trong thời gian chạy.
                Work_With_Static_Track();
            }
            else
            {
                if (leftScrollTrackScript)
                {
                    Work_With_Scroll_Track_Left();
                }
                if (rightScrollTrackScript)
                {
                    Work_With_Scroll_Track_Right();
                }
            }
        }


        void Work_With_Static_Track()
        {
            Left_Angle_Y -= staticTrackScript.Delta_Ang_L * leftStaticTrackRate;
            Right_Angle_Y -= staticTrackScript.Delta_Ang_R * rightStaticTrackRate;
        }


        void Work_With_Scroll_Track_Left()
        {
            Left_Angle_Y -= leftScrollTrackScript.Delta_Ang * scrollTrackRateL;
        }


        void Work_With_Scroll_Track_Right()
        {
            Right_Angle_Y -= rightScrollTrackScript.Delta_Ang * scrollTrackRateR;
        }

    }

}