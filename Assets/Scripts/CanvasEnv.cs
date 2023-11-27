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

    //  Matrix of how the environment looks
    public float[] environment = new float[MATRIX_SIZE * MATRIX_SIZE];

    //  Matrix of how the pixel art looks
    public float[] canvas = new float[MATRIX_SIZE * MATRIX_SIZE];

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


    public List<int> TakenTargets = new List<int>();

    [Header("Max Steps")]
    public int MaxEnvironmentSteps;

    private int resetTimer;

    //[HideInInspector]
    public List<int> pictures = new();

    float totalAgentGroupReward = 0;

    float takenHistory = 0;

    const string ImagePath = @"C:\Users\ML-Lab\Desktop\Erik\ML-Agents\Pixel\Images\";

    // Start is called before the first frame update
    void Start()
    {
        GridLocationSpawner();
        InitPixel();

        m_AgentGroup = new SimpleMultiAgentGroup();
        foreach (var agent in pixelAgent)
        {
            // Add to team manager
            m_AgentGroup.RegisterAgent(agent);
        }

        resetTimer = 0;
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
        //m_AgentGroup.AddGroupReward(-0.0001f);

        //SetAgent_XY_Coordinate();

        //for (int i = 0; i < MATRIX_SIZE * MATRIX_SIZE; i++)
        //{
        //    if (agentLocation.Contains(i))                                                         //  agent is at i
        //        environment[i] = 1;
        //    else                                                                                   //  agent is not at i
        //        environment[i] = 0;
        //}

        //for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        //{
        //    agentLocation[i] = pixelAgent[i].gridLocation;

        //    for (int j = 0; j < NUMBER_OF_AGENTS; j++)
        //    {
        //        if (pictures[j] == pixelAgent[i].gridLocation)                                     // if agent is in a picture location
        //            canvas[pictures[j]] = 0;                                                       // change location to zero 0
        //        else if (!agentLocation.Contains(pictures[j]))
        //            canvas[pictures[j]] = 1;                                                       // No other agent is occupying the grid location
        //    }

        //    if (pixelAgent[i].transform.localPosition.y < gridLocation.transform.localPosition.y)    //  pixel falls out
        //    {
        //        resetTimer = 0;

        //        m_AgentGroup.AddGroupReward(-2);

        //        m_AgentGroup.EndGroupEpisode();
        //        InitPixel();
        //    }

        //    float maxTiltAngle = 40f;
        //    float tiltAngle = Vector3.Angle(pixelAgent[i].transform.up, Vector3.up);

        //    if (tiltAngle > maxTiltAngle)                                                          //  if agent is flipped on side or belly up
        //    {
        //        resetTimer = 0;

        //        m_AgentGroup.EndGroupEpisode();
        //        InitPixel();
        //    }
        //}

        //if (TakenTargetLocations() == pictures.Count)
        //{

        //    resetTimer = 0;

        //    m_AgentGroup.AddGroupReward(2);

        //    m_AgentGroup.EndGroupEpisode();
        //    InitPixel();
        //}

        //if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        //{
        //    resetTimer = 0;

        //    m_AgentGroup.GroupEpisodeInterrupted();
        //    InitPixel();
        //}
        //resetTimer += 1;
    }

    //  Spawn all grid locations
    void GridLocationSpawner()
    {
        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            GameObject targetLocation = Instantiate(gridLocation, Vector3.zero, Quaternion.identity);
            targetLocation.tag = i.ToString();
            targetLocation.SetActive(false);
            GridLocation.Add(targetLocation);   //  create and place grid location            //  gameObject
        }
    }

    public void SpawnFromPool(List<int> targetLocations)
    {
        float[] cordListX = new float[MATRIX_SIZE];

        float[] cordListZ = new float[MATRIX_SIZE];

        float startingCoordinatesX = gameObject.transform.position.x + 11.25f;
        float startingCoordinatesZ = gameObject.transform.position.z + 11.25f;

        for (int i = 0; i < MATRIX_SIZE; i++)
        {
            cordListX[i] = startingCoordinatesX;
            startingCoordinatesX -= 2.5f;

            cordListZ[i] = startingCoordinatesZ;
            startingCoordinatesZ -= 2.5f;
        }

        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            int x = 0, z = 0;

            IndexToIndices(MATRIX_SIZE, targetLocations[i], ref x, ref z);

            Vector3 position = new Vector3(cordListX[x], 0.625f, cordListZ[z]);

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
        float[] cordListX = new float[MATRIX_SIZE];

        float[] cordListZ = new float[MATRIX_SIZE];

        float startingCoordinatesX = gameObject.transform.position.x + 11.25f;
        float startingCoordinatesZ = gameObject.transform.position.z + 11.25f;

        for (int i = 0; i < MATRIX_SIZE; i++)
        {
            cordListX[i] = startingCoordinatesX;
            startingCoordinatesX -= 2.5f;

            cordListZ[i] = startingCoordinatesZ;
            startingCoordinatesZ -= 2.5f;
        }

        pictures.Clear();
        SelectImageSet(pictures);           //  Set of 2

        SpawnFromPool(pictures);

        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            pixel_RB[i].transform.position = new Vector3(cordListX[0], 0.625f, cordListZ[i]);

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
        HashSet<int> gridLocationsTaken = new HashSet<int>();
        int takenSpots = 0;
        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            if (pictures.Contains(pixelAgent[i].gridLocation) && !gridLocationsTaken.Contains(pixelAgent[i].gridLocation))
            {
                takenSpots++;
                gridLocationsTaken.Add(pixelAgent[i].gridLocation);
            }
            else
                gridLocationsTaken.Remove(pixelAgent[i].gridLocation);
        }
        gridLocationsTaken.Clear();
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