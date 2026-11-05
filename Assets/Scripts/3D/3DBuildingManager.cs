using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Text;
using System;

public class V3BuildingManager : MonoBehaviour
{
    [SerializeField]
    private V3JonStam fluidSimulation; // Reference to your fluid simulation script
    [SerializeField]
    private GameObject buildingPrefab; // Prefab of the building
    [SerializeField]
    public bool preMakeCity;

    public bool buildingAdded = false;


    private List<(Vector2Int center, int height, int width)> buildings = new List<(Vector2Int, int, int)>();
    private List<GameObject> buildingObjects = new List<GameObject>();

    private string filePath;


    private void Start()
    {
        filePath = Application.persistentDataPath + "/buildings.json";
        //PlaceBuildingsWithDistribution();

        //AddBuilding(new Vector2Int(30, 90)/2, 3, 5);
        //AddBuilding(new Vector2Int(70, 20)/2, 3, 5);
        //AddBuilding(new Vector2Int(20, 100)/2, 3, 5);

        //AddBuilding(new Vector2Int(54, 17) / 2, 3, 5);
        //AddBuilding(new Vector2Int(48, 74) / 2, 3, 5);
        //AddBuilding(new Vector2Int(100, 92) / 2, 3, 5);

        //AddBuilding(new Vector2Int(10, 10) / 2, 3, 5);
        //AddBuilding(new Vector2Int(101, 34) / 2, 3, 5);
        //AddBuilding(new Vector2Int(20, 45) / 2 , 3, 5);

        //ApplyBuildingsToSimulation();
        if (preMakeCity)
        {
            PlaceBuildingsWithDistribution();
            ApplyBuildingsToSimulation();
        }
    }

    private void Update()
    {
        if (buildingAdded)
        {
            ApplyBuildingsToSimulation();
        }
    }

    public void PlaceBuildingsWithDistribution()
    {
        // Distribution parameters
        int gridSize = fluidSimulation.N;
        double muX = gridSize / 2.0;
        double muY = gridSize / 2.0;
        double sigmaX = gridSize / 4.0;
        double sigmaY = gridSize / 4.0;
        double rho = 0;

        for (int gridX = 1; gridX < gridSize-1; gridX++)
        {
            for (int gridY = 1; gridY < gridSize-1; gridY++)
            {
                double probability = GetBivariateGuassian(muX, sigmaX, muY, sigmaY, gridX, gridY, rho) * 7000;
                

                // Bias for random placement - higher probability has more chance
                if (ShouldPlaceBuilding(probability))
                {
                    int maxHeight = fluidSimulation.yN;
                    int height = (int)Math.Round(probability * maxHeight);
                    height = Math.Min(height, maxHeight);

                    AddBuilding(new Vector2Int(gridX, gridY), height, 1);
                }
            }
        }
        ApplyBuildingsToSimulation();
    }

    private bool ShouldPlaceBuilding(double probability)
    {
        System.Random random = new System.Random();
        double threshold = 0.1; //threshold to control density

        return random.NextDouble() < probability * threshold;
    }

    public static double GetBivariateGuassian(double muX, double sigmaX, double muY, double sigmaY, double x, double y, double rho)
    {
        double sigmaXSquared = Math.Pow(sigmaX, 2); 
        double sigmaYSquared = Math.Pow(sigmaY, 2);

        double dX = x - muX;
        double dY = y - muY;

        double exponent = -0.5;
        double normaliser = 2 * Math.PI * sigmaX * sigmaY;
        if (rho != 0)
        {
            normaliser *= Math.Sqrt(1 - Math.Pow(rho, 2));
            exponent /= 1 - Math.Pow(rho, 2);
        }

        double sum = Math.Pow(dX, 2) / sigmaXSquared;
        sum += Math.Pow(dY, 2) / sigmaYSquared;
        sum -= 2 * rho * dX * dY / (sigmaX * sigmaY);

        exponent *= sum;

        return Math.Exp(exponent) / normaliser;
    }

    public void SaveBuildings()
    {
        StringBuilder fileContent = new StringBuilder();

        foreach ((Vector2Int center, int height, int width) in buildings)
        {
            fileContent.AppendLine($"{center.x},{center.y},{height},{width}"); // CSV format
        }

        File.WriteAllText(filePath, fileContent.ToString());
    }

    public void LoadBuildings()
    {
        if (File.Exists(filePath))
        {
            RemoveBuildings(); // Start with a fresh list

            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 4)
                {
                    try
                    {
                        int x = int.Parse(parts[0]);
                        int y = int.Parse(parts[1]);
                        int height = int.Parse(parts[2]);
                        int width = int.Parse(parts[3]);
                        buildings.Add((new Vector2Int(x, y), height, width));
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Debug.LogError("Invalid building data in save file: " + line + ex);
                    }
                    catch (FormatException ex)
                    {
                        Debug.LogError("Error parsing building data: " + line + ex);
                    }
                }
                else
                {
                    Debug.LogError("Invalid line format in save file: " + line);
                }
            }

            ApplyBuildingsToSimulation();
        }
        else
        {
            Debug.Log("Building save file not found.");
        }
    }



    public void AddBuilding(Vector2Int center, int height, int width)
    {
        buildingAdded = true;
        buildings.Add((center, height, width));
    }

    public void RemoveBuildings()
    {
        buildings.Clear();
        foreach (GameObject building in buildingObjects) 
        {
            Destroy(building);
        }
        buildingObjects.Clear();
    }

    public void SpawnBuilding(Vector3 gridCoordinates)
    {
        Debug.Log("ADDED");
        // Instantiate the building prefab at the world position
        GameObject newBuilding = Instantiate(buildingPrefab, gridCoordinates, Quaternion.identity);
        buildingObjects.Add(newBuilding);

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
                    for (int z = center.y - halfWidth; z <= center.y + halfWidth; z++)
                    {
                        for (int y = 1; y < height; y++)
                        {
                            if (x != 0 && z != 0)
                            {
                                try
                                {
                                    fluidSimulation.AddBuildings(x, y, z);
                                    Vector3 gridPos = new Vector3(x, y, z);
                                    SpawnBuilding(gridPos);
                                    
                                }
                                catch
                                {
                                    Debug.Log("Index out of range error");
                                }
                            }
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
