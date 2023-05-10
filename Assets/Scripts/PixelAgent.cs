using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;



public class PixelAgent : Agent
{
    public CanvasEnv env;

    GameObject pixel;
    Rigidbody pixel_RB;

    // User input
    private float speed = 40.0f;
    private float horizontalInput;
    private float forwardInput;

    public override void Initialize()
    {
        pixel = gameObject;
        pixel_RB = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        //  Maybe set velocity to zero
        pixel_RB.velocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //  Where the agent is
        sensor.AddObservation(pixel.transform.position);

        for (int i = 0; i < env.size * env.size; i++)
        {
            //  The picture
            sensor.AddObservation(env.canvas[i]);
            //  The environment
            sensor.AddObservation(env.environment[i]);
        } 
        sensor.AddObservation(env.OutOfBoundary());
    }


    //Heuristic Controls for debugging.Has not been tested, but "TestMotionScript" contains similar code that will work for testing.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // This is for player input
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
        
        //  REWARDS
        
        //  filling a pixel on the picture

        //  Don't get out of the canvas
    }
}
