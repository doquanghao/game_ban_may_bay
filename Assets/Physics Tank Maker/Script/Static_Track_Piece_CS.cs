using UnityEngine;
using System.Collections.Generic;
using System.Collections;


namespace ChobiAssets.PTM
{
    //Script này được gắn vào các phần của băng tải tĩnh (Static_Track).
    // Nhiệm vụ của script là điều khiển vị trí và quay của các phần của băng tải tĩnh. Script này hoạt động cùng với script "Static_Track_Parent_CS" trong đối tượng cha.
    public class Static_Track_Piece_CS : MonoBehaviour
    {


        public Transform This_Transform;// Transform của phần băng tải tĩnh.
        public Static_Track_Parent_CS Parent_Script;// Script "Static_Track_Parent_CS" liên quan.
        public int Type;  // 0=Static, 1=Anchor, 2=Dynamic.
        public Transform Front_Transform;// Transform của phần băng tải phía trước.
        public Transform Rear_Transform;// Transform của phần băng tải phía sau.
        public Static_Track_Piece_CS Front_Script; // Script "Static_Track_Piece_CS" của phần băng tải phía trước.
        public Static_Track_Piece_CS Rear_Script;// Script "Static_Track_Piece_CS" của phần băng tải phía sau.
        public string Anchor_Name;// Tên của phần neo (anchor) liên kết.
        public string Anchor_Parent_Name;// Tên của đối tượng cha của phần neo (anchor).
        public Transform Anchor_Transform;// Transform của phần neo (anchor).
        public bool Simple_Flag;// Cờ 
        public bool Is_Left;// Bên trái hay bên phải.
        public float Invert_Angle; // Góc đảo ngược cho các phần ở vị trí cao hơn.
        public float Half_Length;// Chiều dài nửa của phần.
        public int Pieces_Count;// Số lượng phần.


        Vector3 invisiblePos;// Vị trí ẩn.
        float invisibleAngY; // Góc Y ẩn.
        float initialPosX;// Chỉ dùng cho loại neo (anchor).
        float anchorInitialPosX; // Chỉ dùng cho loại neo (anchor).
        Vector3 currentAngles = new Vector3(0.0f, 0.0f, 270.0f);// Góc hiện tại.

        void Start()
        {
            if (Type == 1)
            {
                Find_Anchor();
                Basic_Settings();
            }
            else
            {
                Basic_Settings();
            }
        }


        void Basic_Settings()
        {
            // Đặt vị trí và góc ban đầu.
            invisiblePos = This_Transform.localPosition;
            invisibleAngY = This_Transform.localEulerAngles.y;
        }

        void Find_Anchor()
        {
            if (Anchor_Transform == null)
            {
                // Tham chiếu đến "Anchor_Transform" có thể đã bị mất khi chỉnh sửa.
                // Lấy "Anchor_Transform" thông qua tên tham chiếu.
                if (!string.IsNullOrEmpty(Anchor_Name) && !string.IsNullOrEmpty(Anchor_Parent_Name))
                {
                    Anchor_Transform = This_Transform.parent.parent.Find(Anchor_Parent_Name + "/" + Anchor_Name);
                }
            }

            // Đặt giá trị ban đầu cho chiều cao. (Trục X = chiều cao)
            if (Anchor_Transform)
            {
                initialPosX = This_Transform.localPosition.x;
                anchorInitialPosX = Anchor_Transform.localPosition.x;
            }
            else
            {
                Type = 2;
            }
        }


        void LateUpdate()
        {
            // Kiểm tra xem xe tăng có hiển thị trước bất kỳ máy ảnh nào hay không. 
            if (Parent_Script.Is_Visible)
            {
                switch (Type)
                {
                    case 0: // Static.
                        Slide_Control();
                        break;

                    case 1: // Anchor.
                        Anchor_Control();
                        Slide_Control();
                        break;

                    case 2: // Dynamic.
                        Dynamic_Control();
                        Slide_Control();
                        break;
                }
            }
        }


        void Slide_Control()
        {
            if (Is_Left)
            { // Trái
              // Tính toán vị trí mới sử dụng hàm Lerp để lấy một giá trị nằm giữa 'invisiblePos' và 'Rear_Script.invisiblePos'.
                This_Transform.localPosition = Vector3.Lerp(invisiblePos, Rear_Script.invisiblePos, Parent_Script.Rate_L);

                // Tính toán góc quay mới (quay quanh trục Y) sử dụng hàm LerpAngle để lấy một góc nằm giữa 'invisibleAngY' và 'Rear_Script.invisibleAngY'.
                currentAngles.y = Mathf.LerpAngle(invisibleAngY, Rear_Script.invisibleAngY, Parent_Script.Rate_L);

                // Đặt góc quay của phần hiện tại theo góc quay mới tính toán.
                This_Transform.localRotation = Quaternion.Euler(currentAngles);
            }
            else
            { // Phải
              // Tương tự như trên nhưng áp dụng cho phần bên phải.
                This_Transform.localPosition = Vector3.Lerp(invisiblePos, Rear_Script.invisiblePos, Parent_Script.Rate_R);
                currentAngles.y = Mathf.LerpAngle(invisibleAngY, Rear_Script.invisibleAngY, Parent_Script.Rate_R);
                This_Transform.localRotation = Quaternion.Euler(currentAngles);
            }
        }



        void Dynamic_Control()
        {
            if (Simple_Flag)
            {
                invisiblePos = Vector3.Lerp(Rear_Script.invisiblePos, Front_Script.invisiblePos, 0.5f);
                invisibleAngY = Mathf.LerpAngle(Rear_Script.invisibleAngY, Front_Script.invisibleAngY, 0.5f);
            }
            else
            {
                // Tính các vị trí cuối.
                var tempRad = Rear_Script.invisibleAngY * Mathf.Deg2Rad;
                var rearEndPos = Rear_Script.invisiblePos + new Vector3(Half_Length * Mathf.Sin(tempRad), 0.0f, Half_Length * Mathf.Cos(tempRad));
                tempRad = Front_Script.invisibleAngY * Mathf.Deg2Rad;
                var frontEndPos = Front_Script.invisiblePos - new Vector3(Half_Length * Mathf.Sin(tempRad), 0.0f, Half_Length * Mathf.Cos(tempRad));

                // Đặt vị trí và góc.
                invisiblePos = Vector3.Lerp(rearEndPos, frontEndPos, 0.5f);
                invisibleAngY = Mathf.Rad2Deg * Mathf.Atan((frontEndPos.x - rearEndPos.x) / (frontEndPos.z - rearEndPos.z)) + Invert_Angle;
            }
        }


        void Anchor_Control()
        {
            // Đặt vị trí. (Trục X = chiều cao)
            invisiblePos.x = initialPosX + (Anchor_Transform.localPosition.x - anchorInitialPosX);

            if (Simple_Flag)
            {
                return;
            }

            // Tính các vị trí cuối.
            var tempRad = Rear_Script.invisibleAngY * Mathf.Deg2Rad;
            var rearEndPos = Rear_Script.invisiblePos + new Vector3(Half_Length * Mathf.Sin(tempRad), 0.0f, Half_Length * Mathf.Cos(tempRad));
            tempRad = Front_Script.invisibleAngY * Mathf.Deg2Rad;
            var frontEndPos = Front_Script.invisiblePos - new Vector3(Half_Length * Mathf.Sin(tempRad), 0.0f, Half_Length * Mathf.Cos(tempRad));

            // Đặt góc.
            invisibleAngY = Mathf.Rad2Deg * Mathf.Atan((frontEndPos.x - rearEndPos.x) / (frontEndPos.z - rearEndPos.z)) + Invert_Angle;
        }
    }
}