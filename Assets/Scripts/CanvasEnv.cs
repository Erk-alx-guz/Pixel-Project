using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;

public class CanvasEnv : MonoBehaviour
{
    //  Scale
    //  40 X 40: cords = -48.75f
    //  20 X 20: cords = -23.75f
    //  10 X 10: cords = -11.25f

    //  xz          xz
    //
    //  ++          +-
    //
    //
    //  -+          --


    const int MAX_MATRIX_SIZE = 40;
    const int CURRENT_MATRIX_SIZE = 10;
    public int small_size = CURRENT_MATRIX_SIZE;
    public int big_size = MAX_MATRIX_SIZE;
    float[] cordList = new float[CURRENT_MATRIX_SIZE];
    const float BOUNDARY = 11.25f;
    float startingCords = BOUNDARY;

    const int NUMBER_OF_AGENTS = 2;

    //  Matrix of how the environment looks
    public float[] environment = new float[MAX_MATRIX_SIZE * MAX_MATRIX_SIZE];

    //  Matrix of how the pixel art looks
    public float[] canvas = new float[MAX_MATRIX_SIZE * MAX_MATRIX_SIZE];

    [HideInInspector]
    public List<int> pictures = new();

    //  Array of the Agent scripts to 
    public PixelAgent[] pixelAgent = new PixelAgent[NUMBER_OF_AGENTS];

    public Rigidbody[] pixel_RB = new Rigidbody[NUMBER_OF_AGENTS];

    public GameObject Spot;

    //  List holding all grid squares
    [HideInInspector]
    public List<GameObject> GridSquares = new List<GameObject>();

    private int resetTimer;

    [Header("Max Steps")]
    [Space(10)]

    public int MaxEnvironmentSteps;

    private SimpleMultiAgentGroup m_AgentGroup;

    public int controlAgent = 0;

    [HideInInspector]
    public List<int> agentLocation = new();

    // Start is called before the first frame update
    void Start()
    {
        //// Generate Picture
        //GenerateLocation(pictures);      
        pictures.Add(34);
        pictures.Add(19);

        //  Set the pixels on the canvas
        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            canvas[pictures[i]] = 1;
        }


        //  Fill the rest of canvas and environment with zeros
        for (int i = 0; i < MAX_MATRIX_SIZE; i++)
        {
            for (int j = 0; j < MAX_MATRIX_SIZE; j++)
            {
                if (canvas[i * MAX_MATRIX_SIZE + j] != 1)
                    canvas[i * MAX_MATRIX_SIZE + j] = 0;

                if (environment[i * MAX_MATRIX_SIZE + j] != 1)
                    environment[i * MAX_MATRIX_SIZE + j] = 0;
            }
        }

        for (int i = 0; i < CURRENT_MATRIX_SIZE; i++)
        {
            cordList[i] = startingCords;
            startingCords -= 2.5f;
        }

        StartCoroutine(SpotDrop());
        InitPixel();

        m_AgentGroup = new SimpleMultiAgentGroup();
        foreach (var agent in pixelAgent)
        {
            // Add to team manager
            m_AgentGroup.RegisterAgent(agent);
        }

        resetTimer = 0;
    }

    //// User input
    //private float speed = 1f;
    //private float horizontalInput;
    //private float forwardInput;

    // Update is called once per frame
    void Update()
    {
        if (EnvironmentReady())  //  The environment must be set up befor doing anything
        {
            VisualizeImage();

            //VisualizePixelTracking();

            FillEnvironment();

            //  Environment reward
            for (int i = 0; i < NUMBER_OF_AGENTS; ++i)
                m_AgentGroup.AddGroupReward(Mathf.Pow((float)TakenSpots() / NUMBER_OF_AGENTS, 2));


            print("taken: " + TakenSpots() + " reward: " + Mathf.Pow((float)TakenSpots() / NUMBER_OF_AGENTS, 2));


            //// This is for player input
            //horizontalInput = Input.GetAxis("Horizontal");
            //forwardInput = Input.GetAxis("Vertical");


            //////  Test that individual pixels could be tacked and moved
            //////  vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

            //pixelAgent[agentCon].transform.Translate(Vector3.right * Time.deltaTime * speed * forwardInput);
            //pixelAgent[agentCon].transform.Translate(Vector3.back * Time.deltaTime * speed * horizontalInput);

            ////print(pixel_RB[agentCon].velocity.x);
            ////  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            ////  Test that individual pixels could be tacked and moved
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            agentLocation[i] = pixelAgent[i].gridLocation;

            if (pictures.Contains(pixelAgent[i].gridLocation))                                                  //  if agent is in a picture location
                canvas[Convert(pixelAgent[i].gridLocation, CURRENT_MATRIX_SIZE, MAX_MATRIX_SIZE)] = 0;          //  change location to zero 0
            else
                canvas[Convert(pixelAgent[i].gridLocation, CURRENT_MATRIX_SIZE, MAX_MATRIX_SIZE)] = 1;
        }

        resetTimer += 1;
        //Debug.Log(resetTimer.ToString());
        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            resetTimer = 0;

            m_AgentGroup.GroupEpisodeInterrupted();
            InitPixel();
        }

        if (OutOfBoundary())
        {
            resetTimer = 0;

            for (int i = 0; i < NUMBER_OF_AGENTS; i++)
            {
                m_AgentGroup.AddGroupReward(-1);

                m_AgentGroup.EndGroupEpisode();
                InitPixel();
            }
        }
    }


    /// <summary>
    /// Used to check if any pixels are outside of the area they are allowd to be in
    /// </summary>
    /// <returns></returns>
    public bool OutOfBoundary()
    {
        //  check all pixels are with in the set area
        //  check pixel transforms

        //  1.235 diff
        //    if pixelObject[0].transform.position.x > 12.485 || pixelObject[0].transform.position.x < -12.485 then pixel is out
        //    if pixelObject[0].transform.position.z > 12.485 || pixelObject[0].transform.position.z < -12.485 then pixel is out
        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            if (pixel_RB[i].transform.position.x > BOUNDARY + 1.235f || pixel_RB[i].transform.position.x < -1 * BOUNDARY - 1.235
                || pixel_RB[i].transform.position.z > BOUNDARY + 1.235f || pixel_RB[i].transform.position.z < -1 * BOUNDARY - 1.235
                || pixel_RB[i].transform.position.y < 0)
            {
                //  then pixel is out
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Used to check the grid is ready
    /// </summary>
    public bool EnvironmentReady()
    {
        return GridSquares.Count == Mathf.Pow(CURRENT_MATRIX_SIZE, 2);
    }

    //  Spawn all grid squares
    IEnumerator SpotDrop()
    {
        int index;
        for (int i = 0; i < CURRENT_MATRIX_SIZE; i++)
        {
            for (int j = 0; j < CURRENT_MATRIX_SIZE; j++)
            {
                index = i * CURRENT_MATRIX_SIZE + j;

                GridSquares.Add(Instantiate(Spot, new Vector3(cordList[i], 0.65f, cordList[j]), Quaternion.identity));             //  gameObject

                if (index < 10)
                {
                    UnityEditorInternal.InternalEditorUtility.AddTag('0' + index.ToString());
                    GridSquares[index].tag = '0' + index.ToString();
                }
                else
                {
                    UnityEditorInternal.InternalEditorUtility.AddTag(index.ToString());
                    GridSquares[index].tag = index.ToString();
                }

                yield return new WaitForSeconds(0f);
            }
        }
    }

    /// <summary>
    /// Spawns pixels in a random position in training mode
    /// and used to place pixels in a particular pattern if testing
    /// </summary>
    void InitPixel()
    {
        Hashtable spotTaken = new Hashtable();
        string key;
        int xPos, zPos;

        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            do
            {
                xPos = UnityEngine.Random.Range(0, CURRENT_MATRIX_SIZE);
                zPos = UnityEngine.Random.Range(0, CURRENT_MATRIX_SIZE);
                key = string.Format("{0:N2}", xPos);
                key += string.Format("{0:N2}", zPos);
            } while (spotTaken[key] != null);           //  check if the location is taken 

            spotTaken[key] = true;

            if (xPos % 2 == 0 && zPos % 2 == 0)
            {
                ToIndex(pictures[i], ref xPos, ref zPos);       //  Fixed spawn
                pixel_RB[i].transform.localPosition = new Vector3(cordList[xPos], 0.125f, cordList[zPos]);
                pixelAgent[i] = pixel_RB[i].GetComponent<PixelAgent>();
            }
            else
            {
                pixel_RB[i].transform.localPosition = new Vector3(cordList[xPos], 0.5f, cordList[zPos]);
                pixelAgent[i] = pixel_RB[i].GetComponent<PixelAgent>();
            }
        }
    }

    /// <summary>
    /// Checks what spots are taken on the picture by pixel agents
    /// </summary>
    /// <returns></returns>
    int TakenSpots()
    {
        HashSet<int> spotTaken = new HashSet<int>();
        int takenSpots = 0;
        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
        {
            for (int j = 0; j < NUMBER_OF_AGENTS; j++)
            {
                if (pictures.Contains(pixelAgent[i].gridLocation) && !spotTaken.Contains(pictures[j]))
                {
                    takenSpots++;
                    spotTaken.Add(pictures[j]);
                }
            }
        }
        spotTaken.Clear();
        return takenSpots;
    }

    /// <summary>
    /// Fill an array of how the canvas looks with all the agents on it
    /// </summary>
    void FillEnvironment()
    {
        for (int i = 0; i < NUMBER_OF_AGENTS; i++)
            environment[Convert(pixelAgent[i].gridLocation, CURRENT_MATRIX_SIZE, MAX_MATRIX_SIZE)] = 1;     //  keep track of what is taken in the bigger array

        for (int j = 0; j < CURRENT_MATRIX_SIZE * CURRENT_MATRIX_SIZE; j++)
        {
            if (environment[Convert(j, CURRENT_MATRIX_SIZE, MAX_MATRIX_SIZE)] != 1) //  it is not taken so set a ZERO
                environment[Convert(j, CURRENT_MATRIX_SIZE, MAX_MATRIX_SIZE)] = 0;
        }
    }

    /// <summary>
    /// 
    /// The convert function:
    /// Maps a small array onto a bigger array
    /// 
    /// </summary>
    /// <param name="smallArrayIndex"></param>
    /// <param name="smallArraySize"></param>
    /// <param name="bigArraySize"></param>
    /// <returns></returns>
    int Convert(int smallArrayIndex, int smallArraySize, int bigArraySize)
    {
        int currentMatrix_X;
        int currentMatrix_Y;
        int maxMatrix_X;
        int maxMatrix_Y;

        //  The idex of the small 1D array is converted into the indices of a 2D array in the same location
        currentMatrix_X = smallArrayIndex / smallArraySize;
        currentMatrix_Y = smallArrayIndex - currentMatrix_X * smallArraySize;

        // Now we make the small array bigger 
        maxMatrix_X = currentMatrix_X + ((bigArraySize - smallArraySize) / 2);
        maxMatrix_Y = currentMatrix_Y + ((bigArraySize - smallArraySize) / 2);

        //  The indices of the big 2D array is now converted into the index of a 1D array
        int bigArrayIndex = maxMatrix_X * bigArraySize + maxMatrix_Y;

        return bigArrayIndex;
    }

    //  Test functions

    /// <summary>
    /// Test function to see how well the pixels are being tracked
    /// </summary>
    void VisualizePixelTracking()
    {
        HashSet<int> agent_loc = new HashSet<int>();
        //for (int i = 0; i < CURRENT_MATRIX_SIZE * CURRENT_MATRIX_SIZE; i++)
        //{
        //    if (Spots[i].taken)   // (canvas[i] == 1)
        //        GridSquares[i].GetComponent<Renderer>().material.color = new Color(255, 255, 0, 0.75f);                                 //  Where the pixel is 
        //    else// not taken
        //        GridSquares[i].GetComponent<Renderer>().material.color = new Color(0, 255, 255, 0.75f);                                 //  Where there are no pixels or targets
        //}

        agent_loc.Clear();

        for (int i = 0; i < NUMBER_OF_AGENTS; ++i)
        {
            GridSquares[pixelAgent[i].gridLocation].GetComponent<Renderer>().material.color = new Color(255, 255, 0, 0.75f);
            agent_loc.Add(pixelAgent[i].gridLocation);
        }

        for (int i = 0; i < CURRENT_MATRIX_SIZE * CURRENT_MATRIX_SIZE; i++)
        {
            if (!agent_loc.Contains(i))
                GridSquares[i].GetComponent<Renderer>().material.color = new Color(0, 255, 255, 0.75f);   //    not taken                  //  Where there are no pixels or targets
        }


        int maxIndex = (MAX_MATRIX_SIZE - CURRENT_MATRIX_SIZE) / 2;
        for (int i = 0; i < CURRENT_MATRIX_SIZE; i++)
        {
            for (int j = 0; j < CURRENT_MATRIX_SIZE; j++)
            {
                if (canvas[(maxIndex + i) * MAX_MATRIX_SIZE + maxIndex + j] == 1)
                    GridSquares[i * CURRENT_MATRIX_SIZE + j].GetComponent<Renderer>().material.color = new Color(255, 0, 0, 0.75f);     //  Where the targets are
            }
        }
    }

    void VisualizeImage()
    {
        //  (BigArray - SmallArray) / 2 = Starting_Point_Index_For_Small_2DArray

        int maxIndex = (MAX_MATRIX_SIZE - CURRENT_MATRIX_SIZE) / 2;
        for (int i = 0; i < CURRENT_MATRIX_SIZE; i++)
        {
            for (int j = 0; j < CURRENT_MATRIX_SIZE; j++)
            {
                if (canvas[(maxIndex + i) * MAX_MATRIX_SIZE + maxIndex + j] == 1)
                    GridSquares[i * CURRENT_MATRIX_SIZE + j].GetComponent<Renderer>().material.color = new Color(255, 0, 0, 0.75f);
            }
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
    void ToIndex(int index, ref int x, ref int y)
    {
        string snum = index.ToString();

        if (snum.Length == 1)
            x = snum[0] - 48;
        else if (snum.Length == 2)
        {
            x = snum[0] - 48;
            y = snum[1] - 48;
        }
    }
}



//  self assembly 



