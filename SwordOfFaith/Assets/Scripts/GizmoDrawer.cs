using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoDrawer : MonoBehaviour
{
    #region Variables & Inspector Options
    [Header("Gizmo Control")]
    [Space(10)]
    public List<GizmoDrawObject> drawObjects;
    #endregion

    /// <summary>
    /// Calls the OnValidate methods of the Gizmos
    /// </summary>
    private void OnValidate()
    {
        if (drawObjects != null)
        {
			foreach(GizmoDrawObject drawObject in drawObjects)
			{
				drawObject.OnValidate();
			}
        }
    }
}

[System.Serializable]
public class GizmoDrawObject
{
    #region Variables & Inspector Options
    public bool drawGizmo = true; //Check if this gizmo should draw
    public GizmoDrawMode gizmoDrawMode = GizmoDrawMode.SolidMesh;
    public GizmoMeshMode gizmoMeshMode = GizmoMeshMode.MeshFilter;
    public Color drawColor;  
    [ConditionalHide("FilterMode", true)]
    public MeshFilter filterMesh; //Mesh to draw override
    [ConditionalHide("SkinMode", true)]
    public SkinnedMeshRenderer skinMesh; //Mesh to draw override
    public Transform drawTransform; //Reference transform that should probably be a child object of this object
    public Vector3 drawRotationOverride; //Rotate Override to make sure mesh fits where you expect

    #region Stored Data
    public enum GizmoDrawMode { SolidMesh, WireMesh } //Visual mode this gizmo should draw with
    public enum GizmoMeshMode { MeshFilter, SkinMeshRenderer } //Type of renderer this gizmo should draw
    [HideInInspector]
    public Color defaultColor;
    [HideInInspector]
    public bool FilterMode = true, SkinMode;
    #endregion
    #endregion

    /// <summary>
    /// Update inspector tools based on mode
    /// </summary>
    public void OnValidate()
    {
        if (gizmoMeshMode == GizmoMeshMode.MeshFilter)
        {
            if (!FilterMode)
            {
                FilterMode = true;
            }

            if (SkinMode)
            {
                SkinMode = false;
            }
        }
        else if (gizmoMeshMode == GizmoMeshMode.SkinMeshRenderer)
        {
            if (FilterMode)
            {
                FilterMode = false;
            }

            if (!SkinMode)
            {
                SkinMode = true;
            }
        }
    }

    /// <summary>
    /// Draw gizmo based on variables defined above
    /// </summary>
    public void OnDrawGizmos()
    {
        if (drawGizmo)
        {
            if (drawColor != Color.clear)
            {
                Gizmos.color = drawColor;
            }
            else
            {
                Gizmos.color = defaultColor;
            }

            if (drawTransform != null)
            {
                if (filterMesh != null || skinMesh != null)
                {
                    Quaternion drawRotation = Quaternion.Euler(drawTransform.rotation.x + drawRotationOverride.x, drawTransform.rotation.y + drawRotationOverride.y, drawTransform.rotation.z + drawRotationOverride.z);

                    if (gizmoDrawMode == GizmoDrawMode.SolidMesh)
                    {
                        if (gizmoMeshMode == GizmoMeshMode.MeshFilter)
                        {
                            Gizmos.DrawMesh(filterMesh.sharedMesh, drawTransform.position, drawRotation, drawTransform.localScale);
                        }
                        else if (gizmoMeshMode == GizmoMeshMode.SkinMeshRenderer)
                        {
                            Gizmos.DrawMesh(skinMesh.sharedMesh, drawTransform.position, drawRotation, drawTransform.localScale);
                        }
                    }
                    else if (gizmoDrawMode == GizmoDrawMode.WireMesh)
                    {
                        if (gizmoMeshMode == GizmoMeshMode.MeshFilter)
                        {
                            Gizmos.DrawWireMesh(filterMesh.sharedMesh, drawTransform.position, drawTransform.rotation, drawTransform.localScale);
                        }
                        else if (gizmoMeshMode == GizmoMeshMode.SkinMeshRenderer)
                        {
                            Gizmos.DrawWireMesh(skinMesh.sharedMesh, drawTransform.position, drawTransform.rotation, drawTransform.localScale);
                        }
                    }
                }
                else
                {
                    if (gizmoDrawMode == GizmoDrawMode.SolidMesh)
                    {
                        Gizmos.DrawCube(drawTransform.position, drawTransform.localScale);
                    }
                    else if (gizmoDrawMode == GizmoDrawMode.WireMesh)
                    {
                        Gizmos.DrawWireCube(drawTransform.position, drawTransform.localScale);
                    }
                }

                Gizmos.DrawLine(drawTransform.position, drawTransform.position + drawTransform.forward * 2);
            }
        }
    }
}
