using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public int seed1, seed2;
    public Vector2 offset;

    public bool autoUpdate;

    private void Update()
    {
        if (autoUpdate)
        {
            GenerateMap();
        }
    }

    public void GenerateMap()
    {
        float[,] noiseMap1 = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed1, noiseScale, octaves, persistance, lacunarity, offset);
        float[,] noiseMap2 = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed2, noiseScale, octaves, persistance, lacunarity, offset);

        MapDisplay display = FindObjectOfType<MapDisplay>();

        display.DrawNoiseMap(noiseMap1, noiseMap2);
    }

    private void OnValidate()
    {
        if (mapWidth < 1)
        {
            mapWidth = 1;
        }
        if (mapHeight < 1)
        {
            mapHeight = 1;
        }

        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }
}
