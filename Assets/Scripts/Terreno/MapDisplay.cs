using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    [Range(0, 1)]
    public float cut = 0.45f;
    [Range(0, 1)]
    public float range = 0.1f;

    public bool mode = true;

    

    public void DrawNoiseMap(float[,] noiseMap, float[,] noiseMap2)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colourMap = new Color[width * height];

        for (int y = 0; y<height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (mode)
                    colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);

                //APLICAR MAPA DE RUIDO
                else
                {
                    if ((noiseMap[x, y] > cut && noiseMap[x, y] < cut + range) || (noiseMap2[x, y] > cut && noiseMap2[x, y] < cut + range))
                    {
                        colourMap[y * width + x] = Color.white;
                    }
                    else
                    {
                        float dist = Mathf.Min(Mathf.Abs(noiseMap[x, y] - cut - range / 2f), Mathf.Abs(noiseMap2[x, y] - cut - range / 2f));
                        colourMap[y * width + x] = Color.red * dist*3f;
                    }
                }
                
            }
        }

        texture.SetPixels(colourMap);
        texture.Apply();
        
        textureRenderer.sharedMaterial.mainTexture = texture;
        //textureRenderer.transform.localScale = new Vector3(width, height, 1);
    }
}
