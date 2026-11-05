using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using TMPro;
using UnityEngine;

public class V3UI : MonoBehaviour
{
    [SerializeField]
    private V3JonStam flowFieldSimulation;
    [SerializeField]
    private V3BuildingManager buildingManager;
    [SerializeField]
    private FreeFlyCamera cameraScript;

    [SerializeField]
    private TMP_InputField heightInput;
    [SerializeField]
    private TMP_InputField widthInput;
    [SerializeField]
    public TMP_InputField yPlaneInput;

    private int heightVal = 2;
    private int widthVal = 3;
    private int yPlaneVal = 1;


    int gridSize;
    bool running = false;

    private void Start()
    {
        gridSize = flowFieldSimulation.N;
    }

    void Update()
    {
        getGridCoords();
        EscKeyPressed();
    
    }

    public void YPlaneEndEdit()
    {
        try
        {
            yPlaneVal = int.Parse(yPlaneInput.text);
            flowFieldSimulation.UpdateYPlaneToVisualise(yPlaneVal);
        }
        catch
        {
            Debug.Log("Y value must be an int");
        }
        cameraScript.TemporarilyDisableCameraControl(false);

    }

    public void HeightEndEdit()
    {
        try
        {
            heightVal = int.Parse(heightInput.text);
        }
        catch
        {
            Debug.Log("Height value must be an int");
        }
        cameraScript.TemporarilyDisableCameraControl(false);
    }

    public void WidthEndEdit()
    {
        try
        {
            widthVal = int.Parse(widthInput.text);
        }
        catch
        {
            Debug.Log("Width value must be an int");
        }
        cameraScript.TemporarilyDisableCameraControl(false);

    }

    public void GizmoToggle(bool isActive)
    {
        flowFieldSimulation.showAllGizmos = isActive;
        cameraScript.TemporarilyDisableCameraControl(false);
    }

    public void SaveBuildingsButton()
    {
        buildingManager.SaveBuildings();
        cameraScript.TemporarilyDisableCameraControl(false);
    }

    public void LoadBuildingsButton()
    {
        buildingManager.LoadBuildings();
        cameraScript.TemporarilyDisableCameraControl(false);
    }

    public void EscKeyPressed()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (cameraScript._active)
            {
                cameraScript.TemporarilyDisableCameraControl(true);
            }
            else
            {
                cameraScript.TemporarilyDisableCameraControl(false);
            }
        }
    }

    public void PlayButtonPressed()
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
        cameraScript.TemporarilyDisableCameraControl(false);
    }

    public void RemoveBuildingsPressed()
    {
        flowFieldSimulation.ClearBuildings();
        buildingManager.RemoveBuildings();
        cameraScript.TemporarilyDisableCameraControl(false);
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
                buildingManager.AddBuilding(pos, heightVal, widthVal);

                Debug.Log("Grid Coordinates: (" + x + ", " + y + ")");

                return pos;
            }
        }
        else
        {
            return Vector2.zero;
        }
        return Vector2.zero;
    }
}



