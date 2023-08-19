using UnityEngine;
using System.Collections;

namespace ChobiAssets.PTM
{
    /* 
     * Kịch bản này được gắn vào "Cannon_Base" trên chiếc xe tăng.
     * Kịch bản này điều khiển việc xoay đạn pháo theo chiều dọc.
     * Kịch bản này hoạt động kết hợp với "Aiming_Control_CS" trong MainBody.
    */
    public class Cannon_Vertical_CS : MonoBehaviour
    {
        // Tùy chọn của người dùng 
        public float Max_Elevation = 13.0f; // Góc nâng tối đa.
        public float Max_Depression = 7.0f;  // Góc hạ tối đa.
        public float Speed_Mag = 5.0f;  // Tốc độ xoay.
        public float Acceleration_Time = 0.1f; // Thời gian gia tốc.
        public float Deceleration_Time = 0.1f;// Thời gian giảm tốc.
        public bool Upper_Course = false;   // Chọn hướng xoay.
        public Bullet_Generator_CS Bullet_Generator_Script;// Kịch bản tạo viên đạn.


        Transform thisTransform;
        Transform turretBaseTransform;
        Aiming_Control_CS aimingScript;
        bool isTurning;
        bool isTracking;
        float angleX;
        Vector3 currentLocalAngles;
        public float Turn_Rate; // Được tham chiếu từ "Sound_Control_Motor_CS".
        float previousTurnRate;
        float bulletVelocity;
        public bool Is_Ready;  // Được tham chiếu từ "Cannon_Fire".


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            // Hàm này phải được gọi trong Start() sau khi thay đổi cấu trúc hệ thống.
            thisTransform = transform;
            turretBaseTransform = thisTransform.parent;
            aimingScript = GetComponentInParent<Aiming_Control_CS>();
            currentLocalAngles = thisTransform.localEulerAngles;
            angleX = currentLocalAngles.x;
            Max_Elevation = angleX - Max_Elevation;
            Max_Depression = angleX + Max_Depression;

            // Lấy kịch bản "Bullet_Generator_CS".
            if (Bullet_Generator_Script == null)
            {
                Bullet_Generator_Script = GetComponentInChildren<Bullet_Generator_CS>();
            }
        }


        public void Start_Tracking()
        { // Called from "Aiming_Control_CS".
            isTracking = true;
            isTurning = true;
        }


        public void Stop_Tracking()
        { // Called from "Aiming_Control_CS".
            isTracking = false;
        }


        void FixedUpdate()
        {
            Auto_Turn();
        }


        void Auto_Turn()
        {
            // Kiểm tra xem liệu có cần xoay đạn pháo không .
            if (isTurning == false)
            {
                return;
            }

            // Tính góc mục tiêu.
            float targetAngle;
            if (isTracking)
            {
                // Theo dõi mục tiêu.
                // Tính góc mục tiêu.
                targetAngle = Auto_Elevation_Angle();
                targetAngle += Mathf.DeltaAngle(0.0f, angleX) + aimingScript.Adjust_Angle.y;
            }
            else
            {
                // Không theo dõi. >> Quay lại góc ban đầu.
                targetAngle = -Mathf.DeltaAngle(angleX, 0.0f);
                if (Mathf.Abs(targetAngle) < 0.01f)
                {
                    isTurning = false;
                }
            }

            // Tính tỷ lệ quay vòng.
            float sign = Mathf.Sign(targetAngle);
            targetAngle = Mathf.Abs(targetAngle);
            float currentSlowdownAng = Mathf.Abs(Speed_Mag * previousTurnRate) * Deceleration_Time;
            float targetTurnRate = -Mathf.Lerp(0.0f, 1.0f, targetAngle / (Speed_Mag * Time.fixedDeltaTime + currentSlowdownAng)) * sign;
            if (targetAngle > currentSlowdownAng)
            {
                Turn_Rate = Mathf.MoveTowards(Turn_Rate, targetTurnRate, Time.fixedDeltaTime / Acceleration_Time);
            }
            else
            {
                Turn_Rate = Mathf.MoveTowards(Turn_Rate, targetTurnRate, Time.fixedDeltaTime / Deceleration_Time);
            }
            angleX += Speed_Mag * Turn_Rate * Time.fixedDeltaTime * aimingScript.Turret_Speed_Multiplier;
            previousTurnRate = Turn_Rate;

            // Xoay
            angleX = Mathf.Clamp(angleX, Max_Elevation, Max_Depression);
            currentLocalAngles.x = angleX;
            thisTransform.localEulerAngles = currentLocalAngles;

            // Đặt "Is_Ready".
            if (targetAngle <= aimingScript.OpenFire_Angle)
            {
                Is_Ready = true; // Tham khảo từ "Cannon_Fire_CS".
            }
            else
            {
                Is_Ready = false; // Tham khảo từ "Cannon_Fire_CS".
            }
        }


        float Auto_Elevation_Angle()
        {
            // Tính góc thích hợp.
            float properAngle;
            // Lấy tọa độ x và z của mục tiêu (Target_Position) từ script aimingScript và lưu vào biến targetPos2D .
            Vector2 targetPos2D;
            targetPos2D.x = aimingScript.Target_Position.x;
            targetPos2D.y = aimingScript.Target_Position.z;
            //Lấy tọa độ x và z của đối tượng hiện tại (đạn pháo) từ thành phần thisTransform và lưu vào biến thisPos2D.
            Vector2 thisPos2D;
            thisPos2D.x = thisTransform.position.x;
            thisPos2D.y = thisTransform.position.z;

            // Tính toán khoảng cách theo phương x và khoảng cách theo phương y (chiều cao) giữa mục tiêu và đạn pháo.
            // Khoảng cách theo phương x được tính bằng hàm Vector2.Distance(targetPos2D, thisPos2D),
            // còn khoảng cách theo phương y được tính bằng hiệu giữa chiều cao của mục tiêu (aimingScript.Target_Position.y) và chiều cao của đạn pháo (thisTransform.position.y).
            Vector2 dist;
            dist.x = Vector2.Distance(targetPos2D, thisPos2D);
            dist.y = aimingScript.Target_Position.y - thisTransform.position.y;


            if (Bullet_Generator_Script)
            {
                bulletVelocity = Bullet_Generator_Script.Current_Bullet_Velocity;
            }


            float posBase = (Physics.gravity.y * Mathf.Pow(dist.x, 2.0f)) / (2.0f * Mathf.Pow(bulletVelocity, 2.0f));
            float posX = dist.x / posBase;
            float posY = (Mathf.Pow(posX, 2.0f) / 4.0f) - ((posBase - dist.y) / posBase);
            if (posY >= 0.0f)
            {
                if (Upper_Course)
                {
                    properAngle = Mathf.Rad2Deg * Mathf.Atan(-posX / 2.0f + Mathf.Pow(posY, 0.5f));
                }
                else
                {
                    properAngle = Mathf.Rad2Deg * Mathf.Atan(-posX / 2.0f - Mathf.Pow(posY, 0.5f));
                }
            }
            else
            {
                // Viên đạn không đến được mục tiêu.
                properAngle = 45.0f;
            }

            // Thêm góc nghiêng của bể.
            Vector3 forwardPos = turretBaseTransform.forward;
            Vector2 forwardPos2D;
            forwardPos2D.x = forwardPos.x;
            forwardPos2D.y = forwardPos.z;
            properAngle -= Mathf.Rad2Deg * Mathf.Atan(forwardPos.y / Vector2.Distance(Vector2.zero, forwardPos2D));
            return properAngle;
        }
    }

}