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

    public float grassLimit = 0.05f;
    public float treeLimit = 0.15f;

    Color pathColor = new Color(100f / 255f, 147f / 255f, 60f / 255f);
    Color grassColorInit = new Color(60f / 255f, 147f / 255f, 20f / 255f);
    Color grassColorEnd = new Color(15f / 255f, 120f / 255f, 60f / 255f);

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

        StartCoroutine(ValuesParallel());
    }
    
    private void OnValidate()
    {
        NormalizeArray(ref grassObjects);
        NormalizeArray(ref treeObjects);
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

    private IEnumerator ValuesParallel()
    {
        //seeds, scale, octaves, persistance, lacunarity
        float[,] noiseMap1 = Noise.GenerateNoiseMap(gen.chunkLength * gen.chunkSpacing, gen.chunkLength * gen.chunkSpacing,
            gen.seed1, gen.noiseScale,
            gen.octaves, gen.persistance, gen.lacunarity, chunkOffset);
        float[,] noiseMap2 = Noise.GenerateNoiseMap(gen.chunkLength * gen.chunkSpacing, gen.chunkLength * gen.chunkSpacing,
            gen.seed2, gen.noiseScale,
            gen.octaves, gen.persistance, gen.lacunarity, chunkOffset);

        for (int y = 0; y < gen.chunkLength * gen.chunkSpacing; y++)
        {
            for (int x = 0; x < gen.chunkLength * gen.chunkSpacing; x++)
            {
                if ((noiseMap1[x, y] > gen.cut && noiseMap1[x, y] < gen.cut + gen.range)
                    || (noiseMap2[x, y] > gen.cut && noiseMap2[x, y] < gen.cut + gen.range))
                {
                    values[x, y] = -10f;
                }
                else
                {
                    float dist = Mathf.Min(Mathf.Abs(noiseMap1[x, y] - gen.cut - gen.range / 2f),
                        Mathf.Abs(noiseMap2[x, y] - gen.cut - gen.range / 2f));
                    values[x, y] = dist * 3f;
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
                if (values[x + 2, y + 2] < 0f) continue; //el centro tiene camino, no aparece nada


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
                totalValue /= 25f;

                //el SPAWN RELATIVO podria ser diferente para arbol o hierba

                //poner mas tipos de cosas seria mejor (NO ALEATORIAS, DEBEN SALIR SIEMPRE IGUAL)
                Vector3 spPosition = transform.position + new Vector3(25f - relSpawn.x, 0f, 25f - relSpawn.y);

                if (totalValue > treeLimit)
                {
                    float s = (1f + totalValue + pseudoRand % 0.5f)*0.2f;

                    GameObject instance = GetInstanceFromList(pseudoRand % 1f, ref treeObjects);
                    if (instance != null)
                    {
                        GameObject newTree = Instantiate(instance, spPosition, Quaternion.Euler(0f, pseudoRand % 360f, 0f), transform);
                        newTree.transform.localScale = new Vector3(s, s, s);
                    }
                }
                else if (totalValue > grassLimit)
                {
                    float s = (0.6f + totalValue + pseudoRand % 0.5f)*0.2f;

                    GameObject instance = GetInstanceFromList(pseudoRand % 1f, ref grassObjects);
                    if (instance!=null)
                    {
                        GameObject newGrass = Instantiate(instance, spPosition, Quaternion.Euler(0f, pseudoRand % 360f, 0f), transform);
                        newGrass.transform.localScale = new Vector3(s, s, s);
                    }
                }
                
            }
        }
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
                    colourMap[y * gen.chunkLength * gen.chunkSpacing + x] = Color.Lerp(grassColorInit, grassColorEnd, Mathf.Min((values[x, y] - gen.cut)*5f, 1.0f)); // Color.red * values[x, y];
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