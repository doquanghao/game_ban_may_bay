using System.Collections;
using UnityEngine;

namespace ChobiAssets.PTM
{
    public class Camera_Points_Manager_CS : MonoBehaviour
    {

        // Các tùy chọn
        public Camera Main_Camera;// Tham chiếu đến camera chính.
        public Transform Camera_Point;// Điểm camera.

        Transform thisTransform;
        Transform cameraTransform;
        Vector3 cameraTPVLocalPos;

        public bool Is_Selected;

        public float Horizontal_Input;//Đầu vào theo chiều ngang
        public float Vertical_Input;//Đầu vào theo chiều dọc

        Vector3 targetAngles;
        Vector3 currentAngles;
        protected float multiplier;

        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            thisTransform = transform;
            targetAngles = thisTransform.eulerAngles;

            cameraTransform = Main_Camera.transform;
            cameraTPVLocalPos = cameraTransform.localPosition;

            cameraTransform.localPosition = cameraTPVLocalPos;
            targetAngles.x = 0.0f;
            targetAngles.y = thisTransform.eulerAngles.y;
            currentAngles = targetAngles;
        }

        void Update()
        {
            multiplier = Mathf.Lerp(0.1f, 2.0f, Main_Camera.fieldOfView / 15.0f);

            Horizontal_Input = Input.GetAxis("Mouse X") * multiplier;
            Vertical_Input = Input.GetAxis("Mouse Y") * multiplier;
            if (General_Settings_CS.Camera_Invert_Flag)
            {
                Vertical_Input = -Vertical_Input;
            }

            // Điều chỉnh tốc độ đầu vào dựa trên fps hiện tại.
            if (Time.deltaTime != 0.0f)
            {
                // Đôi khi thời gian delta sẽ bằng 0 trong khi chuyển đổi cảnh.
                var currentDelta = Time.fixedDeltaTime / Time.deltaTime;
                Horizontal_Input *= currentDelta;
                Vertical_Input *= currentDelta;
            }
        }

        void LateUpdate()
        {
            if (Is_Selected == false || Main_Camera.enabled == false)
            {
                return;
            }

            // Đặt vị trí cho điểm camera hiện tại.
            thisTransform.position = Camera_Point.position;

            Rotate_TPV();
        }

        Vector3 currentRotationVelocity;
        void Rotate_TPV()
        {
            // Đặt góc xoay mục tiêu.
            targetAngles.y += Horizontal_Input * General_Settings_CS.Camera_Horizontal_Speed;
            targetAngles.z -= Vertical_Input * General_Settings_CS.Camera_Vertical_Speed;

            // Kẹp góc xoay.
            if (General_Settings_CS.Camera_Use_Clamp)
            {
                targetAngles.z = Mathf.Clamp(targetAngles.z, -10.0f, 90.0f);
            }

            // Xoay mượt.
            currentAngles.y = Mathf.SmoothDampAngle(currentAngles.y, targetAngles.y, ref currentRotationVelocity.y, 2.0f * Time.fixedDeltaTime);
            currentAngles.z = Mathf.SmoothDampAngle(currentAngles.z, targetAngles.z, ref currentRotationVelocity.z, 2.0f * Time.fixedDeltaTime);
            thisTransform.eulerAngles = currentAngles;
        }

        void Enable_Camera()
        {
            Main_Camera.enabled = true;
        }


        void Disable_Camera()
        {
            Main_Camera.enabled = false;
        }

        void Selected(bool isSelected)
        {
            // Được gọi từ "ID_Settings_CS".
            this.Is_Selected = isSelected;

            // Bật / tắt camera chính.
            if (isSelected)
            {
                Enable_Camera();
            }
            else
            {
                Disable_Camera();
            }
        }
    }

}
