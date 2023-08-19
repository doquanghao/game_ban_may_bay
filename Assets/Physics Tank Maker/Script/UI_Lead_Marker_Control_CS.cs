using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

namespace ChobiAssets.PTM
{

    public class UI_Lead_Marker_Control_CS : MonoBehaviour
    {
        /*
     * Đoạn mã này được gắn vào "MainBody" của xe tăng với "Aiming_Control_CS".
     * Đoạn mã này điều khiển Đánh dấu Chỉ đạo trong cảnh.
    */

        public string Lead_Marker_Name = "Lead_Marker";
        public Sprite Right_Sprite;
        public Sprite Wrong_Sprite;
        public float Calculation_Time = 2.0f;
        public Bullet_Generator_CS Bullet_Generator_Script;


        Aiming_Control_CS aimingScript;
        Image markerImage;
        Transform markerTransform;
        Transform bulletGeneratorTransform;

        bool isSelected;


        void Start()
        {
            // Lấy hình ảnh của đánh dấu trong cảnh.
            if (string.IsNullOrEmpty(Lead_Marker_Name))
            {
                return;
            }
            GameObject markerObject = GameObject.Find(Lead_Marker_Name);
            if (markerObject)
            {
                markerImage = markerObject.GetComponent<Image>();
            }
            else
            {
                // Không thể tìm thấy đánh dấu trong cảnh.
                Debug.LogWarning(Lead_Marker_Name + " không thể tìm thấy trong cảnh.");
                Destroy(this);
                return;
            }
            markerTransform = markerImage.transform;

            // Lấy "Aiming_Control_CS" trong xe tăng.
            aimingScript = GetComponent<Aiming_Control_CS>();
            if (aimingScript == null)
            {
                Debug.LogWarning("'Aiming_Control_CS' không thể tìm thấy trong MainBody.");
                Destroy(this);
            }

            // Lấy "Bullet_Generator_CS".
            if (Bullet_Generator_Script == null)
            {
                Bullet_Generator_Script = GetComponentInChildren<Bullet_Generator_CS>();
            }
            if (Bullet_Generator_Script == null)
            {
                Debug.LogWarning("'Bullet_Generator_CS' không thể tìm thấy. Xe tăng không thể lấy tốc độ viên đạn.");
                Destroy(this);
                return;
            }
            bulletGeneratorTransform = Bullet_Generator_Script.transform;
        }

        // Cập nhật vào cuối vòng lặp.
        void LateUpdate()
        {
            if (isSelected == false)
            {
                return;
            }

            Marker_Control();
        }

        // Điều khiển Đánh dấu Chỉ đạo.
        void Marker_Control()
        {
            // Kiểm tra chế độ dẫn hướng.
            switch (aimingScript.Mode)
            {
                case 0: // Giữ vị trí ban đầu.
                    markerImage.enabled = false;
                    return;
            }

            // Kiểm tra xem mục tiêu đã khóa hay chưa.
            if (aimingScript.Target_Transform == null)
            { // Mục tiêu chưa được khóa.
                markerImage.enabled = false;
                return;
            }

            // Tính toán viên đạn.
            var muzzlePos = bulletGeneratorTransform.position;
            var targetDir = aimingScript.Target_Position - muzzlePos;
            var targetBase = Vector2.Distance(Vector2.zero, new Vector2(targetDir.x, targetDir.z));
            var bulletVelocity = bulletGeneratorTransform.forward * Bullet_Generator_Script.Current_Bullet_Velocity;
            if (aimingScript.Target_Rigidbody)
            { // Mục tiêu có RigidBody.
              // Giảm tốc độ của mục tiêu để giúp việc dẫn hướng bắn.
                bulletVelocity -= aimingScript.Target_Rigidbody.velocity;
            }
            var isHit = false;
            var isTank = false;
            var previousPos = muzzlePos;
            var currentPos = previousPos;
            var count = 0.0f;
            while (count < Calculation_Time)
            {
                // Lấy vị trí hiện tại.
                var virtualPos = bulletVelocity * count;
                virtualPos.y -= 0.5f * -Physics.gravity.y * Mathf.Pow(count, 2.0f);
                currentPos = virtualPos + muzzlePos;

                // Lấy điểm chạm bằng cách đặt tia.
                if (Physics.Linecast(previousPos, currentPos, out RaycastHit raycastHit, Layer_Settings_CS.Aiming_Layer_Mask))
                {
                    currentPos = raycastHit.point;
                    isHit = true;
                    if (raycastHit.rigidbody && raycastHit.transform.root.tag != "Finish")
                    { // Mục tiêu có RigidBody, và nó còn sống.
                        isTank = true;
                    }
                    break;
                }

                // Kiểm tra xem tia đã vượt quá mục tiêu chưa.
                var currenBase = Vector2.Distance(Vector2.zero, new Vector2(virtualPos.x, virtualPos.z));
                if (currenBase > targetBase)
                {
                    break;
                }

                previousPos = currentPos;
                count += Time.fixedDeltaTime;
            }

            // Chuyển điểm chạm thành điểm trên màn hình.
            var screenPos = Camera.main.WorldToScreenPoint(currentPos);
            if (screenPos.z < 0.0f)
            { // Điểm chạm ở phía sau máy ảnh.
                markerImage.enabled = false;
                return;
            }

            // Đặt vị trí.
            markerImage.enabled = true;
            screenPos.z = 128.0f;
            markerTransform.position = screenPos;

            // Đặt giao diện.
            if (isHit)
            { // Đạn sẽ chạm vào cái gì đó.
                if (isTank)
                { // Đối tượng bị chạm có RigidBody.
                  //markerImage.color = Color.red;
                    markerImage.sprite = Right_Sprite;
                }
                else
                { // Đối tượng bị chạm không có RigidBody.
                  //markerImage.color = Color.white;
                    markerImage.sprite = Wrong_Sprite;
                }
            }
            else
            { // Đạn sẽ không chạm vào cái gì cả.
              //markerImage.color = Color.gray;
                markerImage.sprite = Wrong_Sprite;
            }
        }

        // Được gọi khi có sự kiện chọn.
        void Selected(bool isSelected)
        {
            if (isSelected)
            {
                this.isSelected = true;
            }
            else
            {
                if (this.isSelected)
                { // Xe tăng này đã được chọn cho đến bây giờ.
                    this.isSelected = false;
                    markerImage.enabled = false;
                }
            }
        }
    }

}
