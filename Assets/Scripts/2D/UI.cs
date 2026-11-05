using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] 
    private V2JonStam flowFieldSimulation;

    [SerializeField]
    private BuildingManager buildingManager;

    int gridSize;
    bool running = false;

    private void Start()
    {
         gridSize = flowFieldSimulation.N;
    }

    void Update()
    {
        getGridCoords();
        stopStartSim();
    }

    public void stopStartSim()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (running)
            {
                running = false;
                flowFieldSimulation.shouldRun = false;
            }
            else
            {
                running = true;
                flowFieldSimulation.shouldRun = true;
            } 
        }
       
    }

    public Vector2 getGridCoords()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Get the UV coordinates of the hit point
                Vector2 uv = hit.textureCoord;

                // Convert UV coordinates to grid coordinates
                int x = Mathf.FloorToInt(uv.x * gridSize);
                int y = Mathf.FloorToInt(uv.y * gridSize);

                //Add building to sim
               Vector2Int pos = new Vector2Int(x, y);
               buildingManager.AddBuilding(pos, 1, 3);

                Vector2 MeasuredwindSpeed = new Vector2(flowFieldSimulation.Velx[x, y], flowFieldSimulation.Vely[x, y]);
                Vector2 windSpeed = new Vector2(0.232f, 0.092f);
                Vector2 normalizedWindSpeed = MeasuredwindSpeed/windSpeed;

                Debug.Log("Grid Coordinates: (" + x + ", " + y + ")" + "WindSpeed = " + normalizedWindSpeed.magnitude);

                
            }
        }
        else
        {
            return Vector2.zero;
        }
        return Vector2.zero;
    }
}


