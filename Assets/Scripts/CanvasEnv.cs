using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CanvasEnv : MonoBehaviour
{
    public GameObject pixelAgent;
    public int xPos;
    public int zPos;
    public int pixelCount;

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

    //  List holding all pixels

    List<GameObject> Pixels = new List<GameObject>();
    
    //  List holding all grid squares

    List<GameObject> GridSquares = new List<GameObject>();

    InSpot[] Spots = new InSpot[SIZE * SIZE];


    // User input
    private float speed = 40.0f;
    private float horizontalInput;
    private float forwardInput;



    private void Awake()
    {
        for (int i = 0; i < SIZE; i++)
        {
            cordList[i] = cords;
            cords += 2.5f;
        }

        StartCoroutine(SpotDrop());
        StartCoroutine(PixDrop());
    }

    // Start is called before the first frame update
    void Start()
    {
        //for (int i = 0; i < SIZE; i++)
        //{
        //    cordList[i] = cords;
        //    cords += 2.5f;
        //}

        //StartCoroutine(SpotDrop());
        //StartCoroutine(PixDrop());
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
                yield return new WaitForSeconds(0f);
            }
        }
    }


    //  Spawn all pixels

    IEnumerator PixDrop()
    {
        Hashtable canvas = new Hashtable();
        string key;

        for (int i = 0; i < pixelCount; i++)
        {
            do
            {
                xPos = Random.Range(0, SIZE);
                zPos = Random.Range(0, SIZE);
                key = string.Format("{0:N2}", xPos);
                key += string.Format("{0:N2}", zPos);
            } while (canvas[key] != null);

            canvas[key] = true;

            Pixels.Add(Instantiate(pixelAgent, new Vector3(cordList[xPos], 0.125f, cordList[zPos]), Quaternion.identity));
            yield return new WaitForSeconds(0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GridSquares.Count == SIZE * SIZE)
        {
            for (int i = 0; i < SIZE * SIZE; i++)
            {
                if (Spots[i].IsIn)
                {
                    Debug.Log("Index: " + i + Spots[i].IsIn);
                    GridSquares[i].GetComponent<Renderer>().material.color = new Color(255, 0, 0);
                }
                else
                {
                    GridSquares[i].GetComponent<Renderer>().material.color = new Color(0, 0, 0);
                }
            }
        }

        // This is for player input
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");


        //  Test that individual pixels could be tacked and moved
        //  vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

        //if (Pixels.Count > 2 && Pixels.Count == pixelCount) //  Making sure the pixel exists and don't let it move until all pixels have been spawned
        if (GridSquares.Count == SIZE * SIZE)   //  Don't let pixels move untile grid is set
            Pixels[0].transform.Translate(Vector3.right * Time.deltaTime * speed * forwardInput);
            Pixels[0].transform.Translate(Vector3.back * Time.deltaTime * speed * horizontalInput);

        //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        //  Test that individual pixels could be tacked and moved
    }

    void SpotFilled()
    {

    }
}
