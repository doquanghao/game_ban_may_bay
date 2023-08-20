using UnityEngine;
using System.Collections;

namespace ChobiAssets.PTM
{
    /*
     * Đoạn mã này điều khiển vị trí và góc xoay của các mảnh đường ray tĩnh.
     * Đoạn mã này hoạt động kết hợp với "Static_Track_Piece_CS" trong các mảnh đường ray.
    */

    public class Static_Track_Parent_CS : MonoBehaviour
    {

        // Các tùy chọn của người dùng 
        public Transform Reference_L;// Tham chiếu bánh trái
        public Transform Reference_R;// Tham chiếu bánh phải
        public string Reference_Name_L;// Tên của tham chiếu bánh trái
        public string Reference_Name_R;// Tên của tham chiếu bánh phải
        public string Reference_Parent_Name_L;// Tên của đối tượng cha chứa tham chiếu bánh trái
        public string Reference_Parent_Name_R;// Tên của đối tượng cha chứa tham chiếu bánh phải
        public float Length;// Độ dài
        public float Radius_Offset;// Độ lệch bán kính
        public float Mass = 30.0f;// Khối lượng
        public float RoadWheel_Effective_Range = 0.4f;// Phạm vi hiệu quả của bánh xe đường bộ
        public float SwingBall_Effective_Range = 0.15f;// Phạm vi hiệu quả của quả cầu xoay
        public float Anti_Stroboscopic_Min = 0.125f;// Giá trị nhỏ nhất của hiệu ứng chớp sáng ngược
        public float Anti_Stroboscopic_Max = 0.375f;// Giá trị lớn nhất của hiệu ứng chớp sáng ngược

        // Dành cho tạo kịch bản biên tập.
        public bool Has_Changed;

        // Được thiết lập bởi "Create_TrackBelt_CSEditor".
        public float Stored_Body_Mass;

        // Tham chiếu từ "Static_Wheel_Parent_CS".
        public float Reference_Radius_L;
        public float Reference_Radius_R;
        public float Delta_Ang_L;
        public float Delta_Ang_R;

        // Tham chiếu từ "Static_Track_Piece_CS".
        public float Rate_L;
        public float Rate_R;
        public bool Is_Visible;

        // Tham chiếu từ "Static_Track_Switch_Mesh_CS".
        public bool Switch_Mesh_L;
        public bool Switch_Mesh_R;

        float leftPreviousAng;
        float rightPreviousAng;
        float leftAngRate;
        float rightAngRate;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            var bodyTransform = transform.parent;

            // Tìm các bánh xe tham chiếu.
            if (Reference_L == null)
            {
                // Bánh xe tham chiếu trái bị mất sau khi chỉnh sửa.
                if (string.IsNullOrEmpty(Reference_Name_L) == false && string.IsNullOrEmpty(Reference_Parent_Name_L) == false)
                {
                    Reference_L = bodyTransform.Find(Reference_Parent_Name_L + "/" + Reference_Name_L);
                }
            }
            if (Reference_R == null)
            {
                // Bánh xe tham chiếu phải bị mất sau khi chỉnh sửa.
                if (string.IsNullOrEmpty(Reference_Name_R) == false && string.IsNullOrEmpty(Reference_Parent_Name_R) == false)
                {
                    Reference_R = bodyTransform.Find(Reference_Parent_Name_R + "/" + Reference_Name_R);
                }
            }

            // Thiết lập "Reference_Radius_#" cho "Static_Wheel_Parent_CS" và thiết lập tỷ lệ góc xoay để điều khiển tốc độ.
            if (Reference_L && Reference_R)
            {
                Reference_Radius_L = Reference_L.GetComponent<MeshFilter>().sharedMesh.bounds.extents.x + Radius_Offset;
                leftAngRate = 360.0f / ((2.0f * Mathf.PI * Reference_Radius_L) / Length);
                Reference_Radius_R = Reference_R.GetComponent<MeshFilter>().sharedMesh.bounds.extents.x + Radius_Offset;
                rightAngRate = 360.0f / ((2.0f * Mathf.PI * Reference_Radius_R) / Length);
            }
            else
            {
                this.enabled = false;
                return;
            }

            // Gửi tham chiếu này cho tất cả các đối tượng "Static_Wheel_Parent_CS" trong xe tăng.
            var staticWheelParentScripts = bodyTransform.GetComponentsInChildren<Static_Wheel_Parent_CS>();
            for (int i = 0; i < staticWheelParentScripts.Length; i++)
            {
                staticWheelParentScripts[i].Prepare_With_Static_Track(this);
            }
        }

        void Update()
        {
            // Check the tank is visible by any camera.
            Is_Visible = true;

            if (Is_Visible)
            {
                Speed_Control();
            }
        }


        void Speed_Control()
        {
            // Bên trái
            var currentAng = Reference_L.localEulerAngles.y;
            Delta_Ang_L = Mathf.DeltaAngle(currentAng, leftPreviousAng);
            var tempClampRate = Random.Range(Anti_Stroboscopic_Min, Anti_Stroboscopic_Max);
            Delta_Ang_L = Mathf.Clamp(Delta_Ang_L, -leftAngRate * tempClampRate, leftAngRate * tempClampRate); // Anti Stroboscopic Effect.
            Rate_L += Delta_Ang_L / leftAngRate;
            if (Rate_L > 1.0f)
            {
                Rate_L %= 1.0f;
                Switch_Mesh_L = !Switch_Mesh_L;
            }
            else if (Rate_L < 0.0f)
            {
                Rate_L = 1.0f + (Rate_L % 1.0f);
                Switch_Mesh_L = !Switch_Mesh_L;
            }
            leftPreviousAng = currentAng;

            // Bên phải
            currentAng = Reference_R.localEulerAngles.y;
            Delta_Ang_R = Mathf.DeltaAngle(currentAng, rightPreviousAng);
            Delta_Ang_R = Mathf.Clamp(Delta_Ang_R, -rightAngRate * tempClampRate, rightAngRate * tempClampRate); // Anti Stroboscopic Effect.
            Rate_R += Delta_Ang_R / rightAngRate;
            if (Rate_R > 1.0f)
            {
                Rate_R %= 1.0f;
                Switch_Mesh_R = !Switch_Mesh_R;
            }
            else if (Rate_R < 0.0f)
            {
                Rate_R = 1.0f + (Rate_R % 1.0f);
                Switch_Mesh_R = !Switch_Mesh_R;
            }
            rightPreviousAng = currentAng;
        }
    }

}