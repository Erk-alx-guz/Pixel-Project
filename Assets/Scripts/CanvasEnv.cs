using System.Collections;
using System.Collections.Generic;
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
    const float boundary = 11.25f;
    float startingCords = boundary;

    const int numAgents = 2;

    //  Matrix of how the environment looks
    public float[] environment = new float[MAX_MATRIX_SIZE * MAX_MATRIX_SIZE];

    //  pictures
    int pictureIndex = 5;

    //  Matrix of how the pixel art looks
    public float[] canvas = new float[MAX_MATRIX_SIZE * MAX_MATRIX_SIZE];

    int[,] pictures = { { 03, 59}       
                      , { 47, 09}   
                      , { 51, 34}    
                      , { 11, 43}    
                      , { 06, 60}    
                      , { 34, 19}     
                      , { 60, 36}    
                      , { 27, 28} };


    //  Array of the Agent scripts to 
    public PixelAgent[] pixelAgent = new PixelAgent[numAgents];

    public Rigidbody[] pixel_RB = new Rigidbody[numAgents];

    public GameObject Spot;

    //  List holding all grid squares

    List<GameObject> GridSquares = new List<GameObject>();

    InSpot[] Spots = new InSpot[CURRENT_MATRIX_SIZE * CURRENT_MATRIX_SIZE];

    private int resetTimer;

    [Header("Max Steps")]
    [Space(10)]

    public int MaxEnvironmentSteps;

    // Start is called before the first frame update
    void Start()
    {
        //  Set the pixels on the canvas
        for (int i = 0; i < numAgents; i++)
        {
            canvas[Convert(pictures[pictureIndex, i], CURRENT_MATRIX_SIZE, MAX_MATRIX_SIZE)] = 1;
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
        
        resetTimer = 0;
    }

    //// User input
    //private float speed = 0.1f;
    //private float horizontalInput;
    //private float forwardInput;

    // Update is called once per frame
    void Update()
    {
        if (EnvironmentReady())  //  The environment must be set up befor doing anything
        {
            VisualizeImage();

            FillEnvironment();

            //  Environment reward
            for (int i = 0; i < numAgents; ++i)
                pixelAgent[i].AddReward(Mathf.Pow((float)TakenSpots() / numAgents, 2));

            //// This is for player input
            //horizontalInput = Input.GetAxis("Horizontal");
            //forwardInput = Input.GetAxis("Vertical");


            ////  Test that individual pixels could be tacked and moved
            ////  vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

            ////if (Pixels.Count > 2 && Pixels.Count == pixelCount) //  Making sure the pixel exists and don't let it move until all pixels have been spawned
            //pixelAgent[0].transform.Translate(Vector3.right * Time.deltaTime * speed * forwardInput);
            //pixelAgent[0].transform.Translate(Vector3.back * Time.deltaTime * speed * horizontalInput);

            //print(pixel_RB[0].velocity.x);
            ////  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            ////  Test that individual pixels could be tacked and moved
        }
    }

    void FixedUpdate()
    {
        resetTimer += 1;
        //Debug.Log(resetTimer.ToString());
        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            resetTimer = 0;

            for (int i = 0; i < numAgents; i++)
                pixelAgent[i].EndEpisode();


            InitPixel();
        }

        if (OutOfBoundary())
        {

            for (int i = 0; i < numAgents; i++)
            {
                pixelAgent[i].AddReward(-10f);
                pixelAgent[i].EndEpisode();

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
        for (int i = 0; i < numAgents; i++)
        {
            if (pixel_RB[i].transform.position.x > boundary + 1.235f || pixel_RB[i].transform.position.x < -1 * boundary - 1.235
                || pixel_RB[i].transform.position.z > boundary + 1.235f || pixel_RB[i].transform.position.z < -1 * boundary - 1.235
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
        for (int i = 0; i < CURRENT_MATRIX_SIZE; i++)
        {
            for (int j = 0; j < CURRENT_MATRIX_SIZE; j++)
            {
                GridSquares.Add(Instantiate(Spot, new Vector3(cordList[i], 0.125f, cordList[j]), Quaternion.identity));
                Spots[i * CURRENT_MATRIX_SIZE + j] = GridSquares[i * CURRENT_MATRIX_SIZE + j].GetComponent<InSpot>();

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

        for (int i = 0; i < numAgents; i++)
        {
            do
            {
                xPos = Random.Range(0, CURRENT_MATRIX_SIZE);
                zPos = Random.Range(0, CURRENT_MATRIX_SIZE);
                key = string.Format("{0:N2}", xPos);
                key += string.Format("{0:N2}", zPos);
            } while (spotTaken[key] != null);           //  check if the location is taken 

            spotTaken[key] = true;

            pixel_RB[i].transform.localPosition = new Vector3(cordList[xPos], 0.125f, cordList[zPos]);
            pixelAgent[i] = pixel_RB[i].GetComponent<PixelAgent>();


            ////Test mode
            //ToIndex(pictures[pictureIndex, i], ref xPos, ref zPos);
            //pixelObject[i].transform.localPosition = new Vector3(cordList[xPos], 0.125f, cordList[zPos]);
            //pixelAgent[i] = pixelObject[i].GetComponent<PixelAgent>();
        }
    }

    /// <summary>
    /// Checks what spots are taken on the picture by pixel agents
    /// </summary>
    /// <returns></returns>
    int TakenSpots()
    {
        int takenSpots = 0;
        for (int i = 0; i < numAgents; i++)
        {
            if (Spots[pictures[pictureIndex,i]].taken)
                takenSpots++;
        }
        return takenSpots;
    }

    /// <summary>
    /// Fill an array of how the canvas looks with all the agents on it
    /// </summary>
    void FillEnvironment()
    {
        for (int i = 0; i < CURRENT_MATRIX_SIZE * CURRENT_MATRIX_SIZE; i++)
        {
            if (Spots[i].taken)                                                            //  Check if the a grid square is taken in the smaller array
                environment[Convert(i, CURRENT_MATRIX_SIZE, MAX_MATRIX_SIZE)] = 1;         //  keep track of what is taken in the bigger array
            else                                                                           //  it is not taken so set a ZERO
                environment[Convert(i, CURRENT_MATRIX_SIZE, MAX_MATRIX_SIZE)] = 0;
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
        for (int i = 0; i < CURRENT_MATRIX_SIZE * CURRENT_MATRIX_SIZE; i++)
        {
            if (Spots[i].taken)   // (canvas[i] == 1)
                GridSquares[i].GetComponent<Renderer>().material.color = new Color(255, 0, 0);
            else// if (canvas[i] == 0)
                GridSquares[i].GetComponent<Renderer>().material.color = new Color(0, 255, 255);
        }

        int maxIndex = (MAX_MATRIX_SIZE - CURRENT_MATRIX_SIZE) / 2;
        for (int i = 0; i < CURRENT_MATRIX_SIZE; i++)
        {
            for (int j = 0; j < CURRENT_MATRIX_SIZE; j++)
            {
                if (canvas[(maxIndex + i) * MAX_MATRIX_SIZE + maxIndex + j] == 1)
                    GridSquares[i * CURRENT_MATRIX_SIZE + j].GetComponent<Renderer>().material.color = new Color(255, 0, 0);
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
                    GridSquares[i * CURRENT_MATRIX_SIZE + j].GetComponent<Renderer>().material.color = new Color(255, 0, 0);
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



