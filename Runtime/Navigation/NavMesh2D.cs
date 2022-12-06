using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using System.Linq;
using ComputationGeometry_DOTS;
using UnityEngine.SceneManagement;

namespace Core
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider2D))]
    [RequireComponent(typeof(PolygonCollider2D))]
    [ExecuteInEditMode]
    public class NavMesh2D : MonoBehaviour
    {
        [SerializeField] private NavMeshData navData;
        public ContactFilter2D filter;
        public float boundThickness = 1f;

        [Header("Grid properties")]
        public EGridType gridType;
        private int2 cellSize = new int2(1, 1);
        private GridData gridData;
        
        [Header("Components")]
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private BoxCollider2D volume;
        private PolygonCollider2D navCollider;

        // Editor only variables
#if UNITY_EDITOR
        public bool realtimeCook = true;
        private string assetFilepath;
#endif

        // Start is called before the first frame update
        void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            volume = GetComponent<BoxCollider2D>();

            //grid = new CustomGrid(navData.size.x, navData.size.y, cellSize, volume.bounds.min);
        }

        void Update()
        {
            //CreateGrid();
            volume = GetComponent<BoxCollider2D>();

#if UNITY_EDITOR
            if (realtimeCook)
                BakeNavigationMesh();
#endif
        }

        void OnValidate()
        {
            volume = GetComponent<BoxCollider2D>();
            navCollider = GetComponent<PolygonCollider2D>();
            
            // Cache the current scene name
            string sceneName = SceneManager.GetActiveScene().name;
            
            // Path for the folder which contains all the game scenes assets.
            string scenesFilepath = "Assets/Scenes";
            
            // Check if the Scenes folder exist, if not then create it.
            if(!AssetDatabase.IsValidFolder(scenesFilepath))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            
            // If there's no folder related to the current scene create it.
            if(!AssetDatabase.IsValidFolder(scenesFilepath + "/" + sceneName))
                AssetDatabase.CreateFolder(scenesFilepath, sceneName);
            
            // Path to the folder which contains all the navigation data for the current scene.
            string currentSceneFilepath = "Assets/Scenes/" + sceneName;
            // If there's no folder for navigation data create it.
            if (!AssetDatabase.IsValidFolder(currentSceneFilepath + "/NavigationData"))
                AssetDatabase.CreateFolder(currentSceneFilepath, "NavigationData");
            
            assetFilepath = currentSceneFilepath + "/NavigationData/";
        }
        
        // Update this navigation mesh asset with supplied data.
        // if there's not asset this method will create one.
        private void UpdateNavigationAsset(List<Triangle2D> navigableSurface)
        {
            string[] result = AssetDatabase.FindAssets(gameObject.name + "_Data");
            if (result.Length == 0)
            {
                navData = ScriptableObject.CreateInstance<NavMeshData>();
                navData.name = gameObject.name + "_Data";
                AssetDatabase.CreateAsset(navData, assetFilepath + gameObject.name + "_Data.asset");
                EditorUtility.SetDirty(navData);
                AssetDatabase.SaveAssets();
            }
            else if(navData == null)
                navData = ScriptableObject.CreateInstance<NavMeshData>();

            // Using the navMeshVolume determines the width and height of the nav mesh grid.
            int width = Mathf.FloorToInt(volume.size.x / cellSize.x);
            int height = Mathf.FloorToInt(volume.size.y / cellSize.y);
            
            // Write baked data to a navigation data asset to be used by
            // game entities.
            navData.cellSize = cellSize;
            navData.size = new int2(width, height);
            navData.gridType = gridType;
            navData.origin = (Vector2)volume.bounds.min;
            navData.navigableSurface = navigableSurface.ToArray();

            // Save asset.
            EditorUtility.SetDirty(navData);
            AssetDatabase.SaveAssets();
        }

        public void BakeNavigationMesh()
        {
            // Begin navigation mesh cooking, this method will generate
            // the actual mesh and return the nav collider shape(In triangles).
            List<Triangle2D> navigableSurface = CookNavMesh();
            // Update this NavMesh2D asset with the computed data.
            UpdateNavigationAsset(navigableSurface);
        }
        
        private List<Triangle2D> CookNavMesh()
        {
            // Initialize input data.
            NativeList<float2> vertices = new NativeList<float2>(Allocator.TempJob);
            NativeList<Edge2D> holes = new NativeList<Edge2D>(Allocator.TempJob);
            // Generate nav mesh geometry.
            BuildGeometry(ref vertices, ref holes);
            // Triangulate using input data and generate a collision shape and mesh for the navigation.
            ConstrainedDelaunayTriangulation triangulation = new ConstrainedDelaunayTriangulation(vertices, holes);
            triangulation.Run();
            GenerateNavigationMesh(triangulation.triangles);
            List<Triangle2D> triangles = triangulation.triangles.ToArray().ToList();
            // Free all the memory allocated for the job.
            triangulation.Dispose();
            triangulation.triangles.Dispose();
            vertices.Dispose();
            holes.Dispose();
            return triangles;
        }

        ///<summary>
        /// Build all navigation meshes inside the scene.
        /// You should never call this manually unless you're trying
        /// to write your own runtime navigation mesh generation, but for
        /// 99% of cases you should be fine without calling this method, use
        /// instead the built in support for Runtime navigation mesh generation.
        ///</summary>
        [MenuItem("Window/AI/Navigation 2D/Build Navigation")]
        public static void CookAllNavMeshes()
        {
            NavMesh2D[] navMeshes = GameObject.FindObjectsOfType<NavMesh2D>();
            int length = navMeshes.Length;
            // When there is more 1 or more nav meshes in the scene starts cooking them on different threads.
            if (length > 0)
            {
                NativeArray<JobHandle> handlers = new NativeArray<JobHandle>(length, Allocator.TempJob);
                ConstrainedDelaunayTriangulation[] triangulations = new ConstrainedDelaunayTriangulation[length];
                for (int i = 0; i < length; i++)
                {
                    NavMesh2D navMesh = navMeshes[i];
                    // Initialize input data
                    NativeList<float2> vertices = new NativeList<float2>(Allocator.TempJob);
                    NativeList<Edge2D> holes = new NativeList<Edge2D>(Allocator.TempJob);
                    // Generate this navigation mesh geometry data.
                    navMesh.BuildGeometry(ref vertices, ref holes);
                    // Create triangulation job and schedule it. Each nav mesh triangulation will run
                    // on a separate thread to speed-up the baking process.
                    ConstrainedDelaunayTriangulation triangulation = new ConstrainedDelaunayTriangulation(vertices, holes);
                    JobHandle handle = triangulation.Schedule();
                    // Free allocated input data.
                    vertices.Dispose(handle);
                    holes.Dispose(handle);
                    // Store job handle and job for mesh generation and memory
                    // cleanup later on.
                    handlers[i] = handle;
                    triangulations[i] = triangulation;
                }
                JobHandle.CompleteAll(handlers);
                
                // When triangulation of all meshes has ended start generating
                // the actual navigation mesh for all nav meshes in the scene.
                for(int i = 0; i < length; i++)
                {
                    NavMesh2D navMesh = navMeshes[i];
                    ConstrainedDelaunayTriangulation triangulation = triangulations[i];
                    // Generate the actual navigation mesh
                    navMesh.GenerateNavigationMesh(triangulation.triangles);
                    // Update the navigation mesh asset with the computed data.
                    navMesh.UpdateNavigationAsset(triangulation.triangles.ToArray().ToList());
                }

                // Free all the memory allocated for the jobs.
                handlers.Dispose();
                foreach(ConstrainedDelaunayTriangulation triangulation in triangulations)
                {
                    triangulation.Dispose();
                    triangulation.triangles.Dispose();
                }
            }
        }

        ///<summary>
        /// Generate the nav mesh geometry.
        ///</summary>
        public void BuildGeometry(ref NativeList<float2> vertexBuffer, ref NativeList<Edge2D> holes)
        {
            // Cook mesh data
            List<Collider2D> overlappingColliders = new List<Collider2D>();
            int collidersNumber = volume.OverlapCollider(filter, overlappingColliders);
            if (collidersNumber > 0)
            {
                vertexBuffer.Add(new float2(volume.bounds.min.x, volume.bounds.min.y));
                vertexBuffer.Add(new float2(volume.bounds.max.x, volume.bounds.max.y));
                vertexBuffer.Add(new float2(volume.bounds.min.x + volume.bounds.extents.x * 2, volume.bounds.min.y));
                vertexBuffer.Add(new float2(volume.bounds.max.x - volume.bounds.extents.x * 2, volume.bounds.max.y));
                vertexBuffer.Add(new float2(volume.bounds.min.x + volume.bounds.extents.x, volume.bounds.min.y));
                vertexBuffer.Add(new float2(volume.bounds.max.x - volume.bounds.extents.x, volume.bounds.max.y));

                foreach (Collider2D collider in overlappingColliders)
                {
                    Bounds colliderBounds = collider.bounds;
                    float3 colliderExtents = collider.bounds.extents;

                    // Initialize collider bottom left point
                    float2 bottomLeft = new float2(collider.bounds.min.x, collider.bounds.min.y);
                    bottomLeft.x -= boundThickness;
                    bottomLeft.y -= boundThickness;
                    SetNavPoint(bottomLeft, vertexBuffer, collider);

                    // Initialize collider bottom right point
                    float2 bottomRight = new Vector2(colliderBounds.min.x + colliderExtents.x * 2 + boundThickness,
                                                      colliderBounds.min.y - boundThickness);
                    SetNavPoint(bottomRight, vertexBuffer, collider);

                    // Initialize collider top right point
                    float2 topRight = new float2(collider.bounds.max.x, collider.bounds.max.y);
                    topRight.x += boundThickness;
                    topRight.y += boundThickness;
                    SetNavPoint(topRight, vertexBuffer, collider);

                    // Initialize collider top left point
                    float2 topLeft = new Vector2(colliderBounds.max.x - colliderExtents.x * 2 - boundThickness,
                                                  colliderBounds.max.y + boundThickness);
                    SetNavPoint(topLeft, vertexBuffer, collider);

                    holes.Add(new Edge2D(topRight, bottomRight));
                    holes.Add(new Edge2D(bottomRight, bottomLeft));
                    holes.Add(new Edge2D(bottomLeft, topLeft));
                    holes.Add(new Edge2D(topLeft, topRight));
                }
            }
        }

        // Using nav mesh geometry data, triangulate and generate the final navigation data
        public void GenerateNavigationMesh(NativeArray<Triangle2D> triangles)
        {
            // Build collision shape from triangulation.
            navCollider.pathCount = 0;
            Vector2[] path = new Vector2[3];
            for (int i = 0; i < triangles.Count(); i++)
            {
                Triangle2D triangle = triangles[i];
                float2 offset = new float2(transform.position.x, transform.position.y);
                path[0] = triangle.A - offset;
                path[1] = triangle.B - offset;
                path[2] = triangle.C - offset;
                navCollider.pathCount++;
                navCollider.SetPath(i, path);
            }

            // Create navigation mesh using collision shape.
            if (meshFilter.sharedMesh != null)
                meshFilter.sharedMesh.Clear();

            // Generate Mesh using polygon collider data.
            Mesh mesh = navCollider.CreateMesh(true, true);
            // Convert mesh vertices to local space.
            if (mesh != null)
            {
                Vector3[] vertexBuffer = mesh.vertices;
                for (int i = 0; i < vertexBuffer.Length; i++)
                    vertexBuffer[i] = vertexBuffer[i] - transform.position;
                mesh.vertices = vertexBuffer;
                meshFilter.sharedMesh = mesh;
            }
        }

        private void SetNavPoint(Vector2 point, List<Vector2> vertices, Collider2D ignore)
        {
            Collider2D[] collider = Physics2D.OverlapPointAll(point, filter.layerMask);
            if ((collider.Length == 0 || collider[0] == ignore) && volume.OverlapPoint(point))
                vertices.Add(point);
        }

        private void SetNavPoint(float2 point, NativeList<float2> vertices, Collider2D ignore)
        {
            Collider2D[] collider = Physics2D.OverlapPointAll(point, filter.layerMask);
            if ((collider.Length == 0 || collider[0] == ignore) && volume.OverlapPoint(point))
                vertices.Add(point);
        }
    }

    public class VectorComparer : IComparer<Vector2>
    {
        Vector2 origin;

        public VectorComparer(Vector2 origin)
        {
            this.origin = origin;
        }

        public int Compare(Vector2 x, Vector2 y)
        {
            float d1 = Mathf.Abs(origin.x - x.x);
            float d2 = Mathf.Abs(origin.x - y.x);
            if (d1 == d2)
            {
                d1 = Mathf.Abs(origin.y - x.y);
                d2 = Mathf.Abs(origin.y - y.y);
            }
            return d1.CompareTo(d2);
        }
    }

    public class Float2Comparer : IComparer<float2>
    {
        Vector2 origin;

        public Float2Comparer(float2 origin)
        {
            this.origin = origin;
        }

        public int Compare(float2 x, float2 y)
        {
            float d1 = math.abs(origin.x - x.x);
            float d2 = math.abs(origin.x - y.x);
            if (d1 == d2)
            {
                d1 = math.abs(origin.y - x.y);
                d2 = math.abs(origin.y - y.y);
            }
            return d1.CompareTo(d2);
        }
    }
}


