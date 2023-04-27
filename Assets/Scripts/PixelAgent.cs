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

    public override void Initialize()
    {
        pixel = gameObject;

        //m_BallRb = Ball.GetComponent<Rigidbody>();

        //m_JdController = GetComponent<JointDriveController>();

        //m_JdController.SetupBodyPart(Upper);
        //m_JdController.SetupBodyPart(Lower);
    }

    public override void OnEpisodeBegin()
    {




        ////Reset all of the body parts
        //foreach (var bodyPart in m_JdController.bodyPartsDict.Values)
        //{
        //    bodyPart.Reset(bodyPart);
        //}

        //m_BallRb.velocity = new Vector3(0f, 0f, 0f);
        //m_BallRb.angularVelocity = new Vector3(0f, 0f, 0f);
        //m_BallRb.transform.position = new Vector3(0f, 4f, Random.Range(-7.4f, 7.4f)) + Table.transform.position;
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
        //  Don't do anything untill the environment is ready
        if (env.ready)
        {

        }




        //var vectorAction = actionBuffers.ContinuousActions;

        //var bpDict = m_JdController.bodyPartsDict;
        //var i = -1;

        //bpDict[Upper].SetJointTargetRotation(vectorAction[++i], 0, 0);
        //bpDict[Lower].SetJointTargetRotation(vectorAction[++i], 0, 0);

        //bpDict[Upper].SetJointStrength(vectorAction[++i]);
        //bpDict[Lower].SetJointStrength(vectorAction[++i]);

        //AddReward((float)(5.0f / Math.Pow(2.0f, Math.Abs(Table.transform.position.z - Ball.transform.position.z))));
        //AddReward((float)(1.0f / Math.Pow(2.0f, Math.Abs(m_BallRb.velocity.z))));
        //AddReward((float)(1.0f / Math.Pow(2.0f, Math.Abs(m_BallRb.velocity.y))));
        //AddReward((float)(-1 * Math.Abs(m_BallRb.velocity.x)));
        //AddReward((float)(1.0f / Math.Pow(2.0f, Math.Abs(Table.transform.rotation.x))));
        //AddReward((float)(1.0f / Math.Pow(2.0f, Math.Abs(Upper.transform.rotation.x))));
        //AddReward((float)(1.0f / Math.Pow(2.0f, Math.Abs(Lower.transform.rotation.x))));
    }
}
