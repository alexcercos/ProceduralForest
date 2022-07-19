using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    [System.Serializable]
    public struct TerrainObject
    {
        public GameObject asset;
        public float weight;

        public float last;

        public bool sameWeight() { return last == weight; }
        public void updateWeight() { last = weight; }
        public void setOverflow() { last = 2f; }
    }

    //.35, .35, .25, .05
    [NonReorderable] public TerrainObject[] grassObjects;
    //.25 all
    [NonReorderable] public TerrainObject[] treeObjects;

    Renderer textureRenderer;
    float[,] values;
    Vector2 chunkOffset;

    public float globalScale = 0.2f;

    public float minTreeScale = 1f;
    public float maxTreeScale = 1.5f;

    public float minGrassScale = 0.6f;
    public float maxGrassScale = 1.1f;

    public float grassLimit = 0.05f;
    public float treeLimit = 0.15f;

    Color pathColor = new Color(100f / 255f, 
                                147f / 255f, 
                                60f / 255f);
    Color grassColorInit = new Color(60f / 255f, 
                                     147f / 255f, 
                                     20f / 255f);
    Color grassColorEnd = new Color(15f / 255f, 
                                    120f / 255f, 
                                    60f / 255f);

    TerrainGenerator gen;

    public void Awake()
    {
        textureRenderer = GetComponent<Renderer>();
    }

    public void SetChunkValues(TerrainGenerator generator, Vector2 offset)
    {
        chunkOffset = offset;

        gen = generator;

        values = new float[gen.chunkLength*gen.chunkSpacing, gen.chunkLength * gen.chunkSpacing];

        transform.localScale = new Vector3(gen.chunkSpacing, gen.chunkSpacing, gen.chunkSpacing);

        StartCoroutine(ValuesParallel());
    }
    
    private void OnValidate()
    {
        NormalizeArray(ref grassObjects);
        NormalizeArray(ref treeObjects);

        NormalizeScales(ref minGrassScale, ref maxGrassScale);
        NormalizeScales(ref minTreeScale, ref maxTreeScale);
    }
    
    void NormalizeArray(ref TerrainObject[] array)
    {
        float total = 0f;
        float last_total = 0f;
        bool changed = false;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].sameWeight())
                last_total += array[i].weight;
            else
            {
                if (total!=0f)
                {
                    for (int j = 0; j < array.Length; j++) //First call
                        array[j].updateWeight();
                    return;
                }
                array[i].weight = Mathf.Max(array[i].weight, 0f);

                if (array[i].weight>1f)
                {
                    array[i].weight = 1f;
                    array[i].setOverflow();
                }
                total = 1f - array[i].weight;

                changed = true;
            }
        }

        if (!changed)
        {
            for (int j = 0; j < array.Length; j++) //First call
                array[j].updateWeight();
            return;
        }

        if (last_total == 0f)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].sameWeight())
                    array[i].weight = total / (array.Length-1);
            }
        }
        else
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].sameWeight())
                    array[i].weight = total * (Mathf.Max(array[i].weight, 0f) / last_total);
            }
        }

        for (int j = 0; j < array.Length; j++) //First call
            array[j].updateWeight();
    }

    void NormalizeScales(ref float min, ref float max)
    {
        if (min < 0f) min = 0f;
        if (max < min) max = min;
    }

    private IEnumerator ValuesParallel()
    {
        //seeds, scale, octaves, persistance, lacunarity
        for (int i=0; i<gen.noiseMapAmount; i++)
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(gen.chunkLength * gen.chunkSpacing, gen.chunkLength * gen.chunkSpacing,
                gen.seeds[i], gen.noiseScale, gen.octaves, gen.persistance, gen.lacunarity, chunkOffset);

            for (int y = 0; y < gen.chunkLength * gen.chunkSpacing; y++)
            {
                for (int x = 0; x < gen.chunkLength * gen.chunkSpacing; x++)
                {
                    if (noiseMap[x, y] > gen.cut && noiseMap[x, y] < gen.cut + gen.range)
                    {
                        values[x, y] = -10f;
                    }
                    else if (values[x, y] < 0f) continue;
                    else
                    {
                        float dist = 3f * Mathf.Abs(noiseMap[x, y] - gen.cut - gen.range / 2f);

                        if (values[x, y] > 0f) values[x, y] = Mathf.Min(dist, values[x, y]);
                        else values[x, y] = dist;
                    }
                }
            }
        }
        
        GenerateChunkObjects();



        DrawPixels(); //se puede quitar o cambiar por textura

        yield return null;
    }

    public void GenerateChunkObjects()
    {
        for (int y = 0; y < gen.chunkLength * gen.chunkSpacing; y+=gen.chunkSpacing)
        {
            for (int x = 0; x < gen.chunkLength * gen.chunkSpacing; x+= gen.chunkSpacing)
            {
                //if (values[x + 2, y + 2] < 0f) continue; //el centro tiene camino, no aparece nada


                //comprobar 5x5
                Vector2 relSpawn = Vector2.zero;
                float totalValue = 0f;

                for (int subY = y; subY < y + gen.chunkSpacing; subY++)
                {
                    for (int subX = x; subX < x + gen.chunkSpacing; subX++)
                    {
                        if (values[subX, subY] > 0f)
                        {
                            totalValue += values[subX, subY];
                            relSpawn += new Vector2(subX, subY) * values[subX, subY];
                        }
                    }
                }
                float pseudoRand = totalValue * 100f;
                relSpawn = relSpawn / totalValue;
                totalValue /= (float)(gen.chunkSpacing * gen.chunkSpacing);

                //el SPAWN RELATIVO podria ser diferente para arbol o hierba

                //poner mas tipos de cosas seria mejor (NO ALEATORIAS, DEBEN SALIR SIEMPRE IGUAL)
                Vector3 spPosition = transform.position + 
                                        new Vector3((gen.chunkLength * gen.chunkSpacing)/2f - relSpawn.x, 
                                                    0f, 
                                                    (gen.chunkLength * gen.chunkSpacing)/2f - relSpawn.y);

                if (totalValue > treeLimit)
                    CreateNewObject(minTreeScale, maxTreeScale, totalValue, pseudoRand, spPosition, ref treeObjects);
                
                else if (totalValue > grassLimit)
                    CreateNewObject(minGrassScale, maxGrassScale, totalValue, pseudoRand, spPosition, ref grassObjects);
                
                
            }
        }
    }

    void CreateNewObject(float minScale, float maxScale, float totalValue, float pseudoRand, Vector3 spPosition, ref TerrainObject[] list)
    {
        float s = (minScale + totalValue + pseudoRand % (maxScale - minScale)) * globalScale;

        GameObject instance = GetInstanceFromList(pseudoRand % 1f, ref list);
        if (instance == null) return;
        
        GameObject newObj = Instantiate(instance, spPosition, Quaternion.Euler(0f, pseudoRand % 360f, 0f), transform);
        newObj.transform.localScale = new Vector3(s, s, s);
    }

    GameObject GetInstanceFromList(float random, ref TerrainObject[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (random < list[i].weight)
                return list[i].asset;

            random -= list[i].weight;
        }
        return null;
    }


    void DrawPixels()
    {
        Texture2D texture = new Texture2D(gen.chunkLength * gen.chunkSpacing, gen.chunkLength * gen.chunkSpacing);

        Color[] colourMap = new Color[gen.chunkLength * gen.chunkSpacing * gen.chunkLength * gen.chunkSpacing];

        for (int y = 0; y < gen.chunkLength * gen.chunkSpacing; y++)
        {
            for (int x = 0; x < gen.chunkLength * gen.chunkSpacing; x++)
            {
                
                if (values[x, y] < 0f)
                {
                    colourMap[y * gen.chunkLength * gen.chunkSpacing + x] = pathColor;
                }
                else
                {
                    colourMap[y * gen.chunkLength * gen.chunkSpacing + x] = Color.Lerp(grassColorInit, 
                                                                                       grassColorEnd, 
                                                                                       Mathf.Min((values[x, y] - gen.cut)*5f, 
                                                                                       1.0f));
                }
            }
        }

        texture.SetPixels(colourMap);
        texture.filterMode = FilterMode.Point;
        texture.Apply();

        Material newMat = new Material(textureRenderer.material);

        textureRenderer.material = newMat;
        textureRenderer.sharedMaterial.mainTexture = texture;
    }
}