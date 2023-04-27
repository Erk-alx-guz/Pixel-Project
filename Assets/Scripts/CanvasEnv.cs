using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CanvasEnv : MonoBehaviour
{
    public GameObject[] pixelAgent = new GameObject[8];

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
    float[] cordList = new float[SIZE];
    float cords = -11.25f;

    //  List holding all grid squares

    List<GameObject> GridSquares = new List<GameObject>();

    InSpot[] Spots = new InSpot[SIZE * SIZE];


    // User input
    //private float speed = 40.0f;
    //private float horizontalInput;
    //private float forwardInput;


    //  Is the environment ready
    public bool ready = false;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < SIZE; i++)
        {
            cordList[i] = cords;
            cords += 2.5f;
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

            pixelAgent[i].transform.localPosition = new Vector3(cordList[xPos], 0.125f, cordList[zPos]);
        }
    }

    void EnvironmentReady()
    {
        ready = (GridSquares.Count == SIZE * SIZE);
    }
}
