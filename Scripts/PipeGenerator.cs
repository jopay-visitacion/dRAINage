using System.Collections.Generic;
using UnityEngine;

public class PipeGenerator : MonoBehaviour          // ATTACH THIS SCRIPT TO AN EMPTY GAME OBJECT NAMED PIPE GENERATOR
{
    // pipe prefabs
    public GameObject[] pipePrefabs = null;
    public GameObject[] emptyPrefab = null;
    public GameObject[] slotPrefab =  null;

    // where all the tiles will be placed: manual & automated pipes -> container, empty with collider -> slot holder, empty cells -> slotHolder
    private GameObject container;
    private GameObject slotHolder;
    private GameObject emptyCell;

    // the row, column, and the list of manually genrerated pipes
    [Header("Grid")]
    public int row;
    public int column;

    [Header("Pipe.X / Pipe.Y / Pipe.Type")]
    public List<int> tilePos;

    // for the Empty Slots
    private static readonly List<Vector2> emptySpawnPoint = new();
    private static readonly List<GameObject> slots = new();

    // for the Empty Cells
    private static readonly List<Vector2> emptyCells = new();

    // for the Pipe Slots
    private static readonly List<Vector2> spawnPoint = new();
    private static readonly List<GameObject> pipe = new();

    // random rotation of the pipes
    private static readonly int[] zRotation = { 0, 90, 180, 270 };

    private void Awake()
    {
        container = new GameObject(name = "Pipe Holder");
        slotHolder = new GameObject(name = "All Pipes Holder");
        emptyCell = new GameObject(name = "Cell Holder");
    }

    void Start()
    {
        EmptyCell();
        EmptySlotGenerator();
        TileGenerator();
        RandomTileGenerator();
    }
 

    public void AdjustCamera()
    {
        Camera.main.orthographicSize = Mathf.Max(row,column);
        Vector3 cameraPos = Camera.main.transform.position;
        cameraPos.x = row * 0.5f;
        cameraPos.y = -column * 0.5f;
        Camera.main.transform.position = cameraPos;

    }

    public void EmptyCell()
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                Vector2 spawnEmpty = new Vector2(i + 0.5f, -j - 0.5f);
                emptyCells.Add(spawnEmpty);

                GameObject empty = Instantiate(emptyPrefab[0], spawnEmpty, Quaternion.identity);

                if (empty.GetComponent<SlotScript>() == null)
                {
                    // added a SlotScript to empty
                    empty.AddComponent<SlotScript>();
                }

                var box = empty.AddComponent<BoxCollider2D>();
                box.size = Vector2.one;
                empty.transform.localScale = Vector3.one;
                // CHANGE PIPESCRIPT TO SLOTSCRIPT AND SET ALL EMPTYCELL.ACTIVE TO EMPTYCELL.OCCUPIED
                empty.transform.SetParent(emptyCell.transform, false);
            }
        }
    }

    public void TileGenerator() // manually generated pipes
    {
        foreach (int pos in tilePos)
        {
            int i = pos / 100; // row
            int j = (pos % 100) / 10; // column

            Vector2 spawnPos = new Vector2((i + 0.5f) - 1, -j + 0.5f);
            spawnPoint.Add(spawnPos);

            int pipeType = (pos % 100) % 10;
            int rand = UnityEngine.Random.Range(0, zRotation.Length);
            Quaternion rot = Quaternion.Euler(0, 0, zRotation[rand]);

            GameObject tile = Instantiate(pipePrefabs[pipeType], spawnPos, rot);
            // adds PipeScript to the pipes
            tile.AddComponent<PipeScript>();
            // gets the PipeScript from tileBool
            PipeScript tileBool = tile.GetComponent<PipeScript>();
            // set the tile parent to container
            tile.transform.SetParent (container.transform);
            
            pipe.Add(tile);

            foreach (Transform child in emptyCell.transform)
            {
                var cell = child.GetComponent<SlotScript>();
                float dist = Vector2.Distance(child.position, spawnPos);

                if (dist <= 0)
                {
                    // set the tile parent to empty cell
                    tile.transform.SetParent(child.transform, true);
                    tileBool.active = true;
                    cell.occupied = true;
                    break;
                }
            }
        }

        AdjustCamera();
    }

    public void RandomTileGenerator()
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column + 1; j++)
            {
                Vector2 spawnTile = new Vector2(i + 0.5f, -j - 0.5f);
                int random = UnityEngine.Random.Range(0, zRotation.Length);
                Quaternion randRot = Quaternion.Euler(0, 0, zRotation[random]);
                int randPrefab = UnityEngine.Random.Range(0, pipePrefabs.Length);

                GameObject tempPipe = Instantiate(pipePrefabs[0], spawnTile, randRot);

                if (tempPipe.GetComponent<PipeScript>() == null)
                {
                    tempPipe.AddComponent<PipeScript>();
                }
                
                tempPipe.transform.SetParent(container.transform);

                // add a condition if there is already a pipe where spawnTile is...

                bool attached = false;

                foreach (Transform child in emptyCell.transform)
                {
                    SlotScript cellPipe = child.GetComponent<SlotScript>();
                    float dist = Vector2.Distance(child.position, spawnTile);

                    // if cell position matches (within small margin) and it's inactive
                    if (dist < 0.1f && !cellPipe.occupied)
                    {
                        tempPipe.transform.SetParent(child);
                        cellPipe.occupied = true;
                        attached = true;
                        break; // stop checking other cells
                    }
                }

                // if no valid empty cell found, destroy the tempPipe
                if (!attached)
                {
                    Destroy(tempPipe);
                }

            }
        }

        AdjustCamera();
    }

    public void EmptySlotGenerator()
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                Vector2 spawnSlot = new Vector2(i + 0.5f, -j - 0.5f);
                emptySpawnPoint.Add(spawnSlot);

                GameObject emptyTile = Instantiate(slotPrefab[0], spawnSlot, Quaternion.identity);

                if (emptyTile.GetComponent<SlotScript>() == null)
                {
                    // adds SlotScript to the emptyTile
                    emptyTile.AddComponent<SlotScript>();
                }

                // gets the SlotScript from emptyTileBool
                emptyTile.transform.SetParent(slotHolder.transform);
            }
        }

        AdjustCamera();
    }
}
