using UnityEngine;
using System.Collections;

namespace ChobiAssets.PTM
{

    public class Cannon_Fire_CS : MonoBehaviour
    {
        //Thời gian nạp lại đạn
        public float Reload_Time = 2.0f;
        public float Recoil_Force = 5000.0f;//lực giật lùi

        public float Loading_Count;
        public bool Is_Loaded = true;

        Rigidbody bodyRigidbody;
        Transform thisTransform;
        public Bullet_Generator_CS Bullet_Generator_Scripts;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            thisTransform = transform;
            bodyRigidbody = GetComponentInParent<Rigidbody>();
        }

        void Update()
        {
            if (Is_Loaded && Input.GetKey(General_Settings_CS.Fire_Key))
            {
                Fire();
            }
        }

        //Bắt
        public void Fire()
        {
            // Gọi hàm Bắt từ file Bullet_Generator_Scripts
            Bullet_Generator_Scripts.Fire_Linkage();

            // Thêm lực giật giật vào MainBody.
            bodyRigidbody.AddForceAtPosition(-thisTransform.forward * Recoil_Force, thisTransform.position, ForceMode.Impulse);

            // Nạp lại đạn.
            StartCoroutine("Reload");
        }

        //Thời gian nạp đạn.
        public IEnumerator Reload()
        {
            Is_Loaded = false;
            Loading_Count = 0.0f;

            while (Loading_Count < Reload_Time)
            {
                Loading_Count += Time.deltaTime;
                yield return null;
            }

            Is_Loaded = true;
            Loading_Count = Reload_Time;
        }
    }

}