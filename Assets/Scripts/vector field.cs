using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vectorfield : MonoBehaviour
{
    public int gridSizeX = 100;
    public int gridSizeY = 100;
    public float windSpeed = 5f;
    public float turbulence = 1f;

    private Vector2[,] windVectorField;
    private Texture2D windTexture;
    [SerializeField]
    private Material windMaterial;
    [SerializeField]
    Vector2 startingWindDirection1;
    [SerializeField]
    Vector2 startingWindDirection2;
    [SerializeField]
    [Range(0, 99)]
    int WindStartX;
    [SerializeField]
    [Range(1, 100)]
    int WindEndX;
    [SerializeField]
    [Range(0, 99)]
    int WindStartY;
    [SerializeField]
    [Range(1, 100)]
    int WindEndY;


    void Start()
    {
        InitializeVectorField();
        CreateWindTexture();
    }

    void Update()
    {
        UpdateWindField();
        ApplyWindTexture();
    }

    void InitializeVectorField()
    {
        windVectorField = new Vector2[gridSizeX, gridSizeY];

        // Set a starting point for the wind at the center of the grid
        Vector2 startingWind1 = startingWindDirection1.normalized;
        Vector2 startingWind2 = startingWindDirection2.normalized;

        for (int y = WindStartY; y < WindEndY; y++)
        {
            windVectorField[0, y] = startingWind1;
        }

        for (int x = WindStartX; x < WindEndX; x++)
        {
            windVectorField[x, 0] = startingWind2;
        }
    }

    void CreateWindTexture()
    {
        windTexture = new Texture2D(gridSizeX, gridSizeY);
        windTexture.filterMode = FilterMode.Bilinear;
        windTexture.wrapMode = TextureWrapMode.Clamp;

        windMaterial = GetComponent<Renderer>().material;
        windMaterial.mainTexture = windTexture;
    }

    void UpdateNeighbors(int x, int y, Vector2 direction)
    {
        // Normalize the direction vector
        Vector2 normalizedDirection = direction;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                // Skip the current cell
                if (i == 0 && j == 0)
                    continue;

                // Calculate the neighbor indices
                int neighborX = x + i;
                int neighborY = y + j;

                // Check if the neighbor is within the grid bounds
                if (neighborX >= 0 && neighborX < gridSizeX && neighborY >= 0 && neighborY < gridSizeY)
                {

                    // Update the neighbor based on the current vector
                    windVectorField[neighborX, neighborY] += direction;
               
                }
            }
        }
    }

    void UpdateWindField()
    {
        // Iterate through the vector field
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 currentVector = windVectorField[x, y];

                // Check if the current vector is non-zero
                if (currentVector != Vector2.zero)
                {
                    // Find neighbors in the direction of the current vector
                    UpdateNeighbors(x, y, currentVector.normalized);
                }
            }
        }
    }

    void ApplyWindTexture()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Get the normalized wind vector
                Vector2 normalizedWind = windVectorField[x, y];

                // Calculate hue based on the direction
                float hue = Mathf.Atan2(normalizedWind.y, normalizedWind.x) / (2 * Mathf.PI) + 0.5f;

                // Calculate brightness based on the magnitude
                float brightness = windVectorField[x, y].magnitude / windSpeed;

                // Map wind vector to color using the gradient
                Color windColor = Color.HSVToRGB(hue, 1.0f, brightness);
                windTexture.SetPixel(x, y, windColor);
            }
        }

        windTexture.Apply();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if (windVectorField != null)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 position = new Vector3(x, 0, y);
                    Vector3 direction = new Vector3(windVectorField[x, y].x, 0, windVectorField[x, y].y);

                    Gizmos.DrawRay(position, direction.normalized);
                }
            }
        }
    }
}
