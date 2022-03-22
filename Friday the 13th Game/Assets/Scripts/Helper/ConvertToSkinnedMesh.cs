using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConvertToSkinnedMesh : MonoBehaviour
{
    //activate this method from the menu: ('context menu' found under 3 dots in top right of script comp)
    [ContextMenu("Convert to skinned mesh")]
    private void Convert()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        //add skinned mesh comp:
        SkinnedMeshRenderer skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();

        //destroy filter and this script (creating skinned mesh destroyed the mesh renderer):
        DestroyImmediate(meshFilter, true);
        DestroyImmediate(this, true);

        //doesn't work as intended bc we don't have bone weights
    }
}
