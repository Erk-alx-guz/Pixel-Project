using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using System.Linq;

public class PixelAgent : Agent
{
    public CanvasEnv env;

    GameObject pixel;
    Rigidbody pixel_RB;

    // User input
    private float speed = 2.0f;
    private float horizontalInput;
    private float forwardInput;

    public int gridLocation;
    Collider coll;

    public bool done = false;

    public bool outOfBoundary = false;

    //  agent coordinate on big array
    public int agent_x_coordinate;

    public int agent_y_coordinate;

    void Start()
    {
        coll = GetComponent<Collider>();
    }

    public override void Initialize()
    {
        pixel = gameObject;
        pixel_RB = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        //  Maybe set velocity to zero
        pixel_RB.velocity = Vector3.zero;
        pixel_RB.rotation = Quaternion.identity;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(env.canvas);                      //  1600

        sensor.AddObservation(agent_x_coordinate);              //  1

        sensor.AddObservation(agent_y_coordinate);              //  1

        sensor.AddObservation(outOfBoundary);                   //  1

        sensor.AddObservation(done);                            //  1
    }   

    //Heuristic Controls for debugging.Has not been tested, but "TestMotionScript" contains similar code that will work for testing.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //This is for player input

       horizontalInput = Input.GetAxis("Horizontal");
       forwardInput = Input.GetAxis("Vertical");

        pixel.transform.Translate(Vector3.right * Time.deltaTime * speed * forwardInput);
        pixel.transform.Translate(Vector3.back * Time.deltaTime * speed * horizontalInput);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var i = -1;

        var vectorAction = actionBuffers.ContinuousActions;

        //  Don't do anything untill the environment is ready
        if (env.EnvironmentReady())
        {
            pixel.transform.Translate(Vector3.right * Time.deltaTime * speed * vectorAction[++i]);
            pixel.transform.Translate(Vector3.back * Time.deltaTime * speed * vectorAction[++i]);
        }
    }


    /// <summary>
    /// When hitting another pixel at a fasl velocity give bad reward
    /// 
    /// This is to prevent pixels from flipping 
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Pixel") && (Mathf.Abs(pixel_RB.velocity.x) > 2 || Mathf.Abs(pixel_RB.velocity.z) > 2))
        {
            AddReward(-0.5f);
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.tag != "Pixel")
        {
            if (other.bounds.Contains(coll.bounds.center))
            {
                gridLocation = (other.tag[0] - 48) * 10;
                gridLocation += other.tag[1] - 48;
            }
        }
    }
}
