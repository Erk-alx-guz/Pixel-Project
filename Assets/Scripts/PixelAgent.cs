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
    private float speed = 1.5f;
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
        var action = actionsOut.DiscreteActions;

        var dirToGo = Vector3.zero;

        if (!done)
        {
            if (Input.GetKey(KeyCode.D))
            {
                dirToGo = transform.forward * -0.75f;
            }
            else if (Input.GetKey(KeyCode.W))
            {
                dirToGo = transform.right * 0.75f;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                dirToGo = transform.forward * 0.75f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                dirToGo = transform.right * -0.75f;
            }

            pixel_RB.AddForce(dirToGo * speed, ForceMode.VelocityChange);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (!done)
            MoveAgent(actionBuffers.DiscreteActions);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        //var rotateDir = Vector3.zero;

        var action = act[0];

        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 0.75f;
                break;
            case 2:
                dirToGo = transform.forward * -0.75f;
                break;
            case 3:
                dirToGo = transform.right * -0.75f;
                break;
            case 4:
                dirToGo = transform.right * 0.75f;
                break;
            //case 5:
            //    rotateDir = transform.up * 1f;
            //    break;
            //case 6:
            //    rotateDir = transform.up * -1f;
            //    break;
        }
        //transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        pixel_RB.AddForce(dirToGo * speed, ForceMode.VelocityChange);
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

                //if (targetBounds.Contains(agentCollider.bounds.min + wiggleRoom) && targetBounds.Contains(agentCollider.bounds.max - wiggleRoom))
                if (targetBounds.Contains(agentCollider.bounds.center))
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
