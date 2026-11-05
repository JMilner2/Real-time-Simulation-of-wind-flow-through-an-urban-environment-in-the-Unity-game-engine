using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class V3JonStam : MonoBehaviour
{
    [SerializeField]
    public int N = 32; //SIZE
    [SerializeField]
    public int yN = 10;
    [SerializeField]
    public int iter = 4;  //Iterations
    [SerializeField]
    public float dt = 0.1f;
    [SerializeField]
    public float windSpeedX = 0.1f;
    [SerializeField]
    public float windSpeedY = 0.0f;
    [SerializeField]
    public float windSpeedZ = 0.0f;
    [SerializeField]
    public float diffusion = 0;
    [SerializeField]
    public float viscosity = 0.0000148f;
    [SerializeField]
    public int yPlaneToVisualise = 2;

    public Gradient windSpeedGradient;

    private float[,,] s;
    public float[,,] density;

    private float[,,] Velx;
    private float[,,] Vely;
    private float[,,] Velz;

    private float[,,] PreVelx;
    private float[,,] PreVely;
    private float[,,] PreVelz;

    private bool[,,] Buildings;

    private Texture2D windTexture;

    public bool shouldRun = true;

    public bool showAllGizmos = false;


    private void Start()
    {
        gameObject.transform.position = (new Vector3((N / 2)-0.5f, 0, (N / 2)-0.5f));
        shouldRun = false;
        InitializeField();
        CreateWindSpeedGradient();
        CreateWindTexture();
        AddDensity();
    }

    private void Update()
    {
        if (shouldRun)
        {
            FluidSolverLoop();
        }
        
    }

    private void FixedUpdate()
    {

        AddVelocity(true,false,windSpeedX, windSpeedY, windSpeedZ);
        ApplyWindTexture();

        //long totalMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
        //Debug.Log("Total Allocated Memory: " + totalMemory / (1024 * 1024) + " MB");

    }

    public void FluidSolverLoop()
    {
        diffuse(1, PreVelx, Velx, viscosity, dt);
        diffuse(2, PreVely, Vely, viscosity, dt);
        diffuse(3, PreVelz, Velz, viscosity, dt);

        project(PreVelx, PreVely, PreVelz, Velx, Vely);

        advect(1, Velx, PreVelx, PreVelx, PreVely,PreVelz, dt);
        advect(2, Vely, PreVely, PreVelx, PreVely, PreVelz, dt);
        advect(3, Velz, PreVelz, PreVelx, PreVely, PreVelz, dt);

        project(Velx, Vely, Velz, PreVelx, PreVely);

        diffuse(0, s, density, diffusion, dt);
        advect(0, density, s, Velx, Vely, Velz, dt);

    }

    void InitializeField()
    {
        s = new float[N, yN, N];
        density = new float[N, yN, N];

        Velx = new float[N, yN, N];
        Vely = new float[N, yN, N];
        Velz = new float[N, yN, N];

        PreVelx = new float[N, yN, N];
        PreVely = new float[N, yN, N];
        PreVelz = new float[N, yN, N];

        Buildings = new bool[N, yN, N];

    }

    public void AddBuildings(int x, int y, int z)
    {
        Debug.Log("ADDING");
        Buildings[x, y, z] = true;
    }

    public void ClearBuildings()
    {
        Buildings = new bool[N, yN, N];
    }

    public void AddDensity()
    {
        float minDensity = 0.0f;
        float maxDensity = 0.5f; 

        for (int i = 1; i < N - 1; i++)
        {
            for (int j = 1; j < yN-1; j++)
            {
                for (int k = 1; k < N-1; k++)
                {
                    if (Buildings[i,2,k] == true)
                    { 
                        density[i, 2, k] = minDensity;
                    }
                    else 
                    {
                        density[i, 2, k] += maxDensity;
                    }
                }
            }
        }
    }


void AddVelocity(bool zPlane, bool xPlane, float amountX, float amountY, float amountZ)
    {
        if (zPlane){
            for (int i = 1; i < yN - 1; i++){
                for (int j = 1; j < N - 1; j++){
                    Velx[1, i, j] = amountX;
                    Vely[1, i, j] = amountY;
                    Velz[1, i, j] = amountZ;
                }

            }
        }
        if (xPlane){
            for (int i = 1; i < yN - 1; i++){
                for (int j = 1; j < N - 1; j++){
                    Velx[j, i, 1] = amountX;
                    Vely[j, i, 1] = amountY;
                    Velz[j, i, 1] = amountZ;
                }

            }
        }

    }
       


    void diffuse(int b, float[,,] x, float[,,] x0, float diff, float dt)
    {
        float a = dt * diff * (N - 2) * (yN - 2) * (N - 2);
        gaussSeidelSolver(b, x, x0, a, 1 + 6 * a);
    }

    void gaussSeidelSolver(int b, float[,,] x, float[,,] x0, float a, float c)
    {
        float cRecip = 1.0f / c;
        for (int k = 0; k < iter; k++){
            for (int m = 1; m < N - 1; m++){
                for (int j = 1; j < yN - 1; j++){
                    for (int i = 1; i < N - 1; i++){
                        if (Buildings[i,j,m] == true){
                            x[i, j, m] = 0;
                            continue;}
                        x[i, j, m] =
                            (x0[i, j, m]
                                + a * (x[i + 1, j, m]
                                        + x[i - 1, j, m]
                                        + x[i, j + 1, m]
                                        + x[i, j - 1, m]
                                        + x[i, j, m + 1]
                                        + x[i, j, m - 1]
                               )) * cRecip;
                    }
                }
            }
            set_bnd(b, x);
        }
    }

    void project(float[,,] velocX, float[,,] velocY, float[,,] velocZ, float[,,] p, float[,,] div)
    {
        for (int k = 1; k < N - 1; k++)
        {
            for (int j = 1; j < yN - 1; j++)
            {
                for (int i = 1; i < N - 1; i++)
                {
                    div[i, j, k] = -0.5f * (
                             velocX[i + 1, j, k]
                            - velocX[i - 1, j, k]
                            + velocY[i, j + 1, k]
                            - velocY[i, j - 1, k]
                            + velocZ[i, j, k + 1]
                            - velocZ[i, j, k - 1]
                        ) / N;
                    p[i, j, k] = 0;
                }
            }
        }
        set_bnd(0, div);
        set_bnd(0, p);
        gaussSeidelSolver(0, p, div, 1, 6);

        for (int k = 1; k < N - 1; k++){
            for (int j = 1; j < yN - 1; j++){
                for (int i = 1; i < N - 1; i++){
                    velocX[i, j, k] -= 0.5f * (p[i + 1, j, k]
                                                    - p[i - 1, j, k]) * N;
                    velocY[i, j, k] -= 0.5f * (p[i, j + 1, k    ]
                                                    - p[i, j - 1, k]) * yN;
                    velocZ[i, j, k] -= 0.5f * (p[i, j, k + 1]
                                                    - p[i, j, k - 1]) * N;
                }
            }
        }
        set_bnd(1, velocX);
        set_bnd(2, velocY);
        set_bnd(3, velocZ);
    }


    void advect(int b, float[,,] d, float[,,] d0, float[,,] velocX, float[,,] velocY, float[,,] velocZ, float dt)
    {
        float i0, i1, j0, j1, k0, k1;

        float dtx = dt * (N - 2);
        float dty = dt * (yN - 2);
        float dtz = dt * (N - 2);

        float s0, s1, t0, t1, u0, u1;
        float tmp1, tmp2, tmp3, x, y, z;

        float Nfloat = N;
        float yNfloat = yN;
        float ifloat, jfloat, kfloat;
        int i, j, k;

        for (k = 1, kfloat = 1; k < N - 1; k++, kfloat++)
        {
            for (j = 1, jfloat = 1; j < yN - 1; j++, jfloat++)
            {
                for (i = 1, ifloat = 1; i < N - 1; i++, ifloat++)
                {
                    if (Buildings[i, j, k] == true)
                    {
                        d[i, j, k] = 0;
                        continue;
                    }
                    tmp1 = dtx * velocX[i, j, k];
                    tmp2 = dty * velocY[i, j, k];
                    tmp3 = dtz * velocZ[i, j, k];
                    x = ifloat - tmp1;
                    y = jfloat - tmp2;
                    z = kfloat - tmp3;

                    if (x < 0.5f) x = 0.5f;
                    if (x > Nfloat + 0.5f) x = Nfloat + 0.5f;
                    i0 = Mathf.Floor(x);
                    i1 = i0 + 1.0f;
                    if (y < 0.5f) y = 0.5f;
                    if (y > yNfloat + 0.5f) y = yNfloat + 0.5f;
                    j0 = Mathf.Floor(y);
                    j1 = j0 + 1.0f;
                    if (z < 0.5f) z = 0.5f;
                    if (z > Nfloat - 1f) z = Nfloat - 1.6f;
                    k0 = Mathf.Floor(z);
                    k1 = k0 + 1.0f;

                    s1 = x - i0;
                    s0 = 1.0f - s1;
                    t1 = y - j0;
                    t0 = 1.0f - t1;
                    u1 = z - k0;
                    u0 = 1.0f - u1;

                    int i0i = Mathf.RoundToInt(i0);
                    int i1i = Mathf.RoundToInt(i1);
                    int j0i = Mathf.RoundToInt(j0);
                    int j1i = Mathf.RoundToInt(j1);
                    int k0i = Mathf.RoundToInt(k0);
                    int k1i = Mathf.RoundToInt(k1);


                    d[i, j, k] =

                      s0 * (t0 * (u0 * d0[i0i, j0i, k0i]
                            + u1 * d0[i0i, j0i, k1i])
                        + (t1 * (u0 * d0[i0i, j1i, k0i]
                            + u1 * d0[i0i, j1i, k1i])))
                     + s1 * (t0 * (u0 * d0[i1i, j0i, k0i]
                            + u1 * d0[i1i, j0i, k1i])
                        + (t1 * (u0 * d0[i1i, j1i, k0i]
                            + u1 * d0[i1i, j1i, k1i])));
                }
            }
        }
        set_bnd(b, d);
    }



    void set_bnd(int b, float[,,] x)
    {
        for (int i = 1; i < N - 1; i++)
        {
            for (int j = 1; j < yN - 1; j++)
            {
                for (int k = 1; k < N - 1; k++)
                {
                    if (Buildings[i, j, k] == true)
                    {
                        x[i, j, k] = 0;
                    }


                    if(i == N - 2 && b == 1)
                    {
                        x[i, j, k] = 0.5f;
                    }
                    if (i == N - 2 && b == 2)
                    {
                        x[i, j, k] = 0;
                    }
                    if (i == N - 2 && b == 3)
                    {
                        x[i, j, k] = 0;
                    }
                }
            }
        }

        x[0, 0, 0] = 0.33f * (x[1, 0, 0]
                                      + x[0, 1, 0]
                                      + x[0, 0, 1]);
        x[0, yN - 1, 0] = 0.33f * (x[1, yN - 1, 0]
                                      + x[0, yN - 2, 0]
                                      + x[0, yN - 1, 1]);
        x[0, 0, N - 1] = 0.33f * (x[1, 0, N - 1]
                                      + x[0, 1, N - 1]
                                      + x[0, 0, N-2]);
        x[0, yN - 1, N - 1] = 0.33f * (x[1, yN - 1, N - 1]
                                      + x[0, yN - 2, N - 1]
                                      + x[0, yN - 1, N - 2]);
        x[N - 1, 0, 0] = 0.33f * (x[N - 2, 0, 0]
                                      + x[N - 1, 1, 0]
                                      + x[N - 1, 0, 1]);
        x[N - 1, yN - 1, 0] = 0.33f * (x[N - 2, yN - 1, 0]
                                      + x[N - 1, yN - 2, 0]
                                      + x[N - 1, yN - 1, 1]);
        x[N - 1, 0, N - 1] = 0.33f * (x[N - 2, 0, N - 1]
                                      + x[N - 1, 1, N - 1]
                                      + x[N - 1, 0, N - 2]);
        x[N - 1, yN - 1, N - 1] = 0.33f * (x[N - 2, yN - 1, N - 1]
                                      + x[N - 1, yN - 2, N - 1]
                                      + x[N - 1, yN - 1, N - 2]);

    }

    public void UpdateYPlaneToVisualise(int yVal)
    {
      yPlaneToVisualise = yVal;
    }

    void CreateWindTexture()
    {
        windTexture = new Texture2D(N, N);
        windTexture.filterMode = FilterMode.Bilinear;
        windTexture.wrapMode = TextureWrapMode.Clamp;

        Material windMaterial = GetComponent<Renderer>().material;
        windMaterial.mainTexture = windTexture;
    }

    void ApplyWindTexture()
    {
        int yPlane = yPlaneToVisualise; // The Y-plane to visualize 

        for (int x = 1; x < N - 1; x++)
        {
            for (int z = 1; z < N - 1; z++)
            {
                if (Buildings[x,yPlane,z] == true)
                {
                    windTexture.SetPixel(x, z, Color.black);
                    continue;
                }

                Vector3 windVector = new Vector3(Velx[x, yPlane, z], Velz[x, yPlane, z]);
                float windSpeed = windVector.magnitude;

                Color windColor = windSpeedGradient.Evaluate(windSpeed);

                windTexture.SetPixel(x, z, windColor);
            }
        }
        gameObject.transform.position = (new Vector3((N / 2) - 0.5f, yPlane, (N / 2) - 0.5f));
        windTexture.Apply();
    }


    void CreateWindSpeedGradient()
    {
        windSpeedGradient = new Gradient();

        // Gradient color keys
        GradientColorKey[] colorKeys = new GradientColorKey[5];
        colorKeys[0].color = Color.blue;
        colorKeys[0].time = 0.0f;
        colorKeys[1].color = Color.cyan;
        colorKeys[1].time = 0.075f;
        colorKeys[2].color = Color.green;
        colorKeys[2].time = 0.15f;
        colorKeys[3].color = Color.yellow;
        colorKeys[3].time = 0.225f;
        colorKeys[4].color = Color.red;
        colorKeys[4].time = 0.3f;

        // Gradient alpha keys
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.0f;
        alphaKeys[1].alpha = 1.0f;
        alphaKeys[1].time = 1.0f;

        // Assign the keys to the gradient
        windSpeedGradient.SetKeys(colorKeys, alphaKeys);
    }

    void OnDrawGizmos()
    {
        if (Velx != null && Vely != null && Buildings != null)
        {
            float arrowScale = 5.0f;

            for (int x = 0; x < N - 1; x++)
            {
                for (int y = 0; y < yN - 1; y++)
                {
                    for (int z = 0; z < N - 1; z++)
                    {
                        bool isNearBuilding;

                        if (!showAllGizmos)
                        {
                            isNearBuilding = false;

                            // Check +1, -1, +2, -2 in each direction
                            foreach (int offset in new[] { -2, -1, 1, 2 })
                            {
                                if (x + offset >= 0 && x + offset < N && Buildings[x + offset, y, z] == true) { isNearBuilding = true; break; }
                                if (y + offset >= 0 && y + offset < yN && Buildings[x, y + offset, z] == true) { isNearBuilding = true; break; }
                                if (z + offset >= 0 && z + offset < N && Buildings[x, y, z + offset] == true) { isNearBuilding = true; break; }
                            }
                        }
                        else
                        {
                            isNearBuilding = true;
                        }

                        if (isNearBuilding)
                        {
                            Vector3 position = new Vector3(x, y, z);
                            Vector3 direction = new Vector3(Velx[x, y, z], Vely[x, y, z], Velz[x, y, z]) * arrowScale;
                            if (direction.y > 0.05f || -direction.y > 0.05f)
                            {
                                Color windColor = Color.blue;
                                Gizmos.color = windColor;
                            }
                            else if (direction.z > 0.1f || -direction.z > 0.1f)
                            {
                                Color windColor = Color.green;
                                Gizmos.color = windColor;
                            }
                            else
                            {
                                Color windColor = Color.red;
                                Gizmos.color = windColor;
                            }


                            Gizmos.DrawRay(position, direction);
                        }
                    }
                }
            }
        }

    }


}
