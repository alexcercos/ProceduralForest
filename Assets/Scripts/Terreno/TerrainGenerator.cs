using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public GameObject chunk;

    public int chunkLength = 50;
    public float noiseScale = 45;

    //PLANOS en escala 5*****

    //x= -units / 45

    public int octaves = 3;
    public float persistance = 0.4f;
    public float lacunarity = 1.3f;

    public int seed1; //hacer aleatorias
    public int seed2;
    //no tiene offset

    public float cut = 0.4f;
    public float range = 0.05f;

    public int createRange = 5;
    public int destroyRange = 6; //lo que hay que alejarse para eliminar un chunk

    public Transform playerTransform;
    [HideInInspector] public Vector2 lastChunkPosition;
    
    

    static List<Vector3> list;

    Dictionary<Vector2, GameObject> chunksArr;

    static Color particle1 = new Color(1f, 1f, 1f, 0.0f);
    static Color particle2 = new Color(1f, 1f, 0f, 0.8f);
    static Color particle3 = new Color(0.5f, 0f, 0f, 1f);

    //FALTA tener en cuenta el escenario central

    private void Start()
    {
        chunksArr = new Dictionary<Vector2, GameObject>();
        
        seed1 = Random.Range(1, 100000);
        seed2 = Random.Range(1, 100000);

        if (seed1 == seed2) seed2++;



        lastChunkPosition = new Vector2(Mathf.FloorToInt((playerTransform.position.x + 25) / 50), Mathf.FloorToInt((playerTransform.position.z + 25) / 50));
        GenerateChunksAround((int)lastChunkPosition.x, (int)lastChunkPosition.y);
    }

    void CreateChunk(float x, float z)
    {
        if (!chunksArr.ContainsKey(new Vector2(x, z)))
        {
            GameObject newChunk = Instantiate(chunk, transform.position + new Vector3(x, 0f, z), transform.rotation);
            newChunk.GetComponent<TerrainChunk>().SetChunkValues(this, new Vector2(-newChunk.transform.position.x / 45f, -newChunk.transform.position.z / 45f));

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
                CreateChunk((x + i) * 50f, (z + j) * 50f);
            }
        }
    }

    public void DestroyLimitChunks(int x, int z)
    {
        for (int j = -destroyRange; j <= destroyRange; j++)
        {
            DestroyChunk((x + destroyRange) * 50f, (z + j) * 50f);
            DestroyChunk((x - destroyRange) * 50f, (z + j) * 50f);
        }
        for (int i = -destroyRange+1; i < destroyRange; i++)
        {
            DestroyChunk((x + i) * 50f, (z + destroyRange) * 50f);
            DestroyChunk((x + i) * 50f, (z - destroyRange) * 50f);
        }
    }

    private void Update()
    {
        Vector2 newChunkPosition = new Vector2(Mathf.FloorToInt((playerTransform.position.x + 25) / 50), Mathf.FloorToInt((playerTransform.position.z + 25) / 50));

        if (newChunkPosition != lastChunkPosition)
        {
            lastChunkPosition = newChunkPosition;

            GenerateChunksAround((int)newChunkPosition.x, (int)newChunkPosition.y);
            DestroyLimitChunks((int)newChunkPosition.x, (int)newChunkPosition.y);
        }
    }

    public void AddFinalLocation()
    {
        list.Add(Vector3.zero); //localizacion del monte
    }
}
