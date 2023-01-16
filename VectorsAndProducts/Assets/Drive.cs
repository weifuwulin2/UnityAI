using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

// A very simplistic car driving on the x-z plane.

public class Drive : MonoBehaviour
{
    public float speed = 10.0f;
    public float rotationSpeed = 100.0f;
    public GameObject fuel;

    void Start()
    {

    }

   

    void CalculateAngle()
    {
        Vector3 tankForward = transform.up;
        Vector3 fuelDirection = fuel.transform.position - transform.position;

        Debug.DrawRay(this.transform.position, tankForward, Color.green, 2);
        Debug.DrawRay(this.transform.position, fuelDirection, Color.red, 2);

        float dot = tankForward.x * fuelDirection.x + tankForward.y * fuelDirection.y;
        float angle = Mathf.Acos(dot / (tankForward.magnitude * fuelDirection.magnitude));

        Debug.Log("Angle: " + angle * Mathf.Rad2Deg);
        Debug.Log("Unity Angle: " + Vector3.Angle(tankForward, fuelDirection));

        CrossLookAt(tankForward,fuelDirection,angle);
    }
    
    //calculate the cross product between two vectors
    Vector3 Cross(Vector3 v, Vector3 w)
    {
        float crossX = v.y * w.z - v.z * w.y;
        float crossY = v.z * w.x - v.x * w.z;
        float crossZ = v.x * w.y - v.y * w.x;
        return new Vector3(crossX, crossY, crossZ);
    }

    private void CrossLookAt(Vector3 tankForward, Vector3 fuelDirection, float angle)
    {
        int clockwise = 1;
        if (Cross(tankForward, fuelDirection).z < 0)
            clockwise = -1;
        this.transform.Rotate(0, 0, angle * Mathf.Rad2Deg * clockwise);
        print("Cross product between two vectors is: " + Cross(tankForward, fuelDirection));
    }

    void CalculateDistance()
    {
        float distance = Mathf.Sqrt(Mathf.Pow(fuel.transform.position.x - transform.position.x,2) +
                                    Mathf.Pow(fuel.transform.position.z - transform.position.z,2));

        Vector3 fuelPos = new Vector3(fuel.transform.position.x, 0, fuel.transform.position.z);
        Vector3 tankPos = new Vector3(transform.position.x, 0, transform.position.z);
        float uDistance = Vector3.Distance(fuelPos, tankPos);

        Vector3 tankToFuel = fuelPos - tankPos;

        Debug.Log("Distance: " + distance);
        Debug.Log("U Distance: " + uDistance);
        Debug.Log("V Magnitude: " + tankToFuel.magnitude);
        Debug.Log("V SqMagnitude: " + tankToFuel.sqrMagnitude);
    }

    bool autoPilot = false;
    private void AutoPilot()
    {
        //1. look at the fuel
        //get two vectors
        Vector3 tankForward = transform.up;
        Vector3 fuelDirection = fuel.transform.position - transform.position;
        //calculate dot product and angle
        CrossLookAt(tankForward, fuelDirection, CalculateAngleBasedOnDot(CalculateDot(tankForward, fuelDirection), tankForward, fuelDirection));
        //2. move towards the fuel until reach the destination
        autoPilot = true;
    }

    void TranslateTo(Vector3 dir, float speed)
    {
        if (autoPilot)
        {
            Vector3 velocity = dir.normalized * speed * Time.deltaTime;
            transform.position += velocity;
        }
        if (Vector3.Distance(transform.position,fuel.transform.position)<=.1f)
        {
            autoPilot=false;
        }
    }

    float CalculateDot(Vector3 v, Vector3 w)
    {
        float dot = v.x*w.x+v.y*w.y;
        print("dot product is : " + dot);
        return dot;
    }

    float CalculateAngleBasedOnDot(float dot, Vector3 v, Vector3 w)
    {
        float angle = Mathf.Acos(dot/(Mathf.Sqrt(v.x*v.x + v.y*v.y+v.z*v.z)*Mathf.Sqrt(w.x*w.x+w.y*w.y+w.z*w.z)));
        print("Angle is: " + angle);
        return angle;
    }



    void LateUpdate()
    {
        // Get the horizontal and vertical axis.
        // By default they are mapped to the arrow keys.
        // The value is in the range -1 to 1
        float translation = Input.GetAxis("Vertical") * speed;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;

        // Make it move 10 meters per second instead of 10 meters per frame...
        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;

        // Move translation along the object's z-axis
        transform.Translate(0, translation, 0);

        // Rotate around our y-axis
        transform.Rotate(0, 0, -rotation);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CalculateDistance();
            CalculateAngle();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            AutoPilot();
            
        }
        Vector3 fuelDirection = fuel.transform.position - transform.position;
        TranslateTo(fuelDirection, 5);
    }
   

}