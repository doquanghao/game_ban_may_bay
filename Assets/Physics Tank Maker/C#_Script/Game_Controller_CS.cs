using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


namespace ChobiAssets.PTM
{

    public class Game_Controller_CS : MonoBehaviour
    {
        /*
        * Kịch bản này được gắn vào "Game_Controller" trong cảnh.
        * Kịch bản này điều khiển các thiết lập về vật lý, cài đặt va chạm giữa các lớp và trạng thái con trỏ trong cảnh.
        * Ngoài ra, cũng điều khiển các chức năng chung như thoát và tạm dừng.
       */

        public bool Fix_Frame_Rate = true;
        public int Target_Frame_Rate = 120;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // Thiết lập lớp.
            Layer_Settings_CS.Layers_Collision_Settings();

            // Đặt tốc độ khung hình.
            if (Fix_Frame_Rate)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = Target_Frame_Rate;
            }
        }

        void Update()
        {
            // Thoát.
            if (General_Settings_CS.Allow_Instant_Quit && Input.GetKeyDown(General_Settings_CS.Quit_Key))
            {
                Application.Quit();
                return;
            }
        }

    }

}