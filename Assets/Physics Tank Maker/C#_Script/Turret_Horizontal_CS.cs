using UnityEngine;
using System.Collections;


namespace ChobiAssets.PTM
{

    public class Turret_Horizontal_CS : MonoBehaviour
    {
        /*
        * Kịch bản này xoay pháo theo chiều ngang.
        * Kịch bản này hoạt động phối hợp với "Aiming_Control_CS" trong MainBody.
        */


        public bool Limit_Flag;
        public float Max_Right = 170.0f;
        public float Max_Left = 170.0f;
        public float Speed_Mag = 10.0f;
        public float Acceleration_Time = 0.5f;
        public float Deceleration_Time = 0.5f;
        public Bullet_Generator_CS Bullet_Generator_Script;


        Transform thisTransform;
        Transform parentTransform;
        Aiming_Control_CS aimingScript;
        bool isTurning;
        bool isTracking;
        float angleY;
        Vector3 currentLocalAngles;
        public float Turn_Rate; // Được tham chiếu từ "Sound_Control_Motor_CS".
        float previousTurnRate;
        float bulletVelocity;
        public bool Is_Ready = true; // Được tham chiếu từ "Cannon_Fire_CS".

        void Start()
        {
            Initialize();
        }


        void Initialize()
        { // This function must be called in Start() after changing the hierarchy.
            thisTransform = transform;
            parentTransform = thisTransform.parent;
            aimingScript = GetComponentInParent<Aiming_Control_CS>();
            currentLocalAngles = thisTransform.localEulerAngles;
            angleY = currentLocalAngles.y;
            Max_Right = angleY + Max_Right;
            Max_Left = angleY - Max_Left;

            // Get the "Bullet_Generator_CS".
            if (Bullet_Generator_Script == null)
            {
                Bullet_Generator_Script = GetComponentInChildren<Bullet_Generator_CS>();
            }
            if (Bullet_Generator_Script == null)
            {
                Debug.LogWarning("'Bullet_Generator_CS' cannot be found. The cannon cannot get the bullet velocity.");
                // Set the fake value.
                bulletVelocity = 250.0f;
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
            if (isTurning == false)
            {
                return;
            }

            // Tính toán góc mục tiêu.
            float targetAngle;
            if (isTracking)
            { // Theo dõi mục tiêu.

                // Lấy vị trí của mục tiêu.
                Vector3 targetPosition = aimingScript.Target_Position;
                if (aimingScript.Target_Rigidbody && aimingScript.Use_Auto_Lead)
                {
                    // Mục tiêu có một rigidbody, và tùy chọn "Use_Auto_Lead" đã được bật.
                    // Tính toán góc dẫn đến mục tiêu.
                    float distance = Vector3.Distance(thisTransform.position, targetPosition);
                    if (Bullet_Generator_Script)
                    {
                        bulletVelocity = Bullet_Generator_Script.Current_Bullet_Velocity;
                    }
                    targetPosition += aimingScript.Target_Rigidbody.velocity * aimingScript.Aiming_Blur_Multiplier * (distance / bulletVelocity);
                }

                // Tính toán góc mục tiêu.
                Vector3 targetLocalPos = parentTransform.InverseTransformPoint(targetPosition);
                Vector2 targetLocalPos2D;
                targetLocalPos2D.x = targetLocalPos.x;
                targetLocalPos2D.y = targetLocalPos.z;
                targetAngle = Vector2.Angle(Vector2.up, targetLocalPos2D) * Mathf.Sign(targetLocalPos.x);
                if (Limit_Flag)
                {
                    targetAngle -= angleY;
                }
                else
                {
                    targetAngle = Mathf.DeltaAngle(angleY, targetAngle);
                }
                targetAngle += aimingScript.Adjust_Angle.x;
            }
            else
            { // Không theo dõi. >> Quay trở lại góc ban đầu.
                targetAngle = Mathf.DeltaAngle(angleY, 0.0f);
                if (Mathf.Abs(targetAngle) < 0.01f)
                {
                    isTurning = false;
                }
            }

            // Tính toán "Turn_Rate".
            float sign = Mathf.Sign(targetAngle);
            targetAngle = Mathf.Abs(targetAngle);
            float currentSlowdownAng = Mathf.Abs(Speed_Mag * previousTurnRate) * Deceleration_Time;
            float targetTurnRate = Mathf.Lerp(0.0f, 1.0f, targetAngle / (Speed_Mag * Time.fixedDeltaTime + currentSlowdownAng)) * sign;
            if (targetAngle > currentSlowdownAng)
            {
                Turn_Rate = Mathf.MoveTowards(Turn_Rate, targetTurnRate, Time.fixedDeltaTime / Acceleration_Time);
            }
            else
            {
                Turn_Rate = Mathf.MoveTowards(Turn_Rate, targetTurnRate, Time.fixedDeltaTime / Deceleration_Time);
            }
            previousTurnRate = Turn_Rate;

            // Rotate.
            angleY += Speed_Mag * Turn_Rate * Time.fixedDeltaTime * aimingScript.Turret_Speed_Multiplier;
            if (Limit_Flag)
            {
                angleY = Mathf.Clamp(angleY, Max_Left, Max_Right);
                if (angleY <= Max_Left || angleY >= Max_Right)
                {
                    Turn_Rate = 0.0f;
                }
            }
            currentLocalAngles.y = angleY;
            thisTransform.localEulerAngles = currentLocalAngles;


            // Đặt trạng thái "Is_Ready".
            if (targetAngle <= aimingScript.OpenFire_Angle)
            {
                Is_Ready = true; // Được tham chiếu từ "Cannon_Fire_CS".
            }
            else
            {
                Is_Ready = false; // Được tham chiếu từ "Cannon_Fire_CS".
            }
        }
    }

}