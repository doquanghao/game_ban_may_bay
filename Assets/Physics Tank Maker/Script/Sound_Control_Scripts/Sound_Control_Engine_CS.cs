using UnityEngine;
using System.Collections;

namespace ChobiAssets.PTM
{

    [RequireComponent(typeof(AudioSource))]

    public class Sound_Control_Engine_CS : MonoBehaviour
    {
        /*
        Tập lệnh này được gắn vào đối tượng "Engine_Sound" trong xe tăng.
        Tập lệnh này điều khiển âm thanh động cơ trong xe tăng.
        */

        public float Min_Engine_Pitch = 1.0f;//Điểm pitch tối thiểu của âm thanh động cơ.
        public float Max_Engine_Pitch = 2.0f;//Điểm pitch tối đa của âm thanh động cơ.
        public float Min_Engine_Volume = 0.5f;//Âm lượng tối thiểu của âm thanh động cơ.
        public float Max_Engine_Volume = 1.0f;//Âm lượng tối đa của âm thanh động cơ.
        public Rigidbody Left_Reference_Rigidbody;//Rigidbody tham chiếu bánh xe trái.
        public Rigidbody Right_Reference_Rigidbody;//Rigidbody tham chiếu bánh xe phải.
        public string Reference_Name_L;//Tên của đối tượng tham chiếu bánh xe trái.
        public string Reference_Name_R;//Tên của đối tượng tham chiếu bánh xe phải.
        public string Reference_Parent_Name_L;//Tên của cha của đối tượng tham chiếu bánh xe trái.
        public string Reference_Parent_Name_R;//Tên của cha của đối tượng tham chiếu bánh xe phải.

        // Để kịch bản biên tập hiển thị vận tốc hiện tại.
        public float Left_Velocity;//Tốc độ của bánh xe trái.
        public float Right_Velocity;//Tốc độ của bánh xe phải.

        // Đối với kịch bản biên tập viên.
        public bool Has_Changed;//Biến để xác định xem đã có thay đổi hay không.

        AudioSource thisAudioSource;//Đối tượng AudioSource để điều khiển âm thanh.
        float currentRate;//Tỷ lệ hiện tại của tốc độ động cơ.
        float targetRate;// Tỷ lệ mục tiêu của tốc độ động cơ.
        float accelerationRate = 8.0f;//Tỷ lệ tăng tốc của âm thanh động cơ.
        float decelerationRate = 4.0f;//Tỷ lệ giảm tốc của âm thanh động cơ.
        Drive_Control_CS driveScript;//Tham chiếu tới tập lệnh "Drive_Control_CS".


        void Start()
        {
            Initial_Settings();
        }


        void Initial_Settings()
        {
            thisAudioSource = GetComponent<AudioSource>();
            thisAudioSource.playOnAwake = false;
            thisAudioSource.loop = true;
            thisAudioSource.volume = 0.0f;
            thisAudioSource.Play();

            // Tìm các rigidbody tham chiếu.
            Transform bodyTransform = transform.parent;
            if (Left_Reference_Rigidbody == null)
            {
                // Bánh xe tham chiếu bên trái đã bị thay đổi.
                if (string.IsNullOrEmpty(Reference_Name_L) == false && string.IsNullOrEmpty(Reference_Parent_Name_L) == false)
                {
                    Transform leftReferenceTransform = bodyTransform.Find(Reference_Parent_Name_L + "/" + Reference_Name_L);
                    if (leftReferenceTransform)
                    {
                        Left_Reference_Rigidbody = leftReferenceTransform.GetComponent<Rigidbody>();
                    }
                }
            }
            if (Right_Reference_Rigidbody == null)
            {
                // Bánh xe tham chiếu bên phải đã bị thay đổi.
                if (string.IsNullOrEmpty(Reference_Name_R) == false && string.IsNullOrEmpty(Reference_Parent_Name_R) == false)
                {
                    Transform rightReferenceTransform = bodyTransform.Find(Reference_Parent_Name_R + "/" + Reference_Name_R);
                    if (rightReferenceTransform)
                    {
                        Right_Reference_Rigidbody = rightReferenceTransform.GetComponent<Rigidbody>();
                    }
                }
            }

            // Lấy "Drive_Control_CS".
            driveScript = GetComponentInParent<Drive_Control_CS>();
        }


        void Update()
        {
            Engine_Sound();
        }


        void Engine_Sound()
        {
            // Lấy vận tốc.
            Left_Velocity = Left_Reference_Rigidbody.velocity.magnitude;
            Right_Velocity = Right_Reference_Rigidbody.velocity.magnitude;

            // Đặt tỷ lệ.
            targetRate = (Left_Velocity + Right_Velocity) / 2.0f / driveScript.Max_Speed;

            if (targetRate > currentRate)
            { // Tăng tốc
                currentRate = Mathf.MoveTowards(currentRate, targetRate, accelerationRate * Time.deltaTime);
            }
            else
            { // Giảm tốc
                currentRate = Mathf.MoveTowards(currentRate, targetRate, decelerationRate * Time.deltaTime);
            }
            // Đặt pitch và âm lượng.
            thisAudioSource.pitch = Mathf.Lerp(Min_Engine_Pitch, Max_Engine_Pitch, currentRate);
            thisAudioSource.volume = Mathf.Lerp(Min_Engine_Volume, Max_Engine_Volume, currentRate);
        }
    }

}