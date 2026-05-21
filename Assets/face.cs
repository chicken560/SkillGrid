using UnityEngine;

public class VoimaBillboard : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }

        // THE FIX FOR THE TREE DISAPPEARING:
        // We manually override the tree's bounding box. This forces Unity to 
        // keep rendering the tree even if the camera is staring right at its edge.
        if (TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
        {
            Mesh mesh = meshFilter.mesh;
            // Extends the tracking bubble to 15 units wide/tall
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 15f);
        }
    }

    void LateUpdate()
    {
        if (camTransform == null) return;

        Vector3 lookDir = camTransform.forward;
        lookDir.y = 0; // Keeps it upright like a tree cutout

        if (lookDir != Vector3.zero)
        {
            // If the tree looks backwards or disappears, change '-lookDir' to 'lookDir'
            transform.rotation = Quaternion.LookRotation(-lookDir, Vector3.up);
        }
    }
}
