using UnityEngine;

/// <summary>
/// Step 3.8 from the guide. Kept available for the generated prefab workflow.
/// The current-scene installer does not use it automatically, because the team scene already has dirt objects.
/// </summary>
public class GardenGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 4;
    public int columns = 4;
    public float spacing = 1.1f;
    public GameObject tilePrefab;

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        if (tilePrefab == null) return;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Vector3 pos = transform.position + new Vector3(c * spacing, 0f, r * spacing);
                Instantiate(tilePrefab, pos, Quaternion.identity, transform);
            }
        }
    }
}
