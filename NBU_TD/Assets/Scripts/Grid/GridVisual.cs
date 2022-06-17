using UnityEngine;
using System.Collections.Generic; 

public class GridVisual : MonoBehaviour
{
    
    public void CreateGridVisual(int hight, int width)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4 * (width * hight)];
        Vector2[] uv = new Vector2[4 * (width * hight)];
        int[] triangles = new int[6 * (width * hight)];
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < hight; j++)
            {
                int index = i * hight + j;

                vertices[0 + 4 * index] = new Vector3(1 * i, 1 * j);
                vertices[1 + 4 * index] = new Vector3(1 * i, 1 * (j + 1));
                vertices[2 + 4 * index] = new Vector3(1 * (i + 1), 1 * (j + 1));
                vertices[3 + 4 * index] = new Vector3(1 * (i + 1), 1 * j);

                uv[0 + 4 * index] = new Vector2(0, 0);
                uv[1 + 4 * index] = new Vector2(0, 1);
                uv[2 + 4 * index] = new Vector2(1, 1);
                uv[3 + 4 * index] = new Vector2(1, 0);

                triangles[index * 6 + 0] = 0 + 4 * index;
                triangles[index * 6 + 1] = 1 + 4 * index;
                triangles[index * 6 + 2] = 2 + 4 * index;

                triangles[index * 6 + 3] = 0 + 4 * index;
                triangles[index * 6 + 4] = 2 + 4 * index;
                triangles[index * 6 + 5] = 3 + 4 * index;


            }
        }

        GiveColor(vertices, colors, hight);

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.colors = colors;
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void GiveColor(Vector3 [] vertices, Color[] colors, int hight)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = Color.white;
        }
    }

    public void ChangeColorsForCell(List<int[]> indexs, Color color)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        Color[] colors = mesh.colors;

        foreach(int[] index in indexs)
        {
            for (int j = 0; j < index.Length; j++)
            {
                colors[index[j]] = color;
            }
        }

        mesh.colors = colors;
    }
}
