using System.Collections;
using UnityEngine;
using System.Collections.Generic;


namespace ChobiAssets.PTM
{
    public class Aiming_Control_CS : MonoBehaviour
    {
        /*
        Kịch bản này được gắn vào phần "MainBody" của xe tăng.
        Kịch bản này điều khiển việc nhắm mục tiêu của xe tăng.
        Các kịch bản "Turret_Horizontal_CS" và "Cannon_Vertical_CS" sẽ quay pháo và đại bác dựa vào các biến số trong kịch bản này.
        */
        public float OpenFire_Angle = 180.0f;


        Turret_Horizontal_CS[] turretHorizontalScripts;
        Cannon_Vertical_CS[] cannonVerticalScripts;
        public bool Use_Auto_Turn; // Được tham chiếu từ "Turret_Horizontal_CS" và "Cannon_Vertical_CS".
        public bool Use_Auto_Lead;  // Được tham chiếu từ "Turret_Horizontal_CS".
        public float Aiming_Blur_Multiplier = 1.0f; // Được tham chiếu từ "Turret_Horizontal_CS".
        public bool reticleAimingFlag;// Điều khiển từ "Aiming_Control_Input_##_###", và được tham chiếu từ "UI_Aim_Marker_Control_CS".

        // Cho quay tự động.
        public int Mode; // Được tham chiếu từ "UI_Aim_Marker_Control_CS". // 0 => Giữ vị trí ban đầu, 1 => Nhắm tự do, 2 => Khóa mục tiêu.
        Transform rootTransform;
        Rigidbody thisRigidbody;
        public Vector3 Target_Position; // Được tham chiếu từ "Turret_Horizontal_CS", "Cannon_Vertical_CS", "UI_Aim_Marker_Control_CS", "ReticleWheel_Control_CS".
        public Transform Target_Transform; // Được tham chiếu từ "UI_Aim_Marker_Control_CS", "UI_HP_Bars_Target_CS".
        Vector3 targetOffset;
        public Rigidbody Target_Rigidbody;  // Được tham chiếu từ "Turret_Horizontal_CS".
        public Vector3 Adjust_Angle;// Được tham chiếu từ "Turret_Horizontal_CS" và "Cannon_Vertical_CS".
        const float spherecastRadius = 3.0f;
        public float Turret_Speed_Multiplier; // Được tham chiếu từ "Turret_Horizontal_CS" và "Cannon_Vertical_CS".

        // Cho quay thủ công.
        public float Turret_Turn_Rate; // Được tham chiếu từ "Turret_Horizontal_CS".
        public float Cannon_Turn_Rate; // Được tham chiếu từ "Cannon_Vertical_CS".


        protected Aiming_Control_Input_00_Base_CS inputScript;

        public bool Is_Selected; // Được tham chiếu từ "UI_HP_Bars_Target_CS".


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            rootTransform = transform.root;
            thisRigidbody = GetComponent<Rigidbody>();
            Turret_Speed_Multiplier = 1.0f;

            Use_Auto_Lead = General_Settings_CS.Use_Auto_Lead;

            // Lấy các kịch bản "Turret_Horizontal_CS" và "Cannon_Vertical_CS" trong xe tăng.
            turretHorizontalScripts = GetComponentsInChildren<Turret_Horizontal_CS>();
            cannonVerticalScripts = GetComponentsInChildren<Cannon_Vertical_CS>();

            // Đặt kịch bản đầu vào.
            inputScript = gameObject.AddComponent<Aiming_Control_Input_01_Mouse_Keyboard_CS>();


            // Chuẩn bị kịch bản đầu vào.
            if (inputScript != null)
            {
                inputScript.Prepare(this);
            }
        }





        void Update()
        {
            if (Is_Selected == false)
            {
                return;
            }

            if (inputScript != null)
            {
                inputScript.Get_Input();
            }
        }


        void FixedUpdate()
        {
            // Cập nhật vị trí mục tiêu.
            if (Target_Transform)
            {
                Update_Target_Position();
            }
            else
            {
                Target_Position += thisRigidbody.velocity * Time.fixedDeltaTime;
            }
        }


        void Update_Target_Position()
        {
            // Kiểm tra xem mục tiêu còn tồn tại không.
            if (Target_Transform.root.tag == "Finish")
            { // Mục tiêu đã bị phá hủy.
                Target_Transform = null;
                Target_Rigidbody = null;
                return;
            }

            // Cập nhật vị trí mục tiêu.
            Target_Position = Target_Transform.position + (Target_Transform.forward * targetOffset.z) + (Target_Transform.right * targetOffset.x) + (Target_Transform.up * targetOffset.y);
        }


        public void Switch_Mode()
        { // Được gọi từ cả "Aiming_Control_Input_##_###".
            switch (Mode)
            {
                case 0: // Giữ vị trí ban đầu.
                    Target_Transform = null;
                    Target_Rigidbody = null;
                    for (int i = 0; i < turretHorizontalScripts.Length; i++)
                    {
                        turretHorizontalScripts[i].Stop_Tracking();
                    }
                    for (int i = 0; i < cannonVerticalScripts.Length; i++)
                    {
                        cannonVerticalScripts[i].Stop_Tracking();
                    }
                    break;

                case 1: // Nhắm mục tiêu tự do.
                    Target_Transform = null;
                    Target_Rigidbody = null;
                    for (int i = 0; i < turretHorizontalScripts.Length; i++)
                    {
                        turretHorizontalScripts[i].Start_Tracking();
                    }
                    for (int i = 0; i < cannonVerticalScripts.Length; i++)
                    {
                        cannonVerticalScripts[i].Start_Tracking();
                    }
                    break;

                case 2: // Đang nhắm mục tiêu.
                    for (int i = 0; i < turretHorizontalScripts.Length; i++)
                    {
                        turretHorizontalScripts[i].Start_Tracking();
                    }
                    for (int i = 0; i < cannonVerticalScripts.Length; i++)
                    {
                        cannonVerticalScripts[i].Start_Tracking();
                    }
                    break;
            }
        }


        public void Cast_Ray_Lock(Vector3 screenPos)
        { // Được gọi từ "Aiming_Control_Input_##_###".

            // Tìm mục tiêu bằng cách phát một tia từ camera.
            var mainCamera = Camera.main;
            var ray = mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 2048.0f, Layer_Settings_CS.Aiming_Layer_Mask))
            {
                var colliderTransform = raycastHit.collider.transform;

                // Kiểm tra hit collider không phải là một phần của xe tăng hiện tại.
                if (colliderTransform.root != rootTransform)
                {
                    // Kiểm tra đối tượng hit có một rigidbody.
                    // (Ghi chú.) Khi collider hit không có rigidbody, và parent của nó có rigidbody, thì rigidbody của parent sẽ được đặt là 'RaycastHit.rigidbody'.
                    if (raycastHit.rigidbody)
                    {
                        // Đặt collider hit làm mục tiêu.
                        Target_Transform = colliderTransform;

                        // Đặt offset từ pivot.
                        targetOffset = Target_Transform.InverseTransformPoint(raycastHit.point);
                        if (Target_Transform.localScale != Vector3.one)
                        { // Collider hit phải là "Armor_Collider".
                            targetOffset.x *= Target_Transform.localScale.x;
                            targetOffset.y *= Target_Transform.localScale.y;
                            targetOffset.z *= Target_Transform.localScale.z;
                        }

                        // Lưu trữ rigidbody của mục tiêu.
                        Target_Rigidbody = raycastHit.rigidbody;
                    }
                    else
                    { // Đối tượng hit không có rigidbody.

                        // Xóa mục tiêu.
                        Target_Transform = null;
                        Target_Rigidbody = null;

                        // Lưu trữ điểm hit.
                        Target_Position = raycastHit.point;
                    }

                    // Chuyển chế độ nhắm mục tiêu.
                    Mode = 2; // Đang nhắm mục tiêu.
                    Switch_Mode();
                    return;
                }
                else
                { // Hit collider là một phần của chính nó.

                    // Xóa mục tiêu.
                    Target_Transform = null;
                    Target_Rigidbody = null;

                    // Chuyển chế độ nhắm mục tiêu.
                    Mode = 0; // Giữ vị trí ban đầu.
                    Switch_Mode();
                    return;
                }
            }
            else
            { // Tia không va chạm bất kỳ đối tượng nào.

                // Xóa mục tiêu.
                Target_Transform = null;
                Target_Rigidbody = null;

                // Đặt vị trí qua camera.
                screenPos.z = 1024.0f;
                Target_Position = mainCamera.ScreenToWorldPoint(screenPos);

                // Chuyển chế độ nhắm mục tiêu.
                Mode = 2; // Đang nhắm mục tiêu.
                Switch_Mode();
                return;
            }
        }



        public void Cast_Ray_Free(Vector3 screenPos)
        { // Được gọi từ "Aiming_Control_Input_##_###".

            // Tìm mục tiêu bằng cách phát tia từ camera.
            var mainCamera = Camera.main;
            var ray = mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 2048.0f, Layer_Settings_CS.Aiming_Layer_Mask))
            {
                var colliderTransform = raycastHit.collider.transform;

                // Kiểm tra hit collider không phải là một phần của xe tăng hiện tại.
                if (colliderTransform.root != rootTransform)
                {
                    // Kiểm tra đối tượng hit có một rigidbody.
                    // (Ghi chú.) Khi collider hit không có rigidbody, và parent của nó có rigidbody, thì rigidbody của parent sẽ được đặt là 'RaycastHit.rigidbody'.
                    if (raycastHit.rigidbody)
                    {
                        // Kiểm tra đối tượng hit không phải là một xe tăng đã bị phá hủy.
                        if (colliderTransform.root.tag != "Finish")
                        {
                            // Đặt collider hit làm mục tiêu.
                            Target_Transform = colliderTransform;

                            // Đặt offset từ pivot.
                            targetOffset = Target_Transform.InverseTransformPoint(raycastHit.point);
                            if (Target_Transform.localScale != Vector3.one)
                            { // Collider hit phải là "Armor_Collider".
                                targetOffset.x *= Target_Transform.localScale.x;
                                targetOffset.y *= Target_Transform.localScale.y;
                                targetOffset.z *= Target_Transform.localScale.z;
                            }

                            // Lưu trữ rigidbody của mục tiêu.
                            Target_Rigidbody = raycastHit.rigidbody;

                            // Lưu trữ điểm hit.
                            Target_Position = raycastHit.point;

                            return;
                        }
                    }

                    // Đối tượng hit không có rigidbody, hoặc nó là một xe tăng đã bị phá hủy.

                } // Hit collider là một phần của chính nó.

            } // Tia không va chạm bất kỳ đối tượng nào.

            // Xóa mục tiêu.
            Target_Transform = null;
            Target_Rigidbody = null;

            // Đặt vị trí qua tank hiện tại.
            screenPos.z = 64.0f;
            Target_Position = mainCamera.ScreenToWorldPoint(screenPos);
        }



        public void Reticle_Aiming(Vector3 screenPos, int thisRelationship)
        { // Được gọi từ "Aiming_Control_Input_##_###".

            // Tìm mục tiêu bằng cách phát tia từ camera ra một khối cầu.
            var ray = Camera.main.ScreenPointToRay(screenPos);
            var raycastHits = Physics.SphereCastAll(ray, spherecastRadius, 2048.0f, Layer_Settings_CS.Aiming_Layer_Mask);
            for (int i = 0; i < raycastHits.Length; i++)
            {
                Transform colliderTransform = raycastHits[i].collider.transform;

                // Kiểm tra hit collider không phải là một phần của xe tăng hiện tại.
                if (colliderTransform.root == rootTransform)
                {
                    continue;
                }

                // Kiểm tra đối tượng hit có một rigidbody.
                var targetRigidbody = raycastHits[i].rigidbody;
                if (targetRigidbody == null)
                {
                    continue;
                }

                // Kiểm tra mục tiêu có phải là MainBody.
                if (targetRigidbody.gameObject.layer != Layer_Settings_CS.Body_Layer)
                {
                    continue;
                }

                // Kiểm tra đối tượng hit không phải là một xe tăng đã bị phá hủy.
                if (colliderTransform.root.tag == "Finish")
                {
                    continue;
                }

                // Kiểm tra mối quan hệ.
                var idScript = raycastHits[i].transform.GetComponentInParent<ID_Settings_CS>();
                if (idScript && idScript.Relationship == thisRelationship)
                {
                    continue;
                }

                // Kiểm tra vật cản.
                if (Physics.Linecast(ray.origin, raycastHits[i].point, out RaycastHit raycastHit, Layer_Settings_CS.Aiming_Layer_Mask))
                {
                    // Kiểm tra vật cản không phải là một phần của chính nó.
                    if (raycastHit.transform.root != rootTransform)
                    {
                        continue;
                    }
                }

                // Đặt đối tượng hit làm mục tiêu mới.
                Target_Transform = raycastHits[i].transform;
                targetOffset = Vector3.zero;
                targetOffset.y = 0.5f;
                Target_Rigidbody = targetRigidbody;
                Target_Position = raycastHits[i].point;
                Adjust_Angle = Vector3.zero;
                return;

            } // Không tìm thấy mục tiêu mới.
        }


        void Selected(bool isSelected)
        { // Called from "ID_Settings_CS".
            this.Is_Selected = isSelected;

            if (isSelected == false)
            {
                return;
            } // This tank is selected.

            // Send this reference to the "UI_HP_Bars_Target_CS" in the scene.
            if (UI_HP_Bars_Target_CS.Instance)
            {
                UI_HP_Bars_Target_CS.Instance.Get_Aiming_Script(this);
            }
        }

    }

}
