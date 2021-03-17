using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    public bool debug = false;
    public int height = 100;
    public int width = 100;
    int[,] map;
    System.Random generator;
    int seed;

    [Range(0,100)]
    public int density = 50;

    void Start() {
        map = new int[width, height];
        if (debug) {
            seed = 426921939;
        } else {
            seed = System.DateTime.Now.ToString().GetHashCode();
        }

        generator = new System.Random(seed);

        Generate();
        for (int i = 0; i < 4; i++) {
            SmoothMap();
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(map, 1);

        Debug.Log($"Map generated for seed {seed}");
        // meshGen.GenerateMesh(map, 0.5f);
    }

    void Generate() {
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                int a = generator.Next(100);

                if (x == 0 || x == width-1 || y == 0 || y == height -1) {
                    map[x, y] = 1;
                } else {
                    map[x, y] = a < density ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap() {
        for (int y = 0; y < width; y++) {
            for (int x = 0; x < height; x++) {
                int neighbourWallTiles = GetSurroundingcount(x, y);

                if (neighbourWallTiles > 4)
                    map[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    map[x, y] = 0;

            }
        }
    }

    int GetSurroundingcount(int x, int y) {
        int count = 0;
        for (int dx = x - 1; dx <= x + 1; dx ++) {
            for (int dy = y - 1; dy <= y + 1; dy ++) {
                if (dx >= 0 && dx < width && dy >= 0 && dy < height) {
                    if (dx != x || dy != y) {
                        count += map[dx,dy];
                    }
                }
                else {
                    count++;
                }
            }
        }

        return count;
    }
}
