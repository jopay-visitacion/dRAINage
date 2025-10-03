using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private LevelData level;
    [SerializeField] private PipeScript cellPrefabs;

    private bool hasGameFinished;
    private PipeScript[,] pipes;
    private List<PipeScript> startPipes;

    private void Awake()
    {
        Instance = this;
        hasGameFinished = false;
        SpawnLevel();
    }
    private void SpawnLevel()
    {
        pipes = new PipeScript[level.row, level.column];
        startPipes = new List<PipeScript>();

        for (int i = 0; i < level.row; i++)
        {
            for (int j = 0; j < level.column; j++)
            {                                  // spawning starts from the right to left, bottom to top
                Vector2 spawnPos = new Vector2(j + 0.5f, i + 0.5f - ((level.row - 1) * 0.5f));      // added ((level.row - 1) * 0.5f) to make the pipes positioned at the bottom
                PipeScript tempPipe = Instantiate(cellPrefabs);
                tempPipe.transform.position = spawnPos;
                tempPipe.Init(level.Data[i * level.column + j]);
                pipes[i, j] = tempPipe;
                if (tempPipe.PipeType == 1)
                {
                    startPipes.Add(tempPipe);
                }
            }
        }

        Camera.main.orthographicSize = Mathf.Max(level.row, level.column);
        Vector3 cameraPos = Camera.main.transform.position;
        cameraPos.x = level.column * 0.5f;
        cameraPos.y = level.row * 0.5f; 
        Camera.main.transform.position = cameraPos;
    }

    private void Update()
    {
        if (hasGameFinished) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int row = Mathf.FloorToInt(mousePos.y);
        int col = Mathf.FloorToInt(mousePos.x);
        if (row < 0 || col < 0) return;
        if (row >= level.row || col >= level.column) return;

        if (Input.GetMouseButtonDown(0))
        {
            pipes[row, col].UpdateInput();
            StartCoroutine(ShowHint());
        }
    }

    private IEnumerator ShowHint()
    {
        yield return new WaitForSeconds(0.1f);
        CheckFill();
        CheckWin();
    }

    private void CheckFill()
    {
        for (int i = 0; i < level.row; i++)
        {
            for (int j = 0; j < level.column; j++)
            {
                PipeScript tempPipe = pipes[i, j];
                if (tempPipe.PipeType != 0)
                {
                    tempPipe.isFilled = false;
                }
            }
        }

        Queue<PipeScript> check = new Queue<PipeScript>();
        HashSet<PipeScript> finished = new HashSet<PipeScript>();

        foreach (var pipe in startPipes)
        {
            check.Enqueue(pipe);
        }

        while (check.Count > 0)
        {
            PipeScript pipe = check.Dequeue();
            finished.Add(pipe);
            List<PipeScript> connected = pipe.ConnectedPipes();

            foreach (var connectedPipe in connected)
            {
                if (!finished.Contains(connectedPipe))
                {
                    check.Enqueue(connectedPipe);
                }
            }
        }

        foreach (var filled in finished)
        {
            filled.isFilled = true;
        }

        for (int i = 0; i < level.row; i++)
        {
            for (int j = 0; j < level.column; j++)
            {
                PipeScript tempPipe = pipes[i, j];
                tempPipe.UpdateFilled();
            }
        }
    }

    private void CheckWin()
    {
        for (int i = 0; i < level.row; i++)
        {
            for (int j = 0; j < level.column; j++)
            {
                if (!pipes[i, j].isFilled) return;
            }
        }

        hasGameFinished = true;
        StartCoroutine(GameFinished());
    }

    private IEnumerator GameFinished()
    {
        yield return new WaitForSeconds(2f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
