using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;

public class CanvasEnv : MonoBehaviour
{
    const int MATRIX_SIZE = 10;

    const int NUMBER_OF_AGENTS = 2;

    //  Array of the Agent scripts to 
    [Header("Agent List")]
    public PixelAgent[] pixelAgent = new PixelAgent[NUMBER_OF_AGENTS];

    public Rigidbody[] pixel_RB = new Rigidbody[NUMBER_OF_AGENTS];

    private SimpleMultiAgentGroup m_AgentGroup;

    public int controlAgent = 0;

    [Header("Grid Location")]
    public GameObject gridLocation;

    //  List holding all grid locations
    [HideInInspector]
    public List<GameObject> GridLocation = new List<GameObject>();

    [Header("Max Steps")]
    public int MaxEnvironmentSteps;

    private int resetTimer;

    //[HideInInspector]
    public List<int> pictures = new();

    float totalAgentGroupReward = 0;

    float takenHistory = 0;

    float[] cordListX = new float[MATRIX_SIZE];

    float[] cordListZ = new float[MATRIX_SIZE];

    float startingCoordinatesX;
    float startingCoordinatesZ;

    const float canvasSizeConst = 11.29f;

    // Start is called before the first frame update
    void Start()
    {
        startingCoordinatesX = gameObject.transform.position.x + canvasSizeConst;
        startingCoordinatesZ = gameObject.transform.position.z + canvasSizeConst;

        for (int i = 0; i < MATRIX_SIZE; i++)
        {
            cordListX[i] = startingCoordinatesX;
            startingCoordinatesX -= 2.5f;

            cordListZ[i] = startingCoordinatesZ;
            startingCoordinatesZ -= 2.5f;
        }

        GridLocationSpawner();
        ResetEnvironment();

        m_AgentGroup = new SimpleMultiAgentGroup();
        foreach (var agent in pixelAgent)
        {
            // Add to team manager
            m_AgentGroup.RegisterAgent(agent);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float dif = 0;

        dif = Mathf.Pow((float)TakenTargetLocations() / NUMBER_OF_AGENTS, 2) - totalAgentGroupReward;

        if (takenHistory < TakenTargetLocations())
        {
            takenHistory = TakenTargetLocations();

            totalAgentGroupReward += dif;

            for (int i = 0; i < NUMBER_OF_AGENTS; ++i)
                m_AgentGroup.AddGroupReward(dif);
        }
        else if (takenHistory > TakenTargetLocations())
        {
            takenHistory = TakenTargetLocations();

            totalAgentGroupReward += dif;

            for (int i = 0; i < NUMBER_OF_AGENTS; ++i)
                m_AgentGroup.AddGroupReward(dif);
        }
    }

    void FixedUpdate()
    {
        m_AgentGroup.AddGroupReward(-0.0001f);

        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            //  normalize location from 0 - 1
            pixelAgent[i].agent_x_coordinate = Math.Abs(pixelAgent[i].transform.position.x - canvasSizeConst) / (canvasSizeConst * 2);
            pixelAgent[i].agent_z_coordinate = Math.Abs(pixelAgent[i].transform.position.z - canvasSizeConst) / (canvasSizeConst * 2);

            if (pixelAgent[i].transform.position.y < gameObject.transform.position.y)    //  pixel falls out
            {
                m_AgentGroup.AddGroupReward(-2);

                m_AgentGroup.EndGroupEpisode();
                ResetEnvironment();
            }

            float maxTiltAngle = 40f;
            float tiltAngle = Vector3.Angle(pixelAgent[i].transform.up, Vector3.up);

            if (tiltAngle > maxTiltAngle)                                                          //  if agent is flipped on side or belly up
            {
                m_AgentGroup.EndGroupEpisode();
                ResetEnvironment();
            }
        }

        if (TakenTargetLocations() == pictures.Count)
        {
            m_AgentGroup.AddGroupReward(2);

            m_AgentGroup.EndGroupEpisode();
            ResetEnvironment();
        }

        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_AgentGroup.GroupEpisodeInterrupted();
            ResetEnvironment();
        }
        resetTimer += 1;
    }

    void ResetEnvironment()
    {
        resetTimer = 0;

        for (int i = 0; i < GridLocation.Count; i++)
        {
            GridLocation[i].SetActive(false);
        }

        pictures.Clear();
        SelectImageSet(pictures);           //  Set of 2
        SpawnFromPool(pictures);
        InitPixel();
    }

    //  Spawn all grid locations
    void GridLocationSpawner()
    {
        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            GameObject targetLocation = Instantiate(gridLocation, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 0.625f, gameObject.transform.position.z), Quaternion.identity);
            targetLocation.tag = i.ToString();
            targetLocation.SetActive(false);
            GridLocation.Add(targetLocation);   //  create and place grid location            //  gameObject
        }
    }

    public void SpawnFromPool(List<int> targetLocations)
    {
        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            int x = 0, z = 0;

            IndexToIndices(MATRIX_SIZE, targetLocations[i], ref x, ref z);

            Vector3 position = new Vector3(cordListX[x], gameObject.transform.position.y + 0.625f, cordListZ[z]);

            if (!GridLocation[i].activeInHierarchy)
            {
                GridLocation[i].transform.position = position;
                GridLocation[i].transform.rotation = Quaternion.identity;
                GridLocation[i].SetActive(true);
            }
        }
    }

    /// <summary>
    /// Spawns pixels in a random position in training mode
    /// and used to place pixels in a particular pattern if testing
    /// </summary>
    void InitPixel()
    {
        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            //  index for x should be 0
            pixel_RB[i].transform.position = new Vector3(cordListX[0], gameObject.transform.position.y + 0.625f, cordListZ[i]);

            pixelAgent[i] = pixel_RB[i].GetComponent<PixelAgent>();
        }
    }

    void SelectImageSet(List<int> image)
    {
        image.Clear();

        int select;

        select = UnityEngine.Random.Range(1, 3);

        switch (select)
        {
            case 1:
                image.Add(65); 
                image.Add(42);
                break;
            case 2:
                image.Add(19);
                image.Add(34);
                break;
        }
    }

    /// <summary>
    /// Generate random location for agent to spawned or generate target location for image
    /// </summary>
    /// <param name="locations"></param>
    void GenerateLocation(List<int> locations)
    {
        locations.Clear();
        int xPos, zPos;

        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            do
            {
                xPos = UnityEngine.Random.Range(0, MATRIX_SIZE);
                zPos = UnityEngine.Random.Range(0, MATRIX_SIZE);
            } while (locations.Contains(xPos * MATRIX_SIZE + zPos));           //  check if the location is taken 

            locations.Add(xPos * MATRIX_SIZE + zPos);
        }
    }

    /// <summary>
    /// Checks what spots are taken on the picture by pixel agents
    /// </summary>
    /// <returns></returns>
    int TakenTargetLocations()
    {
        int takenSpots = 0;
        for (int i = 0; i < GridLocation.Count; i++)
        {
            if (!GridLocation[i].activeInHierarchy)
                takenSpots++;
        }
        return takenSpots;
    }

    /// <summary>
    /// 
    /// Converts single index of a 1D array 
    /// into two x and y indices of a 2D array
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void IndexToIndices(int size, int index, ref int x, ref int y)
    {
        x = index / size;
        y = index - x * size;
    }
}