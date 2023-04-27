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

    // User input
    private float speed = 40.0f;
    private float horizontalInput;
    private float forwardInput;

    public override void Initialize()
    {
        pixel = gameObject;
    }

    public override void OnEpisodeBegin()
    {
        //  Maybe set velocity to zero
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //  Where the agent is
        sensor.AddObservation(pixel.transform.position);
        //  The picture

        //  Maybe whats taken?

    }


    //Heuristic Controls for debugging.Has not been tested, but "TestMotionScript" contains similar code that will work for testing.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // This is for player input
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        pixel.transform.Translate(Vector3.right * Time.deltaTime * speed * forwardInput);
        pixel.transform.Translate(Vector3.back * Time.deltaTime * speed * horizontalInput);

        //var i = -1;
        //var inputAction = actionsOut.ContinuousActions;

        //inputAction[0] = upperX;
        //inputAction[1] = lowerX;
        //inputAction[2] = strength;

        //var bpDict = m_JdController.bodyPartsDict;

        //bpDict[Upper].SetJointTargetRotation(inputAction[++i], 0, 0);
        //bpDict[Lower].SetJointTargetRotation(inputAction[++i], 0, 0);

        //foreach (var bodyPart in bpDict.Keys)
        //{
        //    bpDict[bodyPart].SetJointStrength(strength);
        //}
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var i = -1;

        var vectorAction = actionBuffers.ContinuousActions;

        //  Don't do anything untill the environment is ready
        if (env.ready)
        {
            pixel.transform.Translate(Vector3.right * Time.deltaTime * speed * vectorAction[++i]);
            pixel.transform.Translate(Vector3.back * Time.deltaTime * speed * vectorAction[++i]);
        }
        
        //  REWARDS
        
        //  filling a pixel on the picture

        //  Don't get out of the canvas
    }
}
