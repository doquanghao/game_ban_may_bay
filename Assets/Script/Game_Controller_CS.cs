using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

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
        public TMP_Text Mission;
        public TMP_Text Destruction_Level;
        public TMP_Text Text_End_Game;
        public GameObject displayTheWinScreen;

        public int requiredEnemiesToWin = 10;

        public int enemiesDestroyed = 0;
        public int enemiesDestruction = 0;

        void Start()
        {
            displayTheWinScreen.SetActive(false);
            enemiesDestroyed = 0;
            enemiesDestruction = 0;
            Mission.text = enemiesDestroyed + "/" + requiredEnemiesToWin;
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
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
                return;
            }
        }

        public void DestructionLevel()
        {
            enemiesDestruction++;
            Destruction_Level.text = "Mức phá hủy " + enemiesDestruction / 3 + "%";
            if (enemiesDestruction / 3 == 100)
            {
                Text_End_Game.text = "Thua";
                Cursor.lockState = CursorLockMode.None; // Bỏ khóa con trỏ chuột
                Cursor.visible = true; // Hiển thị con trỏ chuột
                displayTheWinScreen.SetActive(true);
                Time.timeScale = 0f;
            }
        }

        public void EnemyDestroyed()
        {
            enemiesDestroyed++;
            Mission.text = enemiesDestroyed + "/" + requiredEnemiesToWin;
            if (enemiesDestroyed >= requiredEnemiesToWin)
            {
                Text_End_Game.text = "Chiến thắng";
                // Người chơi thắng
                Cursor.lockState = CursorLockMode.None; // Bỏ khóa con trỏ chuột
                Cursor.visible = true; // Hiển thị con trỏ chuột
                displayTheWinScreen.SetActive(true);
                Time.timeScale = 0f;
            }
        }
        public void ReloadScene()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked; // Khóa con trỏ chuột
            Cursor.visible = false; // Ẩn con trỏ chuột
            // Lấy index của scene hiện tại
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            // Load lại scene bằng cách sử dụng index
            SceneManager.LoadScene(currentSceneIndex);
        }
        public void QuitGame()
        {
            Time.timeScale = 1f;
            // Dừng game
            Application.Quit();
        }
    }

}