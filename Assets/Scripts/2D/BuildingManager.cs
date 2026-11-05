using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    
    [SerializeField]
    private List<(Vector2Int center, int height, int width)> buildings = new List<(Vector2Int, int, int)>();

    [SerializeField]
    private V2JonStam fluidSimulation; // Reference to your fluid simulation script

    [SerializeField]
    private Transform planeTransform;  // Reference to the Plane GameObject

    [SerializeField]
    private GameObject buildingPrefab; // Prefab of the building

    public bool buildingAdded = false;

    private void Start()
    {
        ////Layout for wind tunnel validation test
        //AddBuilding(new Vector2Int(30, 160), 1, 11);
        //AddBuilding(new Vector2Int(30 + 22, 160), 1, 11);
        //AddBuilding(new Vector2Int(30 + 44 , 160), 1, 11);

        //AddBuilding(new Vector2Int(30 , 160 - 22), 1, 11);
        //AddBuilding(new Vector2Int(30 + 22 , 160 - 22), 1, 11);
        //AddBuilding(new Vector2Int(30 + 44 , 160 - 22), 1, 11);

        //AddBuilding(new Vector2Int(30, 160 - 44), 1, 11);
        //AddBuilding(new Vector2Int(30 + 22, 160 - 44), 1, 11);
        //AddBuilding(new Vector2Int(30 + 44, 160 - 44), 1, 11);
        //0.232, 0.092


        ApplyBuildingsToSimulation();
    }

    private void Update()
    {
        if (buildingAdded)
        {
            ApplyBuildingsToSimulation();
        }
    }

    public void AddBuilding(Vector2Int center, int height, int width)
    {
        buildingAdded = true;
        buildings.Add((center, height, width));
    }

    public void SpawnBuilding(Vector2 gridCoordinates)
    {
        // Convert grid coordinates to world position relative to the plane
        Vector3 localPosition = new Vector3(gridCoordinates.x, 0, gridCoordinates.y);

        // Instantiate the building prefab at the world position
        GameObject newBuilding = Instantiate(buildingPrefab,localPosition, Quaternion.identity);

    }


    private void ApplyBuildingsToSimulation()
    {
        // Clear previous buildings in the fluid simulation
        fluidSimulation.ClearBuildings();

        // Add the current list of buildings to the fluid simulation
        foreach ((Vector2Int center, int height, int width) in buildings)
        {
            if (width % 2 != 0) //is odd (has center point)
            {
                int halfWidth = (width - 1) / 2;

                // Loop through the squares around the center point based on the width
                for (int x = center.x - halfWidth; x <= center.x + halfWidth; x++)
                {
                    for (int y = center.y - halfWidth; y <= center.y + halfWidth; y++)
                    {
                        if (x != 0 && y != 0)
                        {
                            fluidSimulation.AddBuildings(x, y);
                            Vector2 gridPos = new Vector2(x, y);
                            SpawnBuilding(gridPos);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Buildings need to be odd");
            }

        }
        buildingAdded = false;
     }
}

