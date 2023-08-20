using UnityEngine;
using System.Collections;

namespace ChobiAssets.PTM
{

    public class Drive_Wheel_Parent_CS : MonoBehaviour
    {
        public bool Drive_Flag = true;//kiểm soát trạng thái của khả năng di chuyển (lái) của đối tượng. Giá trị true cho phép di chuyển, false ngăn cản di chuyển.
        public float Radius = 0.3f;//bán kính của bánh lái, có giá trị mặc định là 0.3.
        public bool Use_BrakeTurn = true;//quyết định liệu có sử dụng chức năng phanh để xoay đối tượng hay không. Giá trị true cho phép sử dụng, false ngăn cản sử dụng.

        public float Max_Angular_Velocity;//vận tốc góc tối đa của đối tượng.

        //biến lưu trữ hệ số lực cản góc cho bánh lái bên trái và bên phải.
        public float Left_Angular_Drag;
        public float Right_Angular_Drag;
        //biến lưu trữ mô-men xoắn cho bánh lái bên trái và bên phải.
        public float Left_Torque;
        public float Right_Torque;

        //lưu trữ giá trị vận tốc góc tối đa.
        float maxAngVelocity;

        //tham chiếu tới một script Drive_Control_CS, để tương tác và điều khiển chức năng lái của đối tượng.
        Drive_Control_CS controlScript;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            // Lấy tham chiếu tới script "Drive_Control_CS".
            controlScript = GetComponentInParent<Drive_Control_CS>();

            // Thiết lập giá trị cho biến "maxAngVelocity".
            maxAngVelocity = Mathf.Deg2Rad * ((controlScript.Max_Speed / (2.0f * Radius * Mathf.PI)) * 360.0f);
        }


        void Update()
        {
            //kiểm soát vận tốc góc và mô-men xoắn của đối tượng, theo như logic và các giá trị.
            Control_Velocity_And_Torque();
        }


        void Control_Velocity_And_Torque()
        {
           // Thiết lập giá trị cho biến "Max_Angular_Velocity".
            Max_Angular_Velocity = controlScript.Speed_Rate * maxAngVelocity;

            // Thiết lập lực cản phanh.
            if (Use_BrakeTurn)
            {
                Left_Angular_Drag = controlScript.L_Brake_Drag;
                Right_Angular_Drag = controlScript.R_Brake_Drag;
            }
            else
            {
                Left_Angular_Drag = 0.0f;
                Right_Angular_Drag = 0.0f;
            }

            // Thiết lập mô-men xoắn.
            if (Drive_Flag == true)
            {
                Left_Torque = controlScript.Left_Torque;
                Right_Torque = controlScript.Right_Torque;
            }
            else
            {
                Left_Torque = 0.0f;
                Right_Torque = 0.0f;
            }
        }
    }

}