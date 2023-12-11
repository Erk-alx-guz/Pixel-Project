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

    Collider coll;

    public bool done = false;

    //  agent coordinate on big array
    public float agent_x_coordinate;

    public float agent_z_coordinate;

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
        pixel_RB.isKinematic = false;
        pixel_RB.velocity = Vector3.zero;
        pixel_RB.rotation = Quaternion.identity;
        done = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(pixel.transform.position.normalized.x);              //  1

        sensor.AddObservation(pixel.transform.position.normalized.z);              //  1

        sensor.AddObservation(done);                            //  1

        for (int i = 0; i < env.GridLocation.Count; i++)
        {
            if (env.GridLocation[i].activeInHierarchy)          // n * 2
            {
                sensor.AddObservation(env.GridLocation[i].transform.position.normalized.x);
                sensor.AddObservation(env.GridLocation[i].transform.position.normalized.z);
            }
            else
            {
                sensor.AddObservation(Vector3.zero.x);
                sensor.AddObservation(Vector3.zero.z);
            }
        }
    }

    //Heuristic Controls for debugging.Has not been tested, but "TestMotionScript" contains similar code that will work for testing.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //This is for player input

        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        if (!done)
        {
            pixel.transform.Translate(Vector3.right * Time.deltaTime * speed * forwardInput);
            pixel.transform.Translate(Vector3.back * Time.deltaTime * speed * horizontalInput);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var i = -1;

        var vectorAction = actionBuffers.ContinuousActions;

        if (!done)
        {
            pixel.transform.Translate(Vector3.right * Time.deltaTime * speed * vectorAction[++i]);
            pixel.transform.Translate(Vector3.back * Time.deltaTime * speed * vectorAction[++i]);
            pixel.transform.Rotate(pixel.transform.up * vectorAction[++i], Time.fixedDeltaTime * 20f);
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
        if (collision.transform.CompareTag("Pixel") && (Mathf.Abs(pixel_RB.velocity.x) > 3 || Mathf.Abs(pixel_RB.velocity.z) > 3))
        {
            AddReward(-0.05f);
        }
    }


    private void OnTriggerStay(Collider other)
    {
        int integerNumber;
        if (other.tag != "Pixel")
        {
            if (int.TryParse(other.tag, out integerNumber))
            {
                Collider agentCollider = pixel.GetComponent<Collider>();

                Bounds targetBounds = env.GridLocation[integerNumber].GetComponent<Collider>().bounds;

                Vector3 wiggleRoom = new Vector3(0.20f, 0.20f, 0.20f);   //  the smaller the values the harder it is

                if (targetBounds.Contains(agentCollider.bounds.min + wiggleRoom) && targetBounds.Contains(agentCollider.bounds.max - wiggleRoom))
                {
                    done = true;
                    env.GridLocation[integerNumber].SetActive(false);
                    pixel_RB.isKinematic = true;
                }
            }
            else
            {
                Console.WriteLine("The string is not a valid integer.");
            }
        }
    }
}
