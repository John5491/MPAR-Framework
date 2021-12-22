using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulatableObject : MonoBehaviour
{
    public static int latestId = 0;
    public int id;

    public static void SetLatestId()
    {
        latestId++;
    }

    private void Awake()
    {
        id = latestId;
        SetLatestId();
    }
}
