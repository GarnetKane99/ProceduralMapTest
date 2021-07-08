using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Generation : MonoBehaviour
{
    public Tilemap TilesInScene;
    public Tile Grass, Water, Dirt;

    public Tile[] FloorTiles;
    public GameObject NodeToSpawn, ParentNode;

    public List<GameObject> Tiles;

    [Header("Walls")]
    public Tile[] Walls;

    public int width, length;

    public bool GeneratingMap = false;
    int Refiner = 0;
    int GrassRefiner = 0;

    // Start is called before the first frame update
    void Start()
    {
        TilesInScene = GameObject.FindGameObjectWithTag("Ground").GetComponent<Tilemap>();


        //GenerateMap();
        //PopulateTiles();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateMap()
    {
        GeneratingMap = true;
        Tiles = new List<GameObject>();

        width = 30;
        length = 20;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < length; j++)
            {
                TilesInScene.SetTile(new Vector3Int(i, j, 0), Dirt);
            }
        }
        PopulateTiles();
        Refiner = 0;
        GrassRefiner = 0;

        if(Tiles != null)
        {
            for(int x = 0; x < Tiles.Count; x++)
            {
                Tiles[x].GetComponent<NodeController>().ConnectedObjects = new List<GameObject>();
            }
        }
    }

    void PopulateTiles()
    {
        for (int i = TilesInScene.cellBounds.xMin; i < TilesInScene.cellBounds.xMax; i++)
        {
            for (int j = TilesInScene.cellBounds.yMin; j < TilesInScene.cellBounds.yMax; j++)
            {
                Vector3Int LocalPos = new Vector3Int(i, j, (int)TilesInScene.transform.position.y);
                Vector3 Pos = TilesInScene.CellToWorld(LocalPos);

                if (TilesInScene.HasTile(LocalPos))
                {
                    Pos.x = Pos.x + 0.5f;
                    Pos.y = Pos.y + 0.5f;
                    GameObject Node = Instantiate(NodeToSpawn, Pos, Quaternion.identity) as GameObject;
                    Node.gameObject.tag = "Node";
                    Node.transform.parent = ParentNode.transform;

                    Tiles.Add(Node);
                    for (int x = 0; x < Tiles.Count; x++)
                    {
                        Tiles[x].GetComponent<NodeController>().Grass = false;
                        Tiles[x].GetComponent<NodeController>().Water = false;
                        Tiles[x].GetComponent<NodeController>().Dirt = false;
                        Tiles[x].GetComponent<NodeController>().Safe = false;
                        Tiles[x].GetComponent<NodeController>().Unsafe = false;
                        Tiles[x].GetComponent<NodeController>().up = false;
                        Tiles[x].GetComponent<NodeController>().down = false;
                        Tiles[x].GetComponent<NodeController>().right = false;
                        Tiles[x].GetComponent<NodeController>().left = false;
                        //Tiles[x].GetComponent<NodeController>().ConnectedObjects = null;
                        Tiles[x].name = "Node " + x;
                    }
                }
                else
                {
                    //nothing
                }
            }
        }

        Invoke("PopulateType", 1.0f);
        Invoke("CreateConnection", 3.0f);
    }

    void PopulateType()
    {
        int TotalGrass = Random.Range((Tiles.Count / 3) * 2, Tiles.Count);

        for (int i = 0; i < TotalGrass; i++)
        {
            int RandomNumber = Random.Range(0, Tiles.Count);
            if (Tiles[RandomNumber].GetComponent<NodeController>().Grass == false)
            {
                Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[RandomNumber].transform.position);
                TilesInScene.SetTile(CurrentTile, FloorTiles[Random.Range(0, FloorTiles.Length)]);
                Tiles[RandomNumber].GetComponent<NodeController>().Grass = true;
            }
            Invoke("PopulateWater", 1.0f);
        }
    }

    void PopulateWater()
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            if (Tiles[i].GetComponent<NodeController>().Grass == false)
            {
                Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                TilesInScene.SetTile(CurrentTile, Water);
                Tiles[i].GetComponent<NodeController>().Water = true;
            }
        }

        //CreateConnection();
    }

    void CreateConnection()
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            for (int j = i + 1; j < Tiles.Count; j++)
            {
                if (Vector2.Distance(Tiles[i].transform.position, Tiles[j].transform.position) <= 1.5f && Vector2.Distance(Tiles[i].transform.position, Tiles[j].transform.position) >= 0.5f)
                {
                    AddConnection(Tiles[i], Tiles[j]);
                    AddConnection(Tiles[j], Tiles[i]);
                }
            }
        }
        Invoke("RefineTiles", 1.0f);
    }

    void AddConnection(GameObject From, GameObject To)
    {
        From.GetComponent<NodeController>().ConnectedObjects.Add(To);
    }



    void RefineTiles()
    {
        Refiner++;

        for (int i = 0; i < Tiles.Count; i++)
        {
            if (Tiles[i].GetComponent<NodeController>().Water && Tiles[i].GetComponent<NodeController>().ConnectedObjects.Count > 3)
            {
                int GrassCounter = 0;
                for (int x = 0; x < Tiles[i].GetComponent<NodeController>().ConnectedObjects.Count; x++)
                {
                    if (Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].GetComponent<NodeController>().Grass)
                    {
                        GrassCounter++;
                    }
                    if (GrassCounter > 5)
                    {
                        Tiles[i].GetComponent<NodeController>().Dirt = true;
                    }
                }
            }
        }
        Invoke("FixTiles", 1.0f);
    }

    void FixTiles()
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            if (Tiles[i].GetComponent<NodeController>().Dirt)
            {
                Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                TilesInScene.SetTile(CurrentTile, FloorTiles[Random.Range(0, FloorTiles.Length)]);
                Tiles[i].GetComponent<NodeController>().Grass = true;
                Tiles[i].GetComponent<NodeController>().Dirt = false;
                Tiles[i].GetComponent<NodeController>().Water = false;
            }
        }
        if (Refiner < 5)
            Invoke("RefineTiles", 1.0f);
        else
            Invoke("RefineGrass", 1.0f);
    }

    void RefineGrass()
    {
        GrassRefiner++;

        for (int i = 0; i < Tiles.Count; i++)
        {
            if (Tiles[i].GetComponent<NodeController>().Grass && Tiles[i].GetComponent<NodeController>().ConnectedObjects.Count > 5)
            {
                int WaterCounter = 0;
                for (int x = 0; x < Tiles[i].GetComponent<NodeController>().ConnectedObjects.Count; x++)
                {
                    if (Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].GetComponent<NodeController>().Water)
                    {
                        WaterCounter++;
                    }
                    if (WaterCounter > 5)
                    {
                        Tiles[i].GetComponent<NodeController>().Dirt = true;
                    }
                }
            }
        }
        Invoke("FixWater", 1.0f);
    }

    void FixWater()
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            if (Tiles[i].GetComponent<NodeController>().Dirt)
            {
                Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                TilesInScene.SetTile(CurrentTile, Water);
                Tiles[i].GetComponent<NodeController>().Grass = false;
                Tiles[i].GetComponent<NodeController>().Dirt = false;
                Tiles[i].GetComponent<NodeController>().Water = true;
            }
        }
        if (GrassRefiner < 5)
            Invoke("RefineGrass", 1.0f);
        else
            Invoke("RefineFloor", 1.0f);
    }

    void RefineFloor()
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            if (Tiles[i].GetComponent<NodeController>().Grass && Tiles[i].GetComponent<NodeController>().ConnectedObjects.Count > 5)
            {
                for (int x = 0; x < Tiles[i].GetComponent<NodeController>().ConnectedObjects.Count; x++)
                {
                    if (Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].GetComponent<NodeController>().Grass == true)
                    {
                        if (Tiles[i].transform.position.x == Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].transform.position.x)
                        {
                            Tiles[i].GetComponent<NodeController>().Safe = true;
                        }
                        else if (Tiles[i].transform.position.y == Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].transform.position.y)
                        {
                            Tiles[i].GetComponent<NodeController>().Safe = true;
                        }
                        else
                        {
                            Tiles[i].GetComponent<NodeController>().Unsafe = true;
                        }
                    }
                }
            }
        }
        Invoke("FixFloor", 1.0f);
    }

    void FixFloor()
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            if (Tiles[i].GetComponent<NodeController>().Unsafe)
            {
                Tiles[i].GetComponent<NodeController>().Dirt = true;
                if (Tiles[i].GetComponent<NodeController>().Dirt)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Water);
                    Tiles[i].GetComponent<NodeController>().Grass = false;
                    Tiles[i].GetComponent<NodeController>().Dirt = false;
                    Tiles[i].GetComponent<NodeController>().Water = true;
                }
            }
        }

        Invoke("RedoWalls", 1.0f);
    }

    void RedoWalls()
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            if (Tiles[i].GetComponent<NodeController>().ConnectedObjects.Count < 8)
            {
                if (Tiles[i].GetComponent<NodeController>().Grass)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Water);
                    Tiles[i].GetComponent<NodeController>().Grass = false;
                    Tiles[i].GetComponent<NodeController>().Dirt = false;
                    Tiles[i].GetComponent<NodeController>().Water = true;
                }
            }
        }

        Invoke("CheckWalls", 1.0f);
    }

    void CheckWalls()
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            if (Tiles[i].GetComponent<NodeController>().Water)
            {
                for (int x = 0; x < Tiles[i].GetComponent<NodeController>().ConnectedObjects.Count; x++)
                {
                    //Above only
                    if (Tiles[i].transform.position.x == Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].transform.position.x
                        && Tiles[i].transform.position.y < Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].transform.position.y
                        && Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].GetComponent<NodeController>().Grass)
                    {
                        Tiles[i].GetComponent<NodeController>().up = true;
                    }
                    else if (Tiles[i].transform.position.x == Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].transform.position.x
                        && Tiles[i].transform.position.y > Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].transform.position.y
                        && Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].GetComponent<NodeController>().Grass)
                    {
                        Tiles[i].GetComponent<NodeController>().down = true;
                    }
                    else if (Tiles[i].transform.position.y == Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].transform.position.y
                        && Tiles[i].transform.position.x < Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].transform.position.x
                        && Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].GetComponent<NodeController>().Grass)
                    {
                        Tiles[i].GetComponent<NodeController>().right = true;
                    }
                    else if (Tiles[i].transform.position.y == Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].transform.position.y
                        && Tiles[i].transform.position.x > Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].transform.position.x
                        && Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].GetComponent<NodeController>().Grass)
                    {
                        Tiles[i].GetComponent<NodeController>().left = true;
                    }
                }
            }
        }

        Invoke("AddWalls", 1.0f);
    }

    void AddWalls()
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            if (Tiles[i].GetComponent<NodeController>().Water)
            {
                NodeController Node = Tiles[i].GetComponent<NodeController>();
                if (Node.up && !Node.down && !Node.left && !Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[0]);
                }
                else if (!Node.up && Node.down && !Node.left && !Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[1]);
                }
                else if (!Node.up && !Node.down && Node.left && !Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[2]);
                }
                else if (!Node.up && !Node.down && !Node.left && Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[3]);
                }
                else if (Node.up && !Node.down && !Node.left && Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[4]);
                }
                else if (Node.up && !Node.down && Node.left && !Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[5]);
                }
                else if (!Node.up && Node.down && Node.left && !Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[6]);
                }
                else if (!Node.up && Node.down && !Node.left && Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[7]);
                }
                else if (Node.up && Node.down && !Node.left && !Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[8]);
                }
                else if (!Node.up && !Node.down && Node.left && Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[9]);
                }
                else if (!Node.up && Node.down && Node.left && Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[10]);
                }
                else if (Node.up && Node.down && !Node.left && Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[11]);
                }
                else if (Node.up && !Node.down && Node.left && Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[12]);
                }
                else if (Node.up && Node.down && Node.left && !Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[13]);
                }
                else if (Node.up && Node.down && Node.left && Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, Walls[14]);
                }
                else if (!Node.up && !Node.down && !Node.left && !Node.right)
                {
                    Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                    TilesInScene.SetTile(CurrentTile, null);
                }
            }
        }

        Invoke("FinishMap", 1.0f);
    }

    void FinishMap()
    {
        GeneratingMap = false;
    }

    /*        void EdgeChecker()
            {
                for (int i = 0; i < Tiles.Count; i++)
                {
                    if (Tiles[i].GetComponent<NodeController>().Grass && Tiles[i].GetComponent<NodeController>().ConnectedObjects.Count == 5)
                    {
                        int GrassCounter = 0;
                        for (int x = 0; x < Tiles[i].GetComponent<NodeController>().ConnectedObjects.Count; x++)
                        {
                            if (Tiles[i].GetComponent<NodeController>().ConnectedObjects[x].GetComponent<NodeController>().Water)
                            {
                                GrassCounter++;
                            }
                            if (GrassCounter > 3)
                            {
                                Tiles[i].GetComponent<NodeController>().Dirt = true;
                            }
                        }
                    }
                }

                Debug.Log("Fixing Edges");
                Invoke("FixEdge", 1.0f);
            }

            void FixEdge()
            {
                for (int i = 0; i < Tiles.Count; i++)
                {
                    if (Tiles[i].GetComponent<NodeController>().Dirt)
                    {
                        Vector3Int CurrentTile = TilesInScene.WorldToCell(Tiles[i].transform.position);
                        TilesInScene.SetTile(CurrentTile, Water);
                        Tiles[i].GetComponent<NodeController>().Grass = false;
                        Tiles[i].GetComponent<NodeController>().Dirt = false;
                        Tiles[i].GetComponent<NodeController>().Water = true;
                    }
                }
            }*/
}
