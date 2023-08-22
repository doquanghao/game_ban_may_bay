using UnityEngine;
using System.Collections;

namespace ChobiAssets.PTM
{

    public class Bullet_Control_CS : MonoBehaviour
    {
        /*
         * Kịch bản này được gắn vào các đối tượng prefab đạn.
         * Kịch bản này điều khiển tư thế của viên đạn và hỗ trợ việc phát hiện va chạm bằng cách cast một tia ray trong quá trình bay.
         * Khi viên đạn chạm vào mục tiêu, kịch bản này gửi giá trị sát thương tới kịch bản "Damage_Control_##_##_CS" trong collider mục tiêu.
         * Giá trị sát thương được tính toán dựa trên góc va chạm.
        */



        public Transform This_Transform;
        public Rigidbody This_Rigidbody;
        public GameObject Explosion_Object;
        public float Explosion_Force;
        public float Explosion_Radius;


        // Được thiết lập bởi "Bullet_Generator_CS".
        public float Attack_Point;
        public float Initial_Velocity;
        public float Life_Time;
        public float Attack_Multiplier = 1.0f;
        public bool Debug_Flag = false;
        public Game_Controller_CS Game_Controller_CS;

        bool isLiving = true;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            if (This_Transform == null)
            {
                This_Transform = transform;
            }
            if (This_Rigidbody == null)
            {
                This_Rigidbody = GetComponent<Rigidbody>();
            }

            // Thiết lập chế độ phát hiện va chạm.
            This_Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            Destroy(this.gameObject, Life_Time);
        }


        void Update()
        {
            if (isLiving == false)
            {
                return;
            }

            // Thiết lập tư thế.
            This_Transform.LookAt(This_Rigidbody.position + This_Rigidbody.velocity);
        }


        void OnCollisionEnter(Collision collision)
        { // Va chạm đã được phát hiện bởi bộ máy vật lý.
            if (isLiving)
            {
                if (collision.gameObject.CompareTag("b52"))
                {
                    Game_Controller_CS.EnemyDestroyed();
                    // Xóa GameObject b52
                    Destroy(collision.transform.parent.gameObject);

                }
                // Bắt đầu quá trình va chạm.
                HE_Hit_Process();
            }
        }


        void HE_Hit_Process()
        {
            isLiving = false;

            // Tạo đối tượng hiệu ứng nổ.
            if (Explosion_Object)
            {
                Instantiate(Explosion_Object, This_Transform.position, Quaternion.identity);
            }

            // Loại bỏ các thành phần không cần thiết.
            Destroy(GetComponent<Renderer>());
            Destroy(GetComponent<Rigidbody>());
            Destroy(GetComponent<Collider>());

            // Thêm lực nổ vào các đối tượng trong bán kính nổ.
            Explosion_Force *= Attack_Multiplier;
            var colliders = Physics.OverlapSphere(This_Transform.position, Explosion_Radius, Layer_Settings_CS.Layer_Mask);
            for (int i = 0; i < colliders.Length; i++)
            {
                Add_Explosion_Force(colliders[i]);
            }

            Destroy(this.gameObject, 0.01f * Explosion_Radius);
        }


        void Add_Explosion_Force(Collider collider)
        {
            if (collider == null)
            {
                return;
            }

            Vector3 direction = (collider.transform.position - This_Transform.position).normalized;
            var ray = new Ray();
            ray.origin = This_Rigidbody.position;
            ray.direction = direction;
            if (Physics.Raycast(ray, out RaycastHit raycastHit, Explosion_Radius, Layer_Settings_CS.Layer_Mask))
            {
                if (raycastHit.collider != collider)
                { // Collider nên nằm sau vật cản.
                    return;
                }

                // Tính tỷ lệ mất mát khoảng cách.
                var loss = Mathf.Pow((Explosion_Radius - raycastHit.distance) / Explosion_Radius, 2);

                // Thêm lực vào rigidbody.
                Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
                if (rigidbody)
                {
                    rigidbody.AddForce(direction * Explosion_Force * loss);
                }

            }
        }

    }

}