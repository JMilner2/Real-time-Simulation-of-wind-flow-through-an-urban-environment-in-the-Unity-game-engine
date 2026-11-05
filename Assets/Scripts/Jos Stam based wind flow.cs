using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class JosStambasedwindflow : MonoBehaviour
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

    private float[] s;
    private float[] density;

    private float[] Velx;
    private float[] Vely;
  
    private float[] PreVelx;
    private float[] PreVely;

    private Texture2D windTexture;
    [SerializeField]
    private Material windMaterial;

    private void Start()
    {
        InitializeField();
        CreateWindTexture();
        AddDensity(N / 2, N / 2, 100);

    }

    private void Update()
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
        AddVelocity(10, 10, 1f, 1f);
        
       

    }

    int IX(int x, int y)
    {
        x = Mathf.Clamp(x, 0, N - 1);
        y = Mathf.Clamp(y, 0, N - 1);
        return x + y * N;
    }

    void InitializeField()
    {

        s = new float[N * N];
        density = new float[N * N];

        Velx = new float[N * N];
        Vely = new float[N * N];
       
        PreVelx = new float[N * N];
        PreVely = new float[N * N];
       
    }

    void AddDensity(int x, int y, float amount)
    {
        density[IX(x, y)] += amount;
    }

    void AddVelocity(int x, int y, float amountX, float amountY)
    {
        int index = IX(x, y);

        Velx[index] += amountX;
        Vely[index] += amountY;
    }


    void diffuse(int b, float[] x, float[] x0, float diff, float dt)
    {
        float a = dt * diff * (N - 2) * (N - 2);
        lin_solve(b, x, x0, a, 1 + 6 * a);
    }

    void lin_solve(int b, float[] x, float[] x0, float a, float c)
    {
        float cRecip = 1.0f / c;
        for (int k = 0; k < iter; k++)
        {
            for (int j = 1; j < N - 1; j++)
            {
                for (int i = 1; i < N - 1; i++)
                {
                    x[IX(i, j)] =
                        (x0[IX(i, j)]
                            + a * (x[IX(i + 1, j)]
                                    + x[IX(i - 1, j)]
                                    + x[IX(i, j + 1)]
                                    + x[IX(i, j - 1)]
                            )) * cRecip;
                }
            }
            set_bnd(b, x);
        }
    }

    void project(float[] velocX, float[] velocY, float[] p, float[] div)
    {
        for (int j = 1; j < N - 1; j++)
        {
            for (int i = 1; i < N - 1; i++)
            {
                div[IX(i, j)] = -0.5f * (
                          velocX[IX(i + 1, j)]
                        - velocX[IX(i - 1, j)]
                        + velocY[IX(i, j + 1)]
                        - velocY[IX(i, j - 1)]) / N;
                p[IX(i, j)] = 0;
            }
        }
        set_bnd(0, div);
        set_bnd(0, p);
        lin_solve(0, p, div, 1, 6);

       
        for (int j = 1; j < N - 1; j++)
        {
            for (int i = 1; i < N - 1; i++)
            {
                velocX[IX(i, j)] -= 0.5f * (p[IX(i + 1, j)]
                    - p[IX(i - 1, j)]) * N;
                velocY[IX(i, j)] -= 0.5f * (p[IX(i, j + 1)]
                    - p[IX(i, j - 1)]) * N;
            }
        }
        
        set_bnd(1, velocX);
        set_bnd(2, velocY);
    }


    void advect(int b, float[] d, float[] d0, float[] velocX, float[] velocY, float dt)
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
                tmp1 = dtx * velocX[IX(i, j)];
                tmp2 = dty * velocY[IX(i, j)];
                x = ifloat - tmp1;
                y = jfloat - tmp2;

                if (x < 0.5f) x = 0.5f;
                if (x > Nfloat + 0.5f) x = Nfloat + 0.5f;
                i0 = Mathf.Floor(x);
                i1 = i0 + 1.0f;
                if (y < 0.5f) y = 0.5f;
                if (y > Nfloat + 0.5f) y = Nfloat + 0.5f;
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

                d[IX(i, j)] =
                     s0 * (t0 * d0[IX(i0i, j0i)] + t1 * d0[IX(i0i, j1i)]) +
                     s1 * (t0 * d0[IX(i1i, j0i)] + t1 * d0[IX(i1i, j1i)]);
                                
            }
        }
        set_bnd(b, d);
    }


    void set_bnd(int b, float[] x)
    {
        for (int i = 1; i < N - 1; i++)
        {
            x[IX(i, 0)] = b == 2 ? -x[IX(i, 1)] : x[IX(i, 1)];
            x[IX(i, N - 1)] = b == 2 ? -x[IX(i, N - 2)] : x[IX(i, N - 2)];
        }
        
        
        for (int j = 1; j < N - 1; j++)
        {
            x[IX(0, j)] = b == 1 ? -x[IX(1, j)] : x[IX(1, j)];
            x[IX(N - 1, j)] = b == 1 ? -x[IX(N - 2, j)] : x[IX(N - 2, j)];
        }
        

        x[IX(0, 0)] = 0.5f * (x[IX(1, 0)] + x[IX(0, 1)]);
        x[IX(0, N - 1)] = 0.5f * (x[IX(1, N - 1)] + x[IX(0, N - 2)]);
        x[IX(N - 1, 0)] = 0.5f * (x[IX(N - 2, 0)] + x[IX(N - 1, 1)]);
        x[IX(N - 1, N - 1)] = 0.5f * (x[IX(N - 2, N - 1)] + x[IX(N - 1, N - 2)]);
                                   
    }

    void CreateWindTexture()
    {
        windTexture = new Texture2D(N, N);
        windTexture.filterMode = FilterMode.Bilinear;
        windTexture.wrapMode = TextureWrapMode.Clamp;

        windMaterial = GetComponent<Renderer>().material;
        windMaterial.mainTexture = windTexture;
    }

    void ApplyWindTexture()
    {
        for (int x = 0; x < N; x++)
        {
            for (int y = 0; y < N; y++)
            {
                // Get the wind vector
                Vector2 windVector = new Vector2(Velx[IX(x, y)], Vely[IX(x, y)]);
                // Calculate hue based on the direction
                float hue = Mathf.Atan2(windVector.y, windVector.x) / (2 * Mathf.PI) + 0.5f;
                // Calculate brightness based on the magnitude
                float brightness = windVector.magnitude;
                // Map wind vector to color
                Color windColor = Color.HSVToRGB(hue, 1.0f, brightness);
                windTexture.SetPixel(x, y, windColor);
            }
        }
        windTexture.Apply();
    }






}