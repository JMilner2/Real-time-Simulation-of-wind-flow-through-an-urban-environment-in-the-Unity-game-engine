using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V2JonStam : MonoBehaviour
{
    [SerializeField]
    public int N = 32; //SIZE
    [SerializeField]
    public int iter = 4;  //Iterations
    [SerializeField]
    public float dt = 0.1f;
    [SerializeField]
    public float diffusion = 0;
    [SerializeField]
    public float viscosity = 0.000001f;
    [SerializeField]
    public float windSpeedX = 0.4f;
    [SerializeField]
    public float windSpeedY = 0.0f;
    [SerializeField]

    public Gradient windSpeedGradient;

    private float[,] s;
    private float[,] density;

    public float[,] Velx;
    public float[,] Vely;

    private float[,] PreVelx;
    private float[,] PreVely;

    private float[,] Buildings;

    private Texture2D windTexture;

    public bool shouldRun = false;


    private void Start()
    {
        gameObject.transform.position = (new Vector3((N / 2)-0.5f, 0, (N / 2)-0.5f));
        shouldRun = false;
        InitializeField();
        CreateWindSpeedGradient();
        CreateWindTexture();

    }

    private void Update()
    {
        if (shouldRun)
        {
            fluidSolverLoop();
            AddVelocity(5, N / 2, windSpeedX, windSpeedY);
        }
    }

   

    public void fluidSolverLoop()
    {
        
        diffuse(1, PreVelx, Velx, viscosity, dt);
        diffuse(2, PreVely, Vely, viscosity, dt);

        project(PreVelx, PreVely, Velx, Vely);

        advect(1, Velx, PreVelx, PreVelx, PreVely, dt);
        advect(2, Vely, PreVely, PreVelx, PreVely, dt);

        project(Velx, Vely, PreVelx, PreVely);

        diffuse(0, s, density, diffusion, dt);
        advect(0, density, s, Velx, Vely, dt);

        ApplyWindTexture();
       
    }

    void InitializeField()
    {
        s = new float[N , N];
        density = new float[N , N ];

        Velx = new float[N, N];
        Vely = new float[N, N];
       
        PreVelx = new float[N, N];
        PreVely = new float[N, N];

        Buildings = new float[N, N];
       
    }

    public void AddBuildings(int x, int y)
    {
        Debug.Log("ADDING");
        Buildings[x, y] = 1;
    }

    public void ClearBuildings()
    {
        Buildings = new float[N, N];   
    }

    void AddDensity(int x, int y, float amount)
    {
        for (int i = 1; i < N - 1; i++)
        {
            density[6, i] = amount;
        }
    }

    void AddVelocity(int x, int y, float amountX, float amountY)
    {
        for (int i = 1; i < N - 1; i++)
        {
            Velx[1, i] = amountX;
            Vely[1, i] = amountY;
            Velx[i, 2] = amountX;
            Vely[i, 2] = amountY;

        }
    }


    void diffuse(int b, float[,] x, float[,] x0, float diff, float dt)
    {
        float a = dt * diff * (N - 1) * (N - 1);
        lin_solve(b, x, x0, a, 1 + 4 * a);
    }

    void lin_solve(int b, float[,] x, float[,] x0, float a, float c)
    {
        float cRecip = 1.0f / c;
        for (int k = 0; k < iter; k++)
        {
            for (int j = 1; j < N - 1; j++)
            {
                for (int i = 1; i < N - 1; i++)
                {
                    x[i, j] =
                        (x0[i, j]
                            + a * (x[i + 1, j]
                                    + x[i - 1, j]
                                    + x[i, j + 1]
                                    + x[i, j - 1]
                            )) * cRecip;
                }
            }
            set_bnd(b, x);
        }
    }

    void project(float[,] velocX, float[,] velocY, float[,] p, float[,] div)
    {
        for (int j = 1; j < N - 1; j++)
        {
            for (int i = 1; i < N - 1; i++)
            {
               
                div[i, j] = -0.5f * (
                          velocX[i + 1, j]
                        - velocX[i - 1, j]
                        + velocY[i, j + 1]
                        - velocY[i, j - 1]) / N;
                p[i, j] = 0;
            }
        }
        set_bnd(0, div);
        set_bnd(0, p);
        lin_solve(0, p, div, 1, 4);

       
        for (int j = 1; j < N - 1; j++)
        {
            for (int i = 1; i < N - 1; i++)
            {
               
                velocX[i, j] -= 0.5f * (p[i + 1, j]
                    - p[i - 1, j]) * N;
                velocY[i, j] -= 0.5f * (p[i, j + 1]
                    - p[i, j - 1]) * N;
            }
        }
        
        set_bnd(1, velocX);
        set_bnd(2, velocY);
    }


    void advect(int b, float[,] d, float[,] d0, float[,] velocX, float[,] velocY, float dt)
    {
        float i0, i1, j0, j1;

        float dtx = dt * (N - 2);
        float dty = dt * (N - 2);

        float s0, s1, t0, t1;
        float tmp1, tmp2, x, y;

        float Nfloat = N;
        float ifloat, jfloat;
        int i, j;

        for (j = 1, jfloat = 1; j < N - 1; j++, jfloat++)
        {
            for (i = 1, ifloat = 1; i < N - 1; i++, ifloat++)
            {
               
                tmp1 = dtx * velocX[i, j];
                tmp2 = dty * velocY[i, j];
                x = ifloat - tmp1;
                y = jfloat - tmp2;

                if (x < 0.5f) x = 0.5f;
                if (x > Nfloat + 0.5f) x = Nfloat + 0.5f;
                i0 = Mathf.Floor(x);
                i1 = i0 + 1.0f;
                if (y < 0.5f) y = 0.5f;
                if (y > Nfloat - 1f) y = Nfloat - 1f;
                j0 = Mathf.Floor(y);
                j1 = j0 + 1.0f;

                s1 = x - i0;
                s0 = 1.0f - s1;
                t1 = y - j0;
                t0 = 1.0f - t1;


                int i0i = Mathf.RoundToInt(i0);
                int i1i = Mathf.RoundToInt(i1);
                int j0i = Mathf.RoundToInt(j0);
                int j1i = Mathf.RoundToInt(j1);

                d[i, j] =
                     s0 * (t0 * d0[i0i, j0i] + t1 * d0[i0i, j1i]) +
                     s1 * (t0 * d0[i1i, j0i] + t1 * d0[i1i, j1i]);
                                
            }
        }
        set_bnd(b, d);
    }


    void set_bnd(int b, float[,] x)
    {
      
        for (int i = 1; i < N - 1; i++)
        {
            for(int j = 1; j < N - 1; j++)
            {
                if (Buildings[i, j] == 1)
                {
                    x[i, j] = 0;
                
                }

            }
        }


        //edge cases
        x[0, 0] = 0.5f * (x[1, 0] + x[0, 1]);
        x[0, N - 1] = 0.5f * (x[1, N - 1] + x[0, N - 2]);
        x[N - 1, 0] = 0.5f * (x[N - 2, 0] + x[N - 1, 1]);
        x[N - 1, N - 1] = 0.5f * (x[N - 2, N - 1] + x[N - 1, N - 2]);
                                   
    }

    void CreateWindTexture()
    {
        windTexture = new Texture2D(N, N);
        windTexture.filterMode = FilterMode.Bilinear;
        windTexture.wrapMode = TextureWrapMode.Clamp;

        Material windMaterial =  GetComponent<Renderer>().material;
        windMaterial.mainTexture = windTexture;
    }

    void ApplyWindTexture()
    {
        // Assuming your texture dimensions match N
        for (int x = 1; x < N - 1; x++)
        {
            for (int z = 1; z < N - 1; z++)
            {
                Vector3 windVector = new Vector3(Velx[x, z], Vely[x, z]);
                float windSpeed = windVector.magnitude;

                Color windColor = windSpeedGradient.Evaluate(windSpeed);

                windTexture.SetPixel(x, z, windColor);
            }
        }

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
        colorKeys[1].time = 0.05f;
        colorKeys[2].color = Color.green;
        colorKeys[2].time = 0.1f;
        colorKeys[3].color = Color.yellow;
        colorKeys[3].time = 0.15f;
        colorKeys[4].color = Color.red;
        colorKeys[4].time = 0.2f;

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
        Gizmos.color = Color.black;
        if (Velx != null && Vely != null)
        {
            float arrowScale = 10.0f;
            float gridSpacing = 1f;
            for (int x = 0; x < N - 2; x++)
            {
                for (int y = 0; y < N - 2; y++)
                {
                    Vector3 position = new Vector3(x * gridSpacing, 0,  y * gridSpacing);
                    Vector3 direction = new Vector3(Velx[x, y], 0, Vely[x, y]) * arrowScale;
                    Gizmos.DrawRay(position, direction);
                }
            }
        }
        
    }


}

