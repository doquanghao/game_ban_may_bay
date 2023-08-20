using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb_Control_CS : MonoBehaviour
{
    public GameObject effect;
    void OnCollisionEnter(Collision collision)
    {
        GameObject effectClone = Instantiate(effect, transform);
        effectClone.GetComponent<ParticleSystem>().Play();
        GetComponent<AudioSource>().Play();
        Destroy(this.gameObject, 0.3f);
        Destroy(effectClone, 5f);
    }

}
