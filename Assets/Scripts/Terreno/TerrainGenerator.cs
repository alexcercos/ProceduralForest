using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public GameObject chunk;

    public int chunkLength = 10;
    public int chunkSpacing = 5;
    public float noiseScale = 45;

    //PLANOS en escala 5*****

    //x= -units / 45

    public int octaves = 3;
    public float persistance = 0.4f;
    public float lacunarity = 1.3f;

    public int noiseMapAmount = 2;
    [HideInInspector] public int[] seeds; //hacer aleatorias
    //no tiene offset

    public float cut = 0.4f;
    public float range = 0.05f;

    public int createRange = 5;
    public int destroyRange = 6; //lo que hay que alejarse para eliminar un chunk

    public Transform playerTransform;

    Vector2 lastChunkPosition;
    Dictionary<Vector2, GameObject> chunksArr;

    GameObject chunk_parent;

    int tile_separation;

    private void Start()
    {
        chunksArr = new Dictionary<Vector2, GameObject>();
        chunk_parent = new GameObject("Chunks Container");

        noiseMapAmount = noiseMapAmount < 1 ? 1 : noiseMapAmount; //Cap amount

        seeds = new int[noiseMapAmount];
        for (int i=0; i< noiseMapAmount; i++)
            seeds[i] = Random.Range(1, 100000);

        tile_separation = chunkLength * chunkSpacing;

        lastChunkPosition = new Vector2(Mathf.FloorToInt((playerTransform.position.x + tile_separation/2) / tile_separation), Mathf.FloorToInt((playerTransform.position.z + tile_separation/2) / tile_separation));
        GenerateChunksAround((int)lastChunkPosition.x, (int)lastChunkPosition.y);
    }

    void CreateChunk(float x, float z)
    {
        if (!chunksArr.ContainsKey(new Vector2(x, z)))
        {
            GameObject newChunk = Instantiate(chunk, transform.position + new Vector3(x, 0f, z), transform.rotation, chunk_parent.transform);
            newChunk.GetComponent<TerrainChunk>().SetChunkValues(this, new Vector2(-newChunk.transform.position.x / noiseScale, -newChunk.transform.position.z / noiseScale));

            chunksArr.Add(new Vector2(x, z), newChunk);
        }
    }

    void DestroyChunk(float x, float z)
    {
        Vector2 v = new Vector2(x, z);
        if (chunksArr.ContainsKey(v))
        {
            GameObject toDelete = chunksArr[v];
            chunksArr.Remove(v);

            Destroy(toDelete);
        }
    }

    public void GenerateChunksAround(int x, int z) //coordenadas de 1 en 1
    {
        for (int i = -createRange; i<=createRange; i++)
        {
            for (int j = -createRange; j <= createRange; j++)
            {
                CreateChunk((x + i) * tile_separation, (z + j) * tile_separation);
            }
        }
    }

    public void DestroyLimitChunks(int x, int z)
    {
        for (int j = -destroyRange; j <= destroyRange; j++)
        {
            DestroyChunk((x + destroyRange) * tile_separation, (z + j) * tile_separation);
            DestroyChunk((x - destroyRange) * tile_separation, (z + j) * tile_separation);
        }
        for (int i = -destroyRange+1; i < destroyRange; i++)
        {
            DestroyChunk((x + i) * tile_separation, (z + destroyRange) * tile_separation);
            DestroyChunk((x + i) * tile_separation, (z - destroyRange) * tile_separation);
        }
    }

    private void Update()
    {
        Vector2 newChunkPosition = new Vector2(Mathf.FloorToInt((playerTransform.position.x + tile_separation/2) / tile_separation), Mathf.FloorToInt((playerTransform.position.z + tile_separation/2) / tile_separation));

        if (newChunkPosition != lastChunkPosition)
        {
            lastChunkPosition = newChunkPosition;

            GenerateChunksAround((int)newChunkPosition.x, (int)newChunkPosition.y);
            DestroyLimitChunks((int)newChunkPosition.x, (int)newChunkPosition.y);
        }
    }
}
