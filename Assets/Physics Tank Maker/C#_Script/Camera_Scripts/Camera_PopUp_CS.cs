using System.Collections;
using UnityEngine;

namespace ChobiAssets.PTM
{

    // Script được gắn vào "Camera_Pivot" trong xe tăng.
    // Camera pivot sẽ được nâng lên dựa trên FOV của camera chính bởi script này.
    public class Camera_PopUp_CS : MonoBehaviour
    {
        // Các tùy chọn 
        public Camera Main_Camera; // Tham chiếu đến camera chính.
        public float PopUp_Start_FOV = 40.0f;// Góc nhìn của camera chính khi bắt đầu nâng lên.
        public float PopUp_Goal_FOV = 10.0f;// Góc nhìn mục tiêu để nâng lên.
        public AnimationCurve Motion_Curve;// Đường cong hoạt hình.


        Transform thisTransform;
        float currentHeight;
        float targetHeight;

        void Start()
        {
            thisTransform = transform;
        }

        void LateUpdate()
        {
            if (Main_Camera.enabled)
            {
                PopUp();
            }
        }

        void PopUp()
        {
            //Tính toán một tỷ lệ (rate) dựa trên góc nhìn của camera chính (Main_Camera) để sử dụng trong hiệu ứng nâng lên của camera pivot.
            float rate = Mathf.Clamp01((PopUp_Start_FOV - Main_Camera.fieldOfView) / (PopUp_Start_FOV - PopUp_Goal_FOV));
            //Tính toán chiều cao mục tiêu (targetHeight) dựa trên giá trị tỷ lệ (rate) đã tính toán trước đó và đường cong hoạt hình (Motion_Curve).
            targetHeight = Motion_Curve.Evaluate(rate) * 1;
            // Dòng mã này kiểm tra xem chiều cao hiện tại (currentHeight) của camera pivot có khác với chiều cao mục tiêu (targetHeight) hay không.
            // Nếu khác nhau, nó sẽ dùng hàm Mathf.MoveTowards để dịch chuyển giá trị currentHeight dần dần gần đến targetHeight với tốc độ cố định.
            if (currentHeight != targetHeight)
            {
                currentHeight = Mathf.MoveTowards(currentHeight, targetHeight, 10.0f * Time.deltaTime);
            }

            // Thêm chiều cao vào vị trí hiện tại.
            Vector3 currentPosition = thisTransform.position;
            currentPosition.y += currentHeight;
            thisTransform.position = currentPosition;
        }

    }
}
