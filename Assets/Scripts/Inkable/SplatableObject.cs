using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SplatableObject : MonoBehaviour
{
    [Tooltip("Destination target for ink blots on this object.")]
    public RenderTexture splatmask;
    public RenderTexture splatBuffer;
    public RenderTexture Splatmask { get { return splatmask; } }

    [SerializeField, Tooltip("If left blank a blank render texture of size " +
        "textureSize will be automatically generated.")]
    protected Texture sourceMask;
    [SerializeField]
    protected int textureSize = 1024;

    [Tooltip("Material to hold the splatmask shader and properties.")]
    private Material splatmaskMaterial;
    private Material blendMaterial;
    [Tooltip("Mesh material for this object. Expected to be first material slot; " +
        "expected to \"_Splatmap\" tex2D property.")]
    private Material thisMaterial;
    private CommandBuffer cmd;

    void Start()
    {
        // Get the splatmask shader
        splatmaskMaterial = new Material(Shader.Find("Unlit/Splatmask"));

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

        // Create blend material
        blendMaterial = new Material(Shader.Find("Unlit/Blend"));

        splatBuffer = new RenderTexture(splatmask.width, splatmask.height, 0, RenderTextureFormat.ARGBFloat);

        // Cache a command buffer for later
        cmd = new CommandBuffer();
    }

    public void DrawSplat(Vector3 worldPos, Vector3 normal, float radius, float hardness, float strength, Color inkColor)
    {
        splatmaskMaterial.SetFloat(Shader.PropertyToID("_Radius"), radius);
        splatmaskMaterial.SetFloat(Shader.PropertyToID("_Hardness"), hardness);
        splatmaskMaterial.SetFloat(Shader.PropertyToID("_Strength"), strength);
        splatmaskMaterial.SetVector(Shader.PropertyToID("_SplatPos"), worldPos);
        splatmaskMaterial.SetVector(Shader.PropertyToID("_Normal"), normal);
        splatmaskMaterial.SetVector(Shader.PropertyToID("_InkColor"), inkColor);

        RenderTexture temp = RenderTexture.GetTemporary(splatmask.width, splatmask.height, 0, RenderTextureFormat.ARGBFloat);
        cmd.SetRenderTarget(temp);
        cmd.DrawRenderer(GetComponent<Renderer>(), splatmaskMaterial);

        cmd.Blit(temp, splatmask, blendMaterial);
        
        Graphics.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        RenderTexture.ReleaseTemporary(temp);
    }
}
