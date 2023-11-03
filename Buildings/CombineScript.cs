#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombineScript : MonoBehaviour
{
    public Transform Buildings;

    [ContextMenu("COMBINE_MESHES")]
    public void CombineMeshesInSubBlocks()
    {
        Transform[] children = Buildings.GetComponentsInChildren<Transform>(true);
        List<MeshFilter> oldMeshFilters = new List<MeshFilter>();

        foreach (Transform child in children)
        {
            if (child.gameObject.name == "SubBlock")
            {
                oldMeshFilters.AddRange(CombineMeshes(child));
            }
        }

        foreach(MeshFilter m in oldMeshFilters)
        {
            if(m.gameObject.name != "SubBlock")
            {
                DestroyImmediate(m.gameObject);
            }
        }
    }

    private List<MeshFilter> CombineMeshes(Transform parent)
    {
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>(true);
        if(meshFilters is null || meshFilters.Length < 1)
        {
            return null;
        }

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        Material material = parent.GetComponentInChildren<MeshRenderer>(true).sharedMaterial;

        Vector3 parentPosition = parent.position;
        parent.position = Vector3.zero;

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        if (parent.GetComponent<MeshFilter>() == null)
        {
            parent.gameObject.AddComponent<MeshFilter>();
        }
        parent.GetComponent<MeshFilter>().mesh = new Mesh();
        parent.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
        parent.GetComponent<MeshFilter>().sharedMesh.name = "CombinedMesh";

        if (parent.GetComponent<MeshRenderer>() == null)
        {
            parent.gameObject.AddComponent<MeshRenderer>();
        }
        parent.GetComponent<MeshRenderer>().sharedMaterial = material;

        if (parent.GetComponent<MeshCollider>() == null)
        {
            parent.gameObject.AddComponent<MeshCollider>();
        }

        parent.position = parentPosition;

        return new List<MeshFilter>(meshFilters);
    }
}
#endif
