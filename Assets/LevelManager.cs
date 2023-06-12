using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> levelObstacles;

    public void CreateLevelFile()
    {
        //Take all objects in list and save position + rotation + width & height
        //Read Level JSON in Server.
    }
}
