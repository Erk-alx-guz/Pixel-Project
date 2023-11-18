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

    [HideInInspector]
    public List<int> agentLocation = new();

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
       
        if (EnvironmentReady())  //  The environment must be set up befor doing anything
        {
            //VisualizeImage();

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
    }

    void FixedUpdate()
    {
        m_AgentGroup.AddGroupReward(-0.0001f);

        SetAgent_XY_Coordinate();

        for (int i = 0; i < MATRIX_SIZE * MATRIX_SIZE; i++)
        {
            if (agentLocation.Contains(i))                                                         //  agent is at i
                environment[i] = 1;
            else                                                                                   //  agent is not at i
                environment[i] = 0;

            for (int j = 0; j < NUMBER_OF_AGENTS; j++)
            {
                if (!agentLocation.Contains(pictures[j]))
                    environment[pictures[j]] = -1;
            }
        }

        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            agentLocation[i] = pixelAgent[i].gridLocation;

            if (pixelAgent[i].transform.localPosition.y < gridLocation.transform.localPosition.y)    //  pixel falls out
            {
                resetTimer = 0;

                m_AgentGroup.AddGroupReward(-2);

                m_AgentGroup.EndGroupEpisode();
                SetArrayToZero();
                ResetGridLocations();
                InitPixel();
            }

            float maxTiltAngle = 40f;
            float tiltAngle = Vector3.Angle(pixelAgent[i].transform.up, Vector3.up);

            if (tiltAngle > maxTiltAngle)                                                          //  if agent is flipped on side or belly up
            {
                resetTimer = 0;

                m_AgentGroup.EndGroupEpisode();
                SetArrayToZero();
                ResetGridLocations();
                InitPixel();
            }
        }

        if (TakenTargetLocations() == pictures.Count)
        {

            resetTimer = 0;

            m_AgentGroup.AddGroupReward(2);

            m_AgentGroup.EndGroupEpisode();
            SetArrayToZero();
            ResetGridLocations();
            InitPixel();
        }

        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            resetTimer = 0;

            m_AgentGroup.GroupEpisodeInterrupted();
            SetArrayToZero();
            ResetGridLocations();
            InitPixel();
        }
        resetTimer += 1;
    }

    void SetAgent_XY_Coordinate()
    {
        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            IndexToIndices(MATRIX_SIZE, pixelAgent[i].gridLocation, ref pixelAgent[i].agent_x_coordinate, ref pixelAgent[i].agent_y_coordinate);
        }
    }

    void SetArrayToZero()
    {
        for (int i = 0; i < MATRIX_SIZE * MATRIX_SIZE; i++)
        {
            environment[i] = 0;
        }
    }

    /// <summary>
    /// Used to check the grid is ready
    /// </summary>
    public bool EnvironmentReady()
    {
        return GridLocation.Count == Mathf.Pow(MATRIX_SIZE, 2);
    }

    //  Spawn all grid locations
    void GridLocationSpawner()
    {
        float[] cordListX = new float[MATRIX_SIZE];

        float[] cordListZ = new float[MATRIX_SIZE];

        float startingCoordinatesX = gridLocation.transform.position.x;
        float startingCoordinatesZ = gridLocation.transform.position.z;

        for (int i = 0; i < MATRIX_SIZE; i++)
        {
            cordListX[i] = startingCoordinatesX;
            startingCoordinatesX -= 2.5f;

            cordListZ[i] = startingCoordinatesZ;
            startingCoordinatesZ -= 2.5f;
        }

        int index;
        for (int i = 0; i < MATRIX_SIZE; i++)
        {
            for (int j = 0; j < MATRIX_SIZE; j++)
            {
                index = i * MATRIX_SIZE + j;

                if (i == 0 && j == 0)
                    GridLocation.Add(gridLocation);
                else
                    GridLocation.Add(Instantiate(gridLocation, new Vector3(cordListX[i], gridLocation.transform.position.y, cordListZ[j]), Quaternion.identity));             //  gameObject

                if (index < 10)
                    GridLocation[index].tag = '0' + index.ToString();                                //  assign tag
                else
                    GridLocation[index].tag = index.ToString();
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

        float startingCoordinatesX = gridLocation.transform.localPosition.x;
        float startingCoordinatesZ = gridLocation.transform.localPosition.z;

        for (int i = 0; i < MATRIX_SIZE; i++)
        {
            cordListX[i] = startingCoordinatesX;
            startingCoordinatesX -= 2.5f;

            cordListZ[i] = startingCoordinatesZ;
            startingCoordinatesZ -= 2.5f;
        }

        int x = 0, z = 0;

        agentLocation.Clear();

        //Spawn agent in top left corner
        agentLocation.Add(0);
        agentLocation.Add(1);

        pictures.Clear();
        SelectImageSet(pictures);           //  Set of 2

        //GenerateLocation(pictures);

        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            IndexToIndices(MATRIX_SIZE, agentLocation[i], ref x, ref z);    //  Random Spawn

            pixel_RB[i].transform.localPosition = new Vector3(cordListX[x], gridLocation.transform.localPosition.y, cordListZ[z]);

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
                image.Add(18); 
                image.Add(55);
                break;
            case 2:
                image.Add(21);
                image.Add(90);
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
    /// The convert function:
    /// Maps a small array onto a bigger array
    /// 
    /// </summary>
    /// <param name="smallArrayIndex"></param>
    /// <param name="smallArrayLength"></param>
    /// <param name="bigArrayLenght"></param>
    /// <returns></returns>
    public int Convert(int smallArrayIndex, int smallArrayLength, int bigArrayLenght)
    {
        int currentMatrix_X;
        int currentMatrix_Y;
        int maxMatrix_X;
        int maxMatrix_Y;

        //  The idex of the small 1D array is converted into the indices of a 2D array in the same location
        currentMatrix_X = smallArrayIndex / smallArrayLength;
        currentMatrix_Y = smallArrayIndex - currentMatrix_X * smallArrayLength;

        // Now we make the small array bigger 
        maxMatrix_X = currentMatrix_X + ((bigArrayLenght - smallArrayLength) / 2);
        maxMatrix_Y = currentMatrix_Y + ((bigArrayLenght - smallArrayLength) / 2);

        //  The indices of the big 2D array is now converted into the index of a 1D array
        int bigArrayIndex = maxMatrix_X * bigArrayLenght + maxMatrix_Y;

        return bigArrayIndex;
    }

    void VisualizeImage()
    {
        for (int i = 0; i < MATRIX_SIZE * MATRIX_SIZE; i++)
        {
            if (environment[i] == 0)
                GridLocation[i].GetComponent<Renderer>().material.color = new Color(0, 213, 255, 0.80f);
            else if (environment[i] == -1)
                GridLocation[i].GetComponent<Renderer>().material.color = new Color(255, 225, 0, 1f);
            else
                GridLocation[i].GetComponent<Renderer>().material.color = new Color(255, 0, 0, 0.75f);
        }
    }

    void ResetGridLocations()
    {
        for (int i = 0; i < MATRIX_SIZE * MATRIX_SIZE; i++)
        {
            GridLocation[i].GetComponent<Renderer>().material.color = new Color(0, 0, 0, 0f);
        }
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