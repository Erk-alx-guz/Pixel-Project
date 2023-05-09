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


    const int ENV_MATRIX_SIZE = 40;
    const int CANVAS_SIZE = 36;
    public int size = CANVAS_SIZE;
    float[] cordList = new float[CANVAS_SIZE];
    const float boundary = 43.75f;
    float startingCords = boundary;

    const int numAgents = 8;

    //  Matrix of how the environment looks
    public float[] environment = new float[ENV_MATRIX_SIZE * ENV_MATRIX_SIZE];

    //  pictures
    int pictureIndex = 5;

    //  Matrix of how the pixel art looks
    public float[] canvas = new float[ENV_MATRIX_SIZE * ENV_MATRIX_SIZE];

    int[,] pictures = { { 03, 11, 19, 27, 35, 43, 51, 59}       //  Line
                      , { 18, 19, 20, 26, 28, 34, 35, 36}       //  Square
                      , { 19, 26, 28, 33, 34, 35, 36, 37}       //  Triangle
                      , { 11, 19, 26, 27, 28, 35, 43, 51}       //  Cross
                      , { 19, 20, 26, 29, 34, 37, 43, 44}       //  Invers square
                      , { 19, 26, 28, 34, 35, 36, 42, 44}       //  Letter A
                      , { 18, 21, 27, 28, 35, 36, 42, 45}       //  Invers of Invers square
                      , { 18, 26, 27, 28, 34, 36, 42, 44} };    //  Letter n


    //  Array of the Agent scripts to 
    public PixelAgent[] pixelAgent = new PixelAgent[numAgents];

    public GameObject[] pixelObject = new GameObject[numAgents];

    public GameObject Spot;

    //  List holding all grid squares

    List<GameObject> GridSquares = new List<GameObject>();

    InSpot[] Spots = new InSpot[CANVAS_SIZE * CANVAS_SIZE];

    // Start is called before the first frame update
    void Start()
    {
        //  Set the pixels on the canvas
        for (int i = 0; i < numAgents; i++)
        {
            int pictureIndex_X = 0;
            int pictureIndex_Y = 0;
            int lastCol = numAgents - 1;

            //  This for loop is looking for x 
            for (int j = 0; j < numAgents; j++)
            {
                if (pictures[pictureIndex, i] < lastCol)
                {
                    pictureIndex_X = j;
                    break;
                }
                lastCol += 8;
            }
            pictureIndex_Y = pictures[pictureIndex, i] - pictureIndex_X * numAgents;

            // (40 - 8)/ 2
            int canvasIndex_X = pictureIndex_X + ((ENV_MATRIX_SIZE - numAgents) / 2);
            int canvasIndex_Y = pictureIndex_Y + ((ENV_MATRIX_SIZE - numAgents) / 2);

            int canvasIndex = canvasIndex_X * ENV_MATRIX_SIZE + canvasIndex_Y;

            canvas[canvasIndex] = 1;
        }


        //  Fill the rest of the canvas with zeros
        for (int i = 0; i < ENV_MATRIX_SIZE; i++)
        {
            for (int j = 0; j < ENV_MATRIX_SIZE; j++)
            {
                if (canvas[i * ENV_MATRIX_SIZE + j] != 1)
                    canvas[i * ENV_MATRIX_SIZE + j] = 0;

                //print(i * ENV_MATRIX_SIZE + j + ": " + canvas[i * ENV_MATRIX_SIZE + j]);
            }
        }

        for (int i = 0; i < CANVAS_SIZE; i++)
        {
            cordList[i] = startingCords;
            startingCords -= 2.5f;

            //print(cordList[i]);
        }

        StartCoroutine(SpotDrop());
        InitPixel();
    }


    //  Spawn all grid squares
    IEnumerator SpotDrop()
    {
        for (int i = 0; i < CANVAS_SIZE; i++)
        {
            for (int j = 0; j < CANVAS_SIZE; j++)
            {
                GridSquares.Add(Instantiate(Spot, new Vector3(cordList[i], 0.125f, cordList[j]), Quaternion.identity));
                Spots[i * CANVAS_SIZE + j] = GridSquares[i * CANVAS_SIZE + j].GetComponent<InSpot>();

                yield return new WaitForSeconds(0f);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (EnvironmentReady())  //  The environment must be set up befor doing anything
        {
            //VisualizeImage();


            //  Environment reward
            for (int i = 0; i < numAgents; ++i)
                pixelAgent[i].AddReward(Mathf.Pow((float)TakenSpots() / (float)numAgents, 2));

            //print(Mathf.Pow((float) TakenSpots() / (float) numAgents, 2));


            //  Test outside of boundary
            print(OutOfBoundary());
        }
    }


    /// <summary>
    /// Test function to see how well the pixels are being tracked
    /// </summary>
    void VisualizePixelTracking()
    {
        for (int i = 0; i < CANVAS_SIZE * CANVAS_SIZE; i++)
        {
            if (Spots[i].taken)   // (canvas[i] == 1)
                GridSquares[i].GetComponent<Renderer>().material.color = new Color(255, 0, 0);
            else// if (canvas[i] == 0)
                GridSquares[i].GetComponent<Renderer>().material.color = new Color(0, 255, 255);
        }
    }

    void VisualizeImage()
    {
        //  (BigArray - SmallArray) / 2 = Starting_Point_Index_For_Small_2DArray

        int maxIndex = (ENV_MATRIX_SIZE - 8) / 2;
        int curIndex = (CANVAS_SIZE - 8) / 2;
        for (int i = 0; i < CANVAS_SIZE; i++)
        {
            for (int j = 0; j < CANVAS_SIZE; j++)
            {
                if (canvas[(maxIndex + i - curIndex) * ENV_MATRIX_SIZE + maxIndex + (j - curIndex)] == 1)//(Spots[i].taken)   // 
                    GridSquares[i * CANVAS_SIZE + j].GetComponent<Renderer>().material.color = new Color(255, 0, 0);
                else// if (canvas[i] == 0)
                    GridSquares[i * CANVAS_SIZE + j].GetComponent<Renderer>().material.color = new Color(0, 255, 255);
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
                xPos = Random.Range(0, CANVAS_SIZE);
                zPos = Random.Range(0, CANVAS_SIZE);
                key = string.Format("{0:N2}", xPos);
                key += string.Format("{0:N2}", zPos);
            } while (spotTaken[key] != null);           //  check if the location is taken 

            spotTaken[key] = true;

            pixelObject[i].transform.localPosition = new Vector3(cordList[xPos], 0.125f, cordList[zPos]);
            pixelAgent[i] = pixelObject[i].GetComponent<PixelAgent>();

            ////Test mode
            //ToIndex(pictures[pictureIndex, i], ref xPos, ref zPos);
            //pixelObject[i].transform.localPosition = new Vector3(cordList[xPos], 0.125f, cordList[zPos]);
            //pixelAgent[i] = pixelObject[i].GetComponent<PixelAgent>();
        }
    }


    /// <summary>
    /// Used to check the grid is ready
    /// </summary>
    public bool EnvironmentReady()
    {
        return GridSquares.Count == Mathf.Pow(CANVAS_SIZE, 2);
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
    /// This function converts the locations of the shape in pictures into 
    /// two indexes that can be used to spawnd the agents in the shape chosen
    /// in the pictures array.
    /// 
    /// This is only a test function
    /// </summary>
    /// <param name="index"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    void ToIndex(int index,ref int x,ref int y)
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


    /// <summary>
    /// Used to check if any pixels are outside of the area they are allowd to be in
    /// </summary>
    /// <returns></returns>
    bool OutOfBoundary()
    {
        //  check all pixels are with in the set area
        //  check pixel transforms

        //  1.235 diff
        //    if pixelObject[0].transform.position.x > 12.485 || pixelObject[0].transform.position.x < -12.485 then pixel is out
        //    if pixelObject[0].transform.position.z > 12.485 || pixelObject[0].transform.position.z < -12.485 then pixel is out
        for (int i = 0; i < numAgents; i++)
        {
            if (pixelObject[i].transform.position.x > boundary + 1.235f || pixelObject[i].transform.position.x < -1 * boundary - 1.235
                || pixelObject[i].transform.position.z > boundary + 1.235f || pixelObject[i].transform.position.z < -1 * boundary - 1.235 
                || pixelObject[i].transform.position.y < 0)
            {
                //  then pixel is out
                return true;
            }
        }
        return false;
    }
}



//  self assembly 



