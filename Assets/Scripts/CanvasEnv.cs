using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CanvasEnv : MonoBehaviour
{
    //  pictures
    int pictureIndex = 0;

    public int[] canvas = new int[SIZE * SIZE];

    int[,] pictures = { { 14, 24, 34, 44, 54, 64, 74, 84}       //  Line
                      , { 43, 44, 45, 53, 55, 63, 64, 65}       //  Square
                      , { 44, 53, 55, 62, 63, 64, 65, 66}       //  Triangle
                      , { 24, 34, 43, 44, 45, 54, 64, 74}       //  Cross
                      , { 34, 35, 43, 46, 53, 56, 64, 65}       //  Invers square
                      , { 34, 43, 45, 53, 54, 55, 63, 65}       //  Letter A
                      , { 33, 36, 44, 45, 54, 55, 63, 66}       //  Invers of Invers square
                      , { 33, 43, 44, 45, 53, 55, 63, 65} };    //  Letter n



    const int numAgents = 8;

    public PixelAgent[] pixelAgent = new PixelAgent[numAgents];

    public GameObject[] pixelObject = new GameObject[numAgents];

    public GameObject Spot;

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
    public float cords = 11.25f;

    //  List holding all grid squares

    List<GameObject> GridSquares = new List<GameObject>();

    InSpot[] Spots = new InSpot[SIZE * SIZE];

    //  Is the environment ready
    public bool ready = false;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numAgents; i++)
            canvas[pictures[pictureIndex, i]] = 1;

        for (int i = 0; i < SIZE; i++)
        {
            cordList[i] = cords;
            cords -= 2.5f;

            canvas[i] = 0;
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

                //EnvironmentReady();

                yield return new WaitForSeconds(0f);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        //////  Testing Pictures

        //if (ready)
        //{
        //    for (int i = 0; i < SIZE * SIZE; i++)
        //    {
        //        if (Spots[i].taken)   // (canvas[i] == 1)
        //            GridSquares[i].GetComponent<Renderer>().material.color = new Color(255, 0, 0);
        //        else// if (canvas[i] == 0)
        //            GridSquares[i].GetComponent<Renderer>().material.color = new Color(0, 255, 255);
        //    }
        //}

        EnvironmentReady();

        //  Environment reward
        for (int i = 0; i < numAgents; ++i)
            pixelAgent[i].AddReward(Mathf.Pow((TakenSpots() / numAgents), 2));
    }

    //  Pixel random spawner 
    void InitPixel()
    {
        Hashtable spotTaken = new Hashtable();
        string key;
        int pixelCount = 8;
        int xPos, zPos;

        for (int i = 0; i < pixelCount; i++)
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
        }
    }

    void EnvironmentReady()
    {
        ready = (GridSquares.Count == Mathf.Pow(SIZE, 2));
    }

    int TakenSpots()
    {
        int takenSpots = 0;
        for (int i = 0; i < numAgents; i++)
        {
            if (Spots[1].taken)
                takenSpots++;
        }

        return takenSpots;
    }
}



//  self assembly 



