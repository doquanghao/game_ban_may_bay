using System.Collections;
using UnityEngine;

namespace ChobiAssets.PTM
{

    public class Aiming_Control_Input_01_Mouse_Keyboard_CS : Aiming_Control_Input_00_Base_CS
    {

        protected Gun_Camera_CS gunCameraScript;
        protected int thisRelationship;
        protected Vector3 screenCenter = Vector3.zero;

        public override void Prepare(Aiming_Control_CS aimingScript)
        {
            this.aimingScript = aimingScript;

            // Đặt "Use_Auto_Turn".
            aimingScript.Use_Auto_Turn = true;

            // Lấy "Gun_Camera_CS".
            gunCameraScript = GetComponentInChildren<Gun_Camera_CS>();

            // Đặt mối quan hệ.
            ID_Settings_CS idScript = GetComponentInParent<ID_Settings_CS>();
            if (idScript)
            {
                thisRelationship = idScript.Relationship;
            }

            // Đặt chế độ ngắm ban đầu.
            aimingScript.Mode = 1; // Free aiming.
            aimingScript.Switch_Mode();

            // Đặt vị trí mục tiêu ban đầu.
            aimingScript.Target_Position = transform.position + (transform.forward * 128.0f);
        }

        public override void Get_Input()
        {
            // Chuyển đổi chế độ ngắm.
            if (Input.GetKeyDown(General_Settings_CS.Aim_Mode_Switch_Key))
            {
                if (aimingScript.Mode == 0 || aimingScript.Mode == 2)
                {
                    aimingScript.Mode = 1; // Free aiming.
                }
                else
                {
                    aimingScript.Mode = 0; // Giữ vị trí ban đầu.
                }
                aimingScript.Switch_Mode();
            }

            // Điều chỉnh ngắm.
            if (gunCameraScript && gunCameraScript.Gun_Camera.enabled)
            { // Hiện tại camera súng đã được kích hoạt.

                // Đặt góc điều chỉnh.
                aimingScript.Adjust_Angle.x += Input.GetAxis("Mouse X") * General_Settings_CS.Aiming_Sensibility;
                aimingScript.Adjust_Angle.y += Input.GetAxis("Mouse Y") * General_Settings_CS.Aiming_Sensibility;

                // Kiểm tra hiện đang trong quá trình khóa mục tiêu.
                if (aimingScript.Target_Transform)
                { // Hiện đang khóa mục tiêu.
                  // Hủy khóa mục tiêu.
                    if (Input.GetKeyDown(General_Settings_CS.Turret_Cancel_Key))
                    {
                        aimingScript.Target_Transform = null;
                        aimingScript.Target_Rigidbody = null;
                    }

                    // Điều khiển "reticleAimingFlag" trong "Aiming_Control_CS".
                    aimingScript.reticleAimingFlag = false;
                }
                else
                { // Hiện tại chưa khóa mục tiêu.
                  // Cố gắng tìm mục tiêu mới.
                    if (Input.GetKey(General_Settings_CS.Turret_Cancel_Key) == false)
                    {
                        screenCenter.x = Screen.width * 0.5f;
                        screenCenter.y = Screen.height * 0.5f;
                        aimingScript.Reticle_Aiming(screenCenter, thisRelationship);
                    }

                    // Điều khiển "reticleAimingFlag" trong "Aiming_Control_CS".
                    aimingScript.reticleAimingFlag = true;
                }

                // Đặt lại "Turret_Speed_Multiplier".
                aimingScript.Turret_Speed_Multiplier = 1.0f;
            }
            else
            { // Hiện tại camera súng đã bị tắt.

                // Đặt lại góc điều chỉnh.
                aimingScript.Adjust_Angle = Vector3.zero;

                // Dừng xoay pháo và nòng súng khi nhấn nút hủy bỏ. >> Chỉ có camera quay.
                if (Input.GetKey(General_Settings_CS.Turret_Cancel_Key))
                {
                    aimingScript.Turret_Speed_Multiplier -= 2.0f * Time.deltaTime;
                }
                else
                {
                    aimingScript.Turret_Speed_Multiplier += 2.0f * Time.deltaTime;
                }
                aimingScript.Turret_Speed_Multiplier = Mathf.Clamp01(aimingScript.Turret_Speed_Multiplier);

                // Ngắm tự do.
                if (aimingScript.Mode == 1)
                { // Ngắm tự do.

                    // Tìm mục tiêu.
                    screenCenter.x = Screen.width * 0.5f;
                    screenCenter.y = Screen.height * (0.5f + General_Settings_CS.Aiming_Offset);
                    aimingScript.Cast_Ray_Free(screenCenter);
                }

                // Điều khiển "reticleAimingFlag" trong "Aiming_Control_CS".
                aimingScript.reticleAimingFlag = false;
            }
        }
    }

}
