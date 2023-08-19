using UnityEngine;
using System.Collections;

namespace ChobiAssets.PTM
{

    public class Bullet_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to bullet prefabs.
		 * This script controls the posture of the bullet, and supports the collision detecting by casting a ray while flying.
		 * When the bullet hits the target, this script sends the damage value to the "Damage_Control_##_##_CS" script in the hit collider.
		 * The damage value is calculated considering the hit angle.
		*/


        // User options >>
        public Transform This_Transform;
        public Rigidbody This_Rigidbody;
        // Only for AP
        public GameObject Impact_Object;
        public GameObject Ricochet_Object;
        // Only for HE
        public GameObject Explosion_Object;
        public float Explosion_Force;
        public float Explosion_Radius;
        // << User options


        // Set by "Bullet_Generator_CS".
        public float Attack_Point;
        public float Initial_Velocity;
        public float Life_Time;
        public float Attack_Multiplier = 1.0f;
        public bool Debug_Flag = false;

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

            // Set the collision detection mode.
            This_Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            Destroy(this.gameObject, Life_Time);
        }


        void Update()
        {
            if (isLiving == false)
            {
                return;
            }

            // Set the posture.
            This_Transform.LookAt(This_Rigidbody.position + This_Rigidbody.velocity);
        }


        void OnCollisionEnter(Collision collision)
        { // The collision has been detected by the physics engine.
            if (isLiving)
            {
                // Start the hit process.
                HE_Hit_Process();
            }
        }


        void HE_Hit_Process()
        {
            isLiving = false;

            // Create the explosion effect object.
            if (Explosion_Object)
            {
                Instantiate(Explosion_Object, This_Transform.position, Quaternion.identity);
            }

            // Remove the useless components.
            Destroy(GetComponent<Renderer>());
            Destroy(GetComponent<Rigidbody>());
            Destroy(GetComponent<Collider>());

            // Add the explosion force to the objects within the explosion radius.
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
                { // The collider should be behind an obstacle.
                    return;
                }

                // Calculate the distance loss rate.
                var loss = Mathf.Pow((Explosion_Radius - raycastHit.distance) / Explosion_Radius, 2);

                // Add force to the rigidbody.
                Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
                if (rigidbody)
                {
                    rigidbody.AddForce(direction * Explosion_Force * loss);
                }
                
            }
        }

    }

}