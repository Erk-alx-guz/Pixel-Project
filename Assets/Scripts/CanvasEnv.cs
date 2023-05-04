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

    const int SIZE = 10;
    public int size = SIZE;
    float[] cordList = new float[SIZE];
    float startingCords = 11.25f;
    const float boundary = 11.25f;

    const int numAgents = 8;


    //  pictures
    int pictureIndex = 1;

    public int[] canvas = new int[SIZE * SIZE];

    int[,] pictures = { { 14, 24, 34, 44, 54, 64, 74, 84}       //  Line
                      , { 43, 44, 45, 53, 55, 63, 64, 65}       //  Square
                      , { 44, 53, 55, 62, 63, 64, 65, 66}       //  Triangle
                      , { 24, 34, 43, 44, 45, 54, 64, 74}       //  Cross
                      , { 34, 35, 43, 46, 53, 56, 64, 65}       //  Invers square
                      , { 34, 43, 45, 53, 54, 55, 63, 65}       //  Letter A
                      , { 33, 36, 44, 45, 54, 55, 63, 66}       //  Invers of Invers square
                      , { 33, 43, 44, 45, 53, 55, 63, 65} };    //  Letter n


    public PixelAgent[] pixelAgent = new PixelAgent[numAgents];

    public GameObject[] pixelObject = new GameObject[numAgents];

    public GameObject Spot;

    //  List holding all grid squares

    List<GameObject> GridSquares = new List<GameObject>();

    InSpot[] Spots = new InSpot[SIZE * SIZE];

    //  Is the environment ready
    public bool ready = false;


    // Start is called before the first frame update
    void Start()
    {
        //  Set the pixels on the canvas
        for (int i = 0; i < numAgents; i++)
            canvas[pictures[pictureIndex, i]] = 1;

        for (int i = 0; i < SIZE; i++)
        {
            cordList[i] = startingCords;
            startingCords -= 2.5f;

            //print(cordList[i]);

            for (int j = 0; j < SIZE; j++)  //  Fill the rest of the canvas with zeros
            {
                if (canvas[i * SIZE + j] != 1)
                    canvas[i * SIZE + j] = 0;
            }
        }

        StartCoroutine(SpotDrop());
        InitPixel();
    }


    //  Spawn all grid squares

    IEnumerator SpotDrop()
    {
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                GridSquares.Add(Instantiate(Spot, new Vector3(cordList[i], 0.125f, cordList[j]), Quaternion.identity));
                Spots[i * SIZE + j] = GridSquares[i * SIZE + j].GetComponent<InSpot>();

                EnvironmentReady();

                yield return new WaitForSeconds(0f);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (ready)  //  The environment must be set up befor doing anything
        {
            for (int i = 0; i < SIZE * SIZE; i++)
            {
                if (Spots[i].taken)   // (canvas[i] == 1)
                    GridSquares[i].GetComponent<Renderer>().material.color = new Color(255, 0, 0);
                else// if (canvas[i] == 0)
                    GridSquares[i].GetComponent<Renderer>().material.color = new Color(0, 255, 255);
            }


            //  Environment reward
            for (int i = 0; i < numAgents; ++i)
                pixelAgent[i].AddReward(Mathf.Pow(TakenSpots() / numAgents, 2));

            //print(Mathf.Pow((float) TakenSpots() / (float) numAgents, 2));


            //  Test outside of boundary
            print(OutOfBoundary());
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
                xPos = Random.Range(0, SIZE);
                zPos = Random.Range(0, SIZE);
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
    void EnvironmentReady()
    {
        ready = GridSquares.Count == Mathf.Pow(SIZE, 2);
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
        else
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



