using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace ChobiAssets.PTM
{

    public class UI_Aim_Marker_Control_CS : MonoBehaviour
    {
        /*
         * Kịch bản này được gắn vào "MainBody" của xe tăng.
         * Kịch bản này điều khiển 'Aim_Marker' trong cảnh.
         * Kịch bản này hoạt động phối hợp với "Aiming_Control_CS" trong xe tăng.
        */

        public string Aim_Marker_Name = "Aim_Marker";


        Aiming_Control_CS aimingScript;
        Image markerImage;
        Transform markerTransform;

        bool isSelected;


        void Start()
        {
            // Lấy hình ảnh của đánh dấu trong cảnh.
            if (string.IsNullOrEmpty(Aim_Marker_Name))
            {
                return;
            }
            GameObject markerObject = GameObject.Find(Aim_Marker_Name);
            if (markerObject)
            {
                markerImage = markerObject.GetComponent<Image>();
            }
            else
            {
                // Không thể tìm thấy đánh dấu trong cảnh.
                Debug.LogWarning(Aim_Marker_Name + " không thể tìm thấy trong cảnh.");
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
        }

        void Update()
        {
            if (isSelected == false)
            {
                return;
            }

            Marker_Control();
        }


        void Marker_Control()
        {
            // Đặt giao diện.
            switch (aimingScript.Mode)
            {
                case 0: // Giữ vị trí ban đầu.
                    markerImage.enabled = false;
                    return;
                case 1: // Nhắm tự do.
                case 2: // Đang khóa mục tiêu.
                    markerImage.enabled = true;
                    if (aimingScript.Target_Transform)
                    {
                        markerImage.color = Color.red;
                    }
                    else
                    {
                        markerImage.color = Color.white;
                    }
                    break;
            }

            // Đặt vị trí.
            // Kiểm tra người chơi có đang tìm mục tiêu bằng ống ngắm súng không.
            if (aimingScript.reticleAimingFlag)
            {
                // Đặt đánh dấu ở trung tâm màn hình.
                markerTransform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 128.0f);
                return;
            }
            // Đặt đánh dấu tại vị trí của mục tiêu.
            Vector3 currentPosition = Camera.main.WorldToScreenPoint(aimingScript.Target_Position);
            if (currentPosition.z < 0.0f)
            { // Phía sau của máy ảnh.
                markerImage.enabled = false;
            }
            else
            {
                currentPosition.z = 128.0f;
            }
            markerTransform.position = currentPosition;
        }

        void Selected(bool isSelected)
        { // Được gọi từ "ID_Settings_CS".
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


        void MainBody_Destroyed_Linkage()
        { // Called from "Damage_Control_Center_CS".

            // Turn off the marker.
            if (isSelected)
            {
                markerImage.enabled = false;
            }

            Destroy(this);
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }

    }

}
