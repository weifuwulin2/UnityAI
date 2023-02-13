using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public GameObject explosion;
    public float speed = 3f;
    public float Yspeed = 0f;

    //calculate speed based on acceleration
    float mass = 10;
    float force = 2;
    float drag = 1;
    float gravity = -9.8f;
    float gAccel;
    float acceleration;

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "tank")
        {
            GameObject exp = Instantiate(explosion, this.transform.position, Quaternion.identity);
            Destroy(exp, 0.5f);
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        acceleration = force / mass;
        speed += acceleration;
        gAccel = gravity / mass;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        speed *= (1-Time.deltaTime*drag);
        Yspeed = gAccel * Time.deltaTime;
        transform.Translate(0.0f, Yspeed, Time.deltaTime * speed);
    }
}
