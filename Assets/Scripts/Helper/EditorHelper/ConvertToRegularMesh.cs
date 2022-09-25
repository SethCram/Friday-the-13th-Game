using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// skinned mesh -> regular mesh
/// </summary>
public class ConvertToRegularMesh : MonoBehaviour
{

    //activate this method from the menu: ('context menu' found under 3 dots in top right of script comp)
    [ContextMenu("Convert to regular mesh")]
    private void Convert()
    {
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        //add new renderer + filter comps:
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

        //retrieve + set mat and mesh from skinned mesh renderer: 
        meshFilter.sharedMesh = skinnedMeshRenderer.sharedMesh;
        meshRenderer.sharedMaterials = skinnedMeshRenderer.sharedMaterials;

        //destroy skinned mesh renderer and this script:
        DestroyImmediate(skinnedMeshRenderer);
        DestroyImmediate(this); //why not just use 'Destroy'?
    }
   
}
