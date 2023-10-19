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

    [Header("Grid Square")]
    public GameObject gridSquare;

    //  List holding all grid squares
    [HideInInspector]
    public List<GameObject> GridSquares = new List<GameObject>();

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
        pictures.Add(34);
        pictures.Add(19);

        GridSquareDrop();
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
            VisualizeImage();

            //m_AgentGroup.AddGroupReward(Mathf.Pow((float)TakenGridSquares() / NUMBER_OF_AGENTS, 2));

            dif = Mathf.Pow((float)TakenGridSquares() / NUMBER_OF_AGENTS, 2) - totalAgentGroupReward;

            if (takenHistory < TakenGridSquares())
            {
                takenHistory = TakenGridSquares();

                totalAgentGroupReward += dif;

                for (int i = 0; i < NUMBER_OF_AGENTS; ++i)
                    m_AgentGroup.AddGroupReward(dif);
            }
            else if (takenHistory > TakenGridSquares())
            {
                takenHistory = TakenGridSquares();

                totalAgentGroupReward += dif;

                for (int i = 0; i < NUMBER_OF_AGENTS; ++i)
                    m_AgentGroup.AddGroupReward(dif);
            }
        }
    }

    void FixedUpdate()
    {
        SetAgent_XY_Coordinate();

        //  NOTE THIS ONLY WORKS BECAUSE MAX AND CURRENT ARE THE SAME SIZE
        //  vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

        for (int i = 0; i < MATRIX_SIZE * MATRIX_SIZE; i++)
        {
            if (agentLocation.Contains(i))                     //  agent is at i               //  to make it work when max and curruent are different make an agen location list holding max index
                environment[i] = 1;
            else                                                    //  agent is not at i
                environment[i] = 0;
        }

        // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        //  NOTE THIS ONLY WORKS BECAUSE MAX AND CURRENT ARE THE SAME SIZE

        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            agentLocation[i] = pixelAgent[i].gridLocation;

            for (int j = 0; j < NUMBER_OF_AGENTS; j++)
            {
                if (pictures[j] == pixelAgent[i].gridLocation)                                       // if agent is in a picture location
                    canvas[pictures[j]] = 0;          // change location to zero 0
                else if (!agentLocation.Contains(pictures[j]))
                    canvas[pictures[j]] = 1;          // No other agent is occupying the grid square
            }
        }

        if (TakenGridSquares() == pictures.Count)
        {
            //StartCoroutine(ScreenShot());

            resetTimer = 0;

            m_AgentGroup.AddGroupReward(NUMBER_OF_AGENTS);

            m_AgentGroup.EndGroupEpisode();
            SetArrayToZero();
            ResetGridSquares();
            InitPixel();
        }

        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            //ScreenCapture.CaptureScreenshot(ImagePath + "PixelOutOfTime " + dateTime + ".png");

            resetTimer = 0;

            m_AgentGroup.GroupEpisodeInterrupted();
            SetArrayToZero();
            ResetGridSquares();
            InitPixel();
        }

        for (int i = 0; i < NUMBER_OF_AGENTS; ++i)
        {
            if (pixelAgent[i].transform.localPosition.y < gridSquare.transform.localPosition.y)   //  pixel falls out
            {
                resetTimer = 0;

                m_AgentGroup.AddGroupReward(-2);

                m_AgentGroup.EndGroupEpisode();
                SetArrayToZero();
                ResetGridSquares();
                InitPixel();
            }

            float maxTiltAngle = 40f;
            float tiltAngle = Vector3.Angle(pixelAgent[i].transform.up, Vector3.up);

            if (tiltAngle > maxTiltAngle)     //  if agent is flipped on side or belly up
            {
                resetTimer = 0;

                m_AgentGroup.EndGroupEpisode();
                SetArrayToZero();
                ResetGridSquares();
                InitPixel();
            }
        }
        resetTimer += 1;
    }

    //IEnumerator ScreenShot()
    //{
    //    print("befor");

    //    DateTime now = DateTime.Now;

    //    string dateTime = now.ToString();

    //    dateTime = dateTime.Replace('/', '-');

    //    dateTime = dateTime.Replace(":", "_");

    //    ScreenCapture.CaptureScreenshot(ImagePath + "PixelImageSuccessful " + dateTime + ".png");

    //    yield return new WaitForSeconds(3);

    //    m_AgentGroup.EndGroupEpisode();
    //    SetArrayToZero();
    //    ResetGridSquares();
    //    InitPixel();

    //    print("after");
    //}

    void SetAgent_XY_Coordinate()
    {
        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            ToIndex(MATRIX_SIZE, pixelAgent[i].gridLocation, ref pixelAgent[i].agent_x_coordinate, ref pixelAgent[i].agent_y_coordinate);
        }
    }

    void SetArrayToZero()
    {
        for (int i = 0; i < MATRIX_SIZE * MATRIX_SIZE; i++)
        {
            canvas[i] = 0;
        }
    }

    /// <summary>
    /// Used to check the grid is ready
    /// </summary>
    public bool EnvironmentReady()
    {
        return GridSquares.Count == Mathf.Pow(MATRIX_SIZE, 2);
    }

    //  Spawn all grid squares
    void GridSquareDrop()
    {
        float[] cordListX = new float[MATRIX_SIZE];

        float[] cordListZ = new float[MATRIX_SIZE];

        float startingCoordinatesX = gridSquare.transform.position.x;
        float startingCoordinatesZ = gridSquare.transform.position.z;

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
                    GridSquares.Add(gridSquare);
                else
                    GridSquares.Add(Instantiate(gridSquare, new Vector3(cordListX[i], gridSquare.transform.position.y, cordListZ[j]), Quaternion.identity));             //  gameObject

                if (index < 10)
                {
                    UnityEditorInternal.InternalEditorUtility.AddTag('0' + index.ToString());       //  creat tag
                    GridSquares[index].tag = '0' + index.ToString();                                //  assign tag
                }   
                else
                {
                    UnityEditorInternal.InternalEditorUtility.AddTag(index.ToString());
                    GridSquares[index].tag = index.ToString();
                }
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

        float startingCoordinatesX = gridSquare.transform.localPosition.x;
        float startingCoordinatesZ = gridSquare.transform.localPosition.z;

        for (int i = 0; i < MATRIX_SIZE; i++)
        {
            cordListX[i] = startingCoordinatesX;
            startingCoordinatesX -= 2.5f;

            cordListZ[i] = startingCoordinatesZ;
            startingCoordinatesZ -= 2.5f;
        }

        int x = 0, z = 0;

        agentLocation.Clear();

        GenerateLocation(agentLocation);

        //pictures.Clear();
        ////Generate Picture
        //GenerateLocation(pictures);

        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            if (UnityEngine.Random.Range(0, 4) == 0)
                ToIndex(MATRIX_SIZE, pictures[i], ref x, ref z);       //  Fixed spawn
            else
                ToIndex(MATRIX_SIZE, agentLocation[i], ref x, ref z);    //  Random Spawn

            pixel_RB[i].transform.localPosition = new Vector3(cordListX[x], gridSquare.transform.localPosition.y, cordListZ[z]);

            pixelAgent[i] = pixel_RB[i].GetComponent<PixelAgent>();
        }
    }

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
    int TakenGridSquares()
    {
        HashSet<int> gridSquaresTaken = new HashSet<int>();
        int takenSpots = 0;
        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            if (pictures.Contains(pixelAgent[i].gridLocation) && !gridSquaresTaken.Contains(pixelAgent[i].gridLocation))
            {
                takenSpots++;
                gridSquaresTaken.Add(pixelAgent[i].gridLocation);
            }
            else
                gridSquaresTaken.Remove(pixelAgent[i].gridLocation);
        }
        gridSquaresTaken.Clear();
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
        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            if (canvas[pictures[i]] == 1)
                GridSquares[pictures[i]].GetComponent<Renderer>().material.color = new Color(255, 0, 0, 0.75f);
            else
                GridSquares[pictures[i]].GetComponent<Renderer>().material.color = new Color(0, 0, 0, 0f);
        }
    }

    void ResetGridSquares()
    {
        for (int i = 0; i < MATRIX_SIZE * MATRIX_SIZE; i++)
        {
            GridSquares[i].GetComponent<Renderer>().material.color = new Color(0, 0, 0, 0f);
        }
    }

    /// <summary>
    /// This function converts the locations of the shape in pictures into 
    /// two indexes that can be used to spawnd the agents in the shape chosen
    /// in the pictures array.
    /// 
    /// This is only a test function
    /// </summary>
    /// <param name="index"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void ToIndex(int size, int index, ref int x, ref int y)
    {
        x = index / size;
        y = index - x * size;
    }
}