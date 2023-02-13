using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public GameObject bullet;
    public GameObject turret;
    public GameObject enemy;
    // Start is called before the first frame update
    void CreateBullet()
    {
        Instantiate(bullet,turret.transform.position,turret.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 aimAt = CalculateTrajectory();
            if (aimAt != Vector3.zero)
            {

                this.transform.forward = CalculateTrajectory();
                CreateBullet();
            }
        }
    }

    private Vector3 CalculateTrajectory()
    {
        Vector3 p = enemy.transform.position - transform.position;
        Vector3 v = enemy.transform.forward * enemy.GetComponent<Drive>().speed;
        float s = bullet.GetComponent<MoveShell>().speed;

        float a = Vector3.Dot(v, v) - s * s;
        float b = Vector3.Dot(p, v);
        float c = Vector3.Dot(p, p);

        float t = 0;
        float t1 = (-b + Mathf.Sqrt(b*b - 4*a*c) )/ 2 * a;
        float t2 = (-b - Mathf.Sqrt(b*b - 4*a*c) )/ 2 * a;

        if (t1 < 0.0f && t2 < 0.0f) return Vector3.zero;
        else if (t1 < 0.0f) t = t2;
        else if (t2 < 0.0f) t = t1;
        else
        {

            t = Mathf.Max(new float[] { t1, t2 });
        }

        //return the vector when there's t time with that velocity that collides two vectors together
        return t * p + v;
    }
}
