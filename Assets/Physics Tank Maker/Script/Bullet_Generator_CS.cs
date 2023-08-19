using UnityEngine;
using System.Collections;

namespace ChobiAssets.PTM
{

    public class Bullet_Generator_CS : MonoBehaviour
    {

        public GameObject HE_Bullet_Prefab;// Prefab đạn HE
        public GameObject MuzzleFire_Object;// Vật phẩm tạo lửa đầu nòng
        public float Attack_Point_HE = 500.0f;// Điểm tấn công cho đạn HE
        public float Initial_Velocity_HE = 500.0f;// Vận tốc ban đầu cho đạn HE

        public float Life_Time = 5.0f;// Thời gian sống của đạn
        public float Offset = 0.5f;// Khoảng cách vị trí bắn so với nòng súng


        public float Attack_Multiplier = 1.0f; // Được thiết lập bởi "Special_Settings_CS".
        public float Current_Bullet_Velocity; // Tham chiếu từ "Turret_Horizontal_CS", "Cannon_Vertical_CS", "UI_Lead_Marker_Control_CS".
        Transform thisTransform;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            thisTransform = transform;

            // Chuyển đổi loại đạn lần đầu.
            Switch_Bullet_Type();
        }


        public void Switch_Bullet_Type()
        {
            // Đặt vận tốc đạn.
            Current_Bullet_Velocity = Initial_Velocity_HE;
        }


        public void Fire_Linkage()
        { 
          // Tạo ra đạn và bắn.
            StartCoroutine(Generate_Bullet());
        }


        IEnumerator Generate_Bullet()
        {
            // Tạo ra lửa đầu nòng.
            if (MuzzleFire_Object)
            {
                Instantiate(MuzzleFire_Object, thisTransform.position, thisTransform.rotation, thisTransform);
            }

            // Tạo ra đạn.
            GameObject bulletObject;
            float attackPoint = 0;

            // HE
            if (HE_Bullet_Prefab == null)
            {
                Debug.LogError("'HE_Bullet_Prefab' is not assigned in the 'Bullet_Generator'.");
                yield break;
            }
            bulletObject = Instantiate(HE_Bullet_Prefab, thisTransform.position + (thisTransform.forward * Offset), thisTransform.rotation) as GameObject;
            attackPoint = Attack_Point_HE;

            // Đặt các giá trị cho "Bullet_Control_CS" trong đạn.
            Bullet_Control_CS bulletScript = bulletObject.GetComponent<Bullet_Control_CS>();
            bulletScript.Attack_Point = attackPoint;
            bulletScript.Initial_Velocity = Current_Bullet_Velocity;
            bulletScript.Life_Time = Life_Time;
            bulletScript.Attack_Multiplier = Attack_Multiplier;

            // Đặt tag.
            bulletObject.tag = "Finish"; // (Note.) The ray cast for aiming does not hit any object with "Finish" tag.

            // Đặt layer.
            bulletObject.layer = Layer_Settings_CS.Bullet_Layer;

            // Bắn.
            yield return new WaitForFixedUpdate();
            Rigidbody rigidbody = bulletObject.GetComponent<Rigidbody>();
            Vector3 currentVelocity = bulletObject.transform.forward * Current_Bullet_Velocity;
            rigidbody.velocity = currentVelocity;
        }
    }

}