using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [SerializeField] Transform playerTrans;
    [SerializeField] Transform[] spawnLocations;

    public void SpawnAtLocation(int index)
    {
        Debug.Log($"WorldManager.SpawnAtLocation(int {index}) : Trying to spawn at index. Location list only contains {spawnLocations.Length} locations");

        if (index >= spawnLocations.Length)
        {
            if (spawnLocations == null || spawnLocations.Length == 0)
            {
                Debug.LogError($"WorldManager.SpawnAtLocation(int {index}) : No spawn locations have been set");
            }
            else
            {
                Debug.LogError($"WorldManager.SpawnAtLocation(int {index}) : Trying to spawn at index when location list only contains {spawnLocations.Length} locations");
                index = spawnLocations.Length - 1;
            }
        }

        playerTrans.position = spawnLocations[index].position;
        playerTrans.rotation = spawnLocations[index].rotation;
    }
}
