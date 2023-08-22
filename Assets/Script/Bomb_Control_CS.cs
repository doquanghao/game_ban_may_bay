using System.Collections;
using System.Collections.Generic;
using ChobiAssets.PTM;
using UnityEngine;

public class Bomb_Control_CS : MonoBehaviour
{
    public GameObject effect;
    public Game_Controller_CS Game_Controller_CS;


    void Start()
    {
        Game_Controller_CS = FindAnyObjectByType<Game_Controller_CS>();
    }
    void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Map"))
        {
            Game_Controller_CS.DestructionLevel();
        }
        GameObject effectClone = Instantiate(effect, transform);
        effectClone.GetComponent<ParticleSystem>().Play();
        GetComponent<AudioSource>().Play();
        Destroy(this.gameObject, 0.3f);
        Destroy(effectClone, 5f);
    }

}
