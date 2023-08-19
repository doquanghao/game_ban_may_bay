using UnityEngine;
using System.Collections;

namespace ChobiAssets.PTM
{

    public class Drive_Control_CS : MonoBehaviour
    {

        /*
  * Kịch bản này được gắn vào "MainBody" của xe tăng.
  * Kịch bản này điều khiển việc lái xe của xe tăng, chẳng hạn như tốc độ, moment xoắn, gia tốc và như vậy.
  * Kịch bản này hoạt động kết hợp với "Drive_Wheel_Parent_CS" trong các đối tượng 'Create_##Wheels', và "Drive_Wheel_CS" trong bánh lái.
 */

        public float Torque = 2000.0f;
        public float Max_Speed = 8.0f;
        public float Turn_Brake_Drag = 150.0f;
        public float Switch_Direction_Lag = 0.5f;
        public bool Allow_Pivot_Turn;
        public float Pivot_Turn_Rate = 0.3f;

        public bool Acceleration_Flag = false;
        public float Acceleration_Time = 4.0f;
        public float Deceleration_Time = 0.1f;
        public AnimationCurve Acceleration_Curve;

        public bool Torque_Limitter = false;
        public float Max_Slope_Angle = 45.0f;

        public float Parking_Brake_Velocity = 0.5f;
        public float Parking_Brake_Angular_Velocity = 0.1f;

        public bool Use_AntiSlip = false;
        public float Ray_Distance = 1.0f;

        public bool Use_Downforce = false;
        public float Downforce = 25000.0f;
        public AnimationCurve Downforce_Curve;

        public bool Sync_Speed_Rate;
        public float Actual_Speed_Offset_Rate = 1.0f;
        public float Actual_Speed_Tolerance_Rate = 0.2f;


        // Được đặt bởi "inputType_Settings_CS".
        public int inputType = 0;

        // Được tham chiếu từ "Drive_Wheel_Parent_CS".
        public bool Stop_Flag = true; // Được tham chiếu từ cả "inputType_Settings_CS".
        public float L_Input_Rate;
        public float R_Input_Rate;
        public float Turn_Brake_Rate;
        public bool Pivot_Turn_Flag;
        public bool Apply_Brake;

        // Được tham chiếu từ "Drive_Wheel_Parent_CS".
        public float Speed_Rate; // Được tham chiếu từ cả "inputType_Settings_CS".
        public float L_Brake_Drag;
        public float R_Brake_Drag;
        public float Left_Torque;
        public float Right_Torque;

        // Được tham chiếu từ "Fix_Shaking_Rotation_CS".
        public bool Parking_Brake;

        // Được tham chiếu từ "UI_Speed_Indicator_Control_CS".
        public float Current_Velocity;

        Transform thisTransform;
        Rigidbody thisRigidbody;
        float leftSpeedRate;
        float rightSpeedRate;
        float defaultTorque;
        float acceleRate;
        float deceleRate;
        int currentStep;
        bool switchDirectionTimerFlag;
        float currentVelocityMagnutude;

        bool isSelected;

        protected Drive_Control_Input_00_Base_CS inputScript;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            BroadcastMessage("Selected", true, SendMessageOptions.DontRequireReceiver);
            thisTransform = transform;
            thisRigidbody = GetComponent<Rigidbody>();
            defaultTorque = Torque;

            inputType = General_Settings_CS.Input_Type;

            // Đặt tỷ lệ gia tốc.
            if (Acceleration_Flag)
            {
                acceleRate = 1.0f / Acceleration_Time;
                deceleRate = 1.0f / Deceleration_Time;
            }

            // Kiểm tra 'Downforce_Curve'.
            if (Use_Downforce && Downforce_Curve.keys.Length < 2)
            { // 'Downforce_Curve' chưa được đặt.
                Downforce_Curve = Create_Curve();
            }

            // Kiểm tra 'Acceleration_Flag'.
            if (Acceleration_Flag && Acceleration_Curve.keys.Length < 2)
            { // 'Acceleration_Curve' chưa được đặt.
                Acceleration_Curve = Create_Curve();
            }

            inputScript = gameObject.AddComponent<Drive_Control_Input_02_Keyboard_Pressing_CS>();

            if (inputScript != null)
            {
                inputScript.Prepare(this);
            }
        }

        AnimationCurve Create_Curve()
        { // Tạo một AnimationCurve tạm thời.
            Debug.LogWarning("'Curve' is not set correctly in 'Drive_Control_CS'.");
            Keyframe key1 = new Keyframe(0.0f, 0.0f, 1.0f, 1.0f);
            Keyframe key2 = new Keyframe(1.0f, 1.0f, 1.0f, 1.0f);
            return new AnimationCurve(key1, key2);
        }


        void Update()
        {
            inputScript.Drive_Input();

            // Đặt các giá trị lái xe, chẳng hạn như tỷ lệ tốc độ, phanh xoắn và moment xoắn.
            Set_Driving_Values();

            // Lấy các giá trị tốc độ hiện tại;
            Current_Velocity = thisRigidbody.velocity.magnitude;
        }


        void FixedUpdate()
        {
            // Lấy các giá trị tốc độ hiện tại;
            currentVelocityMagnutude = thisRigidbody.velocity.magnitude;

            // Kiểm soát phanh tay tự động.
            Control_Parking_Brake();

            // Gọi hàm chống quay.
            Anti_Spin();

            // Gọi hàm chống trượt.
            if (Use_AntiSlip)
            {
                Anti_Slip();
            }

            // Giới hạn moment xoắn theo góc của độ dốc.
            if (Torque_Limitter)
            {
                Limit_Torque();
            }

            // Thêm lực đối nặng.
            if (Use_Downforce)
            {
                Add_Downforce();
            }
        }


        void Set_Driving_Values()
        {
            if (Acceleration_Flag)
            {
                // Đồng bộ hóa tốc độ ảo và tốc độ thực tế.
                if (Sync_Speed_Rate)
                {
                    if (Pivot_Turn_Flag == false && (L_Input_Rate * R_Input_Rate) != 0.0f)
                    { // Không chuyển hướng, không phanh quay.

                        // Kiểm tra sự khác biệt giữa tốc độ ảo và tốc độ thực tế.
                        var currentActualSpeedRate = currentVelocityMagnutude / Max_Speed;
                        if (((Speed_Rate * Actual_Speed_Offset_Rate) - currentActualSpeedRate) > Actual_Speed_Tolerance_Rate)
                        { // Sự khác biệt lớn.

                            // Kiểm tra góc nghiêng.
                            var currentTiltAngle = Mathf.Abs(Mathf.DeltaAngle(thisTransform.eulerAngles.x, 0.0f));
                            if (currentTiltAngle < 5.0f)
                            { // Gần như nằm ngang.

                                // Làm cho tốc độ ảo gần với tốc độ thực tế.
                                if (leftSpeedRate != 0.0f)
                                {
                                    leftSpeedRate = Mathf.MoveTowards(leftSpeedRate, currentActualSpeedRate * Mathf.Sign(leftSpeedRate), 40.0f * Time.deltaTime);
                                }
                                if (rightSpeedRate != 0.0f)
                                {
                                    rightSpeedRate = Mathf.MoveTowards(rightSpeedRate, currentActualSpeedRate * Mathf.Sign(rightSpeedRate), 40.0f * Time.deltaTime);
                                }
                            }
                        }
                    }
                }

                // Đặt giá trị "TyLeToc_L" và "TyLeToc_R".
                leftSpeedRate = Calculate_Speed_Rate(leftSpeedRate, -L_Input_Rate);
                rightSpeedRate = Calculate_Speed_Rate(rightSpeedRate, R_Input_Rate);
            }
            else
            {
                leftSpeedRate = -L_Input_Rate;
                rightSpeedRate = R_Input_Rate;
            }

            // Đặt giá trị "TyLeToc_Vantoc".
            Speed_Rate = Mathf.Max(Mathf.Abs(leftSpeedRate), Mathf.Abs(rightSpeedRate));
            Speed_Rate = Acceleration_Curve.Evaluate(Speed_Rate);

            // Kiểm tra chuyển hướng bánh.
            if (Pivot_Turn_Flag)
            { // Xe tăng đang thực hiện chuyển hướng.
                // Giới hạn tỷ lệ tốc độ.
                Speed_Rate = Mathf.Clamp(Speed_Rate, 0.0f, Pivot_Turn_Rate);
            }

            // Đặt giá trị "PhanhXoanBenTrai" và "PhanhXoanBenPhai".
            L_Brake_Drag = Mathf.Clamp(Turn_Brake_Drag * -Turn_Brake_Rate, 0.0f, Turn_Brake_Drag);
            R_Brake_Drag = Mathf.Clamp(Turn_Brake_Drag * Turn_Brake_Rate, 0.0f, Turn_Brake_Drag);

            // Đặt giá trị "MomentXoanBenTrai" và "MomentXoanBenPhai".
            Left_Torque = Torque * -Mathf.Sign(leftSpeedRate) * Mathf.Ceil(Mathf.Abs(leftSpeedRate)); // (Note.) When the "leftSpeedRate" is zero, the torque will be set to zero.
            Right_Torque = Torque * Mathf.Sign(rightSpeedRate) * Mathf.Ceil(Mathf.Abs(rightSpeedRate));
        }


        float Calculate_Speed_Rate(float currentRate, float targetRate)
        {
            if (switchDirectionTimerFlag)
            {
                return 0.0f;
            }

            if (currentRate == targetRate)
            {
                return currentRate;
            }

            float moveRate;
            if (Apply_Brake)
            {
                moveRate = deceleRate * 10.0f;
            }
            else if (targetRate == 0.0f)
            {
                moveRate = deceleRate;
            }
            else if (Mathf.Sign(targetRate) == Mathf.Sign(currentRate))
            { // Cả hai tỷ lệ cùng hướng.
                if (Mathf.Abs(targetRate) > Mathf.Abs(currentRate))
                {  // Nên gia tốc.
                    moveRate = acceleRate;
                }
                else
                { // Nên giảm tốc.
                    moveRate = deceleRate;
                }
            }
            else
            { // Cả hai tỷ lệ có hướng khác nhau. >> Nên giảm tốc cho đến khi tỷ lệ hiện tại trở thành không.
              // Giảm tốc đột ngột như phanh.
                moveRate = deceleRate * 10.0f;

                // Dừng xe tăng trong quá trình chuyển hướng.
                var tempRate = Mathf.MoveTowards(currentRate, targetRate, moveRate * Time.deltaTime);
                if ((currentRate > 0.0f && tempRate <= 0.0f) || (currentRate <= 0.0f && tempRate > 0.0f))
                { // Từ phía trước sang phía sau, hoặc từ phía sau sang phía trước.
                    StartCoroutine("Switch_Direction_Timer");
                    return tempRate;
                }
            }

            return Mathf.MoveTowards(currentRate, targetRate, moveRate * Time.deltaTime);
        }


        IEnumerator Switch_Direction_Timer()
        {
            switchDirectionTimerFlag = true;
            var count = 0.0f;
            while (count < Switch_Direction_Lag)
            {
                count += Time.deltaTime;
                yield return null;
            }
            switchDirectionTimerFlag = false;
        }


        void Control_Parking_Brake()
        {
            if (Stop_Flag)
            { // Xe tăng nên dừng.

                // Lấy vận tốc góc của Rigidbody.
                var currentAngularVelocityMagnitude = thisRigidbody.angularVelocity.magnitude;

                // Kiểm soát phanh tay dựa trên vận tốc.
                if (Parking_Brake)
                { // Phanh tay đang hoạt động.

                    // Kiểm tra vận tốc của Rigidbody.
                    if (currentVelocityMagnutude > Parking_Brake_Velocity || currentAngularVelocityMagnitude > Parking_Brake_Angular_Velocity)
                    { // Rigidbody nên đang chuyển động bởi lực ngoại.

                        // Giải phóng phanh tay.
                        Parking_Brake = false;
                        thisRigidbody.constraints = RigidbodyConstraints.None;
                        return;
                    } // Rigidbody gần như dừng lại.
                    return;
                }
                else
                { // Phanh tay không hoạt động.

                    // Kiểm tra vận tốc của Rigidbody.
                    if (currentVelocityMagnutude < Parking_Brake_Velocity && currentAngularVelocityMagnitude < Parking_Brake_Angular_Velocity)
                    { // Rigidbody gần như dừng lại.

                        // Đặt phanh tay.
                        Parking_Brake = true;
                        thisRigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationY;
                        leftSpeedRate = 0.0f;
                        rightSpeedRate = 0.0f;
                        return;
                    } // Rigidbody gần như dừng lại.
                    return;
                }
            }
            else
            { // Xe tăng nên chạy.
                if (Parking_Brake == true)
                { // Phanh tay vẫn đang hoạt động.

                    // Giải phóng phanh tay.
                    Parking_Brake = false;
                    thisRigidbody.constraints = RigidbodyConstraints.None;
                }
            }
        }

        void Anti_Spin()
        {
            // Giảm chuyển động xoay bằng cách kiểm soát vận tốc góc của Rigidbody.
            if (L_Input_Rate != R_Input_Rate && Turn_Brake_Rate == 0.0f)
            { // Xe tăng không nên thực hiện pivot-turn hoặc brake-turn.

                // Giảm vận tốc góc theo trục Y.
                Vector3 currentAngularVelocity = thisRigidbody.angularVelocity;
                currentAngularVelocity.y *= 0.9f;

                // Đặt vận tốc góc mới.
                thisRigidbody.angularVelocity = currentAngularVelocity;
            }
        }

        void Anti_Slip()
        {
            // Giảm trượt bằng cách kiểm soát vận tốc của Rigidbody.

            // Phát ra một tia xuống để phát hiện mặt đất.
            var ray = new Ray();
            ray.origin = thisTransform.position;
            ray.direction = -thisTransform.up;
            if (Physics.Raycast(ray, Ray_Distance, Layer_Settings_CS.Anti_Slipping_Layer_Mask))
            { // Tia chạm đất.

                // Kiểm soát vận tốc của Rigidbody.
                Vector3 currentVelocity = thisRigidbody.velocity;
                if (leftSpeedRate == 0.0f && rightSpeedRate == 0.0f)
                { // Xe tăng nên dừng lại.
                  // Giảm vận tốc của Rigidbody một cách dần dần.
                    currentVelocity.x *= 0.9f;
                    currentVelocity.z *= 0.9f;
                }
                else
                { // Xe tăng đang di chuyển.
                    float sign;
                    if (leftSpeedRate == rightSpeedRate)
                    { // Xe tăng nên di chuyển thẳng đi phía trước hoặc phía sau.
                        if (Mathf.Abs(leftSpeedRate) < 0.2f)
                        { // Xe tăng gần dừng lại.
                          // Hủy hàm, để xe tăng có thể chuyển hướng trơn tru giữa đi phía trước và đi phía sau.
                            return;
                        }
                        sign = Mathf.Sign(leftSpeedRate);
                    }
                    else if (leftSpeedRate == -rightSpeedRate)
                    { // Xe tăng đang thực hiện pivot-turn.
                        sign = 1.0f;
                    }
                    else
                    { // Xe tăng đang thực hiện brake-turn.
                        sign = Mathf.Sign(leftSpeedRate + rightSpeedRate);
                    }
                    // Thay đổi vận tốc của Rigidbody bằng cách ép buộc.
                    currentVelocity = Vector3.MoveTowards(currentVelocity, thisTransform.forward * sign * Current_Velocity, 32.0f * Time.fixedDeltaTime);
                }

                // Đặt vận tốc mới.
                thisRigidbody.velocity = currentVelocity;
            }
        }


        void Limit_Torque()
        {
            // Giảm moment quán tính dựa trên góc nghiêng của địa hình.
            var torqueRate = Mathf.DeltaAngle(thisTransform.eulerAngles.x, 0.0f) / Max_Slope_Angle;
            if (leftSpeedRate > 0.0f && rightSpeedRate > 0.0f)
            { // Xe tăng nên tiến về phía trước.
                Torque = Mathf.Lerp(defaultTorque, 0.0f, torqueRate);
            }
            else
            { // Xe tăng nên lùi về phía sau.
                Torque = Mathf.Lerp(defaultTorque, 0.0f, -torqueRate);
            }
        }

        void Add_Downforce()
        {
            // Thêm lực ép xuống (downforce).
            var downforceRate = Downforce_Curve.Evaluate(Current_Velocity / Max_Speed);
            thisRigidbody.AddRelativeForce(Vector3.up * (-Downforce * downforceRate));
        }

        void Call_Indicator()
        {
            // Gọi đối tượng "UI_Speed_Indicator_Control_CS" trong scene.
            if (UI_Speed_Indicator_Control_CS.Instance)
            {
                bool isManual = (General_Settings_CS.Input_Type == 0); // "Mouse + Keyboard (Stepwise)".
                UI_Speed_Indicator_Control_CS.Instance.Get_Drive_Script(this, isManual, currentStep);
            }
        }
        void Selected(bool isSelected)
        {
            this.isSelected = isSelected;

            if (isSelected)
            {
                Call_Indicator();
            }
        }

    }

}