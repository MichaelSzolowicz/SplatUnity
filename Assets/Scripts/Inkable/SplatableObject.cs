using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Defines objects that can receive ink splats from various types of inker.
/// </summary>
public class SplatableObject : MonoBehaviour
{

    [Tooltip("Destination target for ink blots on this object.")]
    private RenderTexture splatmask;
    public RenderTexture Splatmask { get { return splatmask; } }
    [SerializeField, Tooltip("If left blank a blank render texture of size " +
        "textureSize will be automatically generated.")]
    protected Texture sourceMask;
    [SerializeField]
    protected int textureSize = 1024;

    [Tooltip("Buffer to hold new splats before they are blended with the actual splatmap.")]
    private RenderTexture splatBuffer;
    [Tooltip("Material to hold the splatmask shader and properties.")]
    private Material splatmaskMaterial;
    [Tooltip("Material that should define belending operation for new splats.")]
    private Material blendMaterial;
    [Tooltip("Mesh material for this object. Expected to be first material slot; " +
        "expected to have \"_Splatmap\" tex2D property.")]
    private Material thisMaterial;
    private CommandBuffer cmd;


    void Awake()
    {
        // Get the splatmask shader
        splatmaskMaterial = new Material(Shader.Find("Unlit/Splatmask"));
        // Create blend material
        blendMaterial = new Material(Shader.Find("Unlit/Blend"));

        if (sourceMask)
        {
            // Copy the source mask into the splatmask
            splatmask = new RenderTexture(sourceMask.width, sourceMask.height, 0, RenderTextureFormat.ARGBFloat);
            Graphics.Blit(sourceMask, splatmask);
        }
        else
        {
            // Create a new splatmask
            splatmask = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGBFloat);
        }

        // Attach splatmask to this object's material.
        thisMaterial = GetComponent<Renderer>().material;
        thisMaterial.SetTexture("_splatmask", splatmask);

        // Init buffer.
        splatBuffer = new RenderTexture(splatmask.width, splatmask.height, 0, RenderTextureFormat.ARGBFloat);

        // Register with the splatable object manager. The manager is a singleton so one will always exist after this call.
        SplatableObjectsManager.Instance.RegisterSplatableObject(this);
        // Cache a reference to the manager's command buffer so we don't have to retrieve it constantly.
        cmd = SplatableObjectsManager.Instance.CommandBuffer;

        // We need to push some pixels to the splatmap, otherwise the renderer seems to get confused and flash other 
        // objects' splatmaps onto this one.
        cmd.Blit(splatBuffer, splatmask, blendMaterial);
        Graphics.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    /// <summary>
    /// Draw a circular brush stroke into this object's splatmask.
    /// </summary>
    /// <param name="worldPos"></param>
    /// <param name="normal"></param>
    /// <param name="radius"></param>
    /// <param name="hardness"></param>
    /// <param name="strength"></param>
    /// <param name="inkColor"></param>
    public void DrawSplat(Vector3 worldPos, Vector3 normal, float radius, float hardness, float strength, Color inkColor)
    {
        splatmaskMaterial.SetFloat(Shader.PropertyToID("_Radius"), radius);
        splatmaskMaterial.SetFloat(Shader.PropertyToID("_Hardness"), hardness);
        splatmaskMaterial.SetFloat(Shader.PropertyToID("_Strength"), strength);
        splatmaskMaterial.SetVector(Shader.PropertyToID("_SplatPos"), worldPos);
        splatmaskMaterial.SetVector(Shader.PropertyToID("_Normal"), normal);
        splatmaskMaterial.SetVector(Shader.PropertyToID("_InkColor"), inkColor);

        cmd.SetRenderTarget(splatBuffer);
        cmd.DrawRenderer(GetComponent<Renderer>(), splatmaskMaterial);

        cmd.Blit(splatBuffer, splatmask, blendMaterial);
        
        Graphics.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
}
