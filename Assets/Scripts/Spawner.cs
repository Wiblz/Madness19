using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public GameObject playerPrefab;
    public BulletHandler bulletHandler;
    public CinemachineVirtualCamera vcam;
    MeshGenerator.Map map;

    void Start() {
        map = GameObject.Find("MapGenerator").GetComponent<MeshGenerator>().map;
    }

    void Update() {
        if (Input.GetKeyDown("o")) {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer() {
        Vector2 spawnPoint = map.LocateEmptySpace(Vector2.zero);
        GameObject player = Instantiate(playerPrefab, spawnPoint, Quaternion.identity);
        bulletHandler.player = player;
        vcam.Follow = player.transform;
    }
}
