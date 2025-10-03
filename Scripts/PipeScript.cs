using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PipeScript : MonoBehaviour
{
    [HideInInspector] public bool isFilled;
    [HideInInspector] public int PipeType;

    [SerializeField] private Transform[] pipePrefabs;

    private Transform currentPipe;
    private int rotation;

    private SpriteRenderer emptySprite;
    private SpriteRenderer filledSprite;
    private List<Transform> connectBoxes;

    private const int minRotation = 0;
    private const int maxRotation = 3;
    private const int rotateMultiplier = 90;

    public void Init(int pipe)
    {
        PipeType = pipe % 10;   // the 2nd digit decides the pipe type
        currentPipe = Instantiate(pipePrefabs[PipeType], transform);    //  clones the pipePrefab
        currentPipe.transform.localPosition = Vector3.zero;
        if (PipeType == 1 || PipeType == 2)
        {
            rotation = pipe / 10;   // the 1st digit decides the rotation: 0 -> 0° (default), 1 -> 90°, 2 -> 180°, 3 -> 270° 
        }
        else
        {
            rotation = Random.Range(minRotation, maxRotation + 1);  // +1 is for the range to pick up the numbers from 0 -> 3
        }
        currentPipe.transform.eulerAngles = new Vector3(0, 0, rotation * rotateMultiplier);

        if (PipeType == 0 || PipeType == 1)
        {
            isFilled = true;
        }

        if (PipeType == 0)
        {
            return;
        }

        emptySprite = currentPipe.GetChild(0).GetComponent<SpriteRenderer>();
        emptySprite.gameObject.SetActive(!isFilled);
        filledSprite = currentPipe.GetChild(1).GetComponent<SpriteRenderer>();
        filledSprite.gameObject.SetActive(isFilled);

        connectBoxes = new List<Transform>();
        for (int i = 2; i < currentPipe.childCount; i++)
        {
            connectBoxes.Add(currentPipe.GetChild(i));
        }
    }

    public void UpdateInput()
    {
        if (PipeType == 0 || PipeType == 1 || PipeType == 2)
        {
            return ;
        }

        rotation = (rotation + 1) % (maxRotation + 1);
        currentPipe.transform.eulerAngles = new Vector3(0, 0, rotation * rotateMultiplier);
    }

    public void UpdateFilled()
    {
        if (PipeType == 0) return;
        emptySprite.gameObject.SetActive(!isFilled);
        filledSprite.gameObject.SetActive(isFilled);
    }

    public List<PipeScript> ConnectedPipes()
    {
        List<PipeScript> result = new List<PipeScript>();

        foreach (var box in connectBoxes)
        {
            RaycastHit2D[] hit = Physics2D.RaycastAll(box.transform.position, Vector2.zero, 0.1f);
            for (int i = 0; i < hit.Length; i++)
            {
                result.Add(hit[i].collider.transform.parent.parent.GetComponent<PipeScript>());
            }
        }

        return result;
    }
}
