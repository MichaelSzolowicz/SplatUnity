using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Singleton to manage all splatable objects in a scene. 
/// Useful for things like tallying the score.
/// </summary>
public class SplatableObjectsManager : Singleton<SplatableObjectsManager>   
{
    [Tooltip("List off splatable objects in scene.")]
    private List<SplatableObject> splatableObjectsList = new List<SplatableObject>();
    public List<SplatableObject> SplatableObjectsList {
        get { return splatableObjectsList; }
    }
    [Tooltip("Command buffer for open use to prevent need for multiple allocations.")]
    private CommandBuffer commandBuffer;
    public CommandBuffer CommandBuffer {
        get {
            if(commandBuffer == null) commandBuffer = new CommandBuffer();
            return commandBuffer;   
        }
    }

    /// <summary>
    /// Add splatable object to splatable objects list.
    /// </summary>
    /// <param name="splatableObject"></param>
    public void RegisterSplatableObject(SplatableObject splatableObject)
    {
        // Is it faster with a small collection to use a list and linear access?
        // Or is dictionary lookup faster regardless of scale? (ask someone).
        if (!splatableObjectsList.Contains(splatableObject))
        {
            splatableObjectsList.Add(splatableObject);  
        }
    }

    /* TESTONLY */

    private void OnGUI()
    {
        if(GUILayout.Button("print all splatable objects"))
        {
            foreach(var splatableObject in splatableObjectsList)
            {
                Debug.Log(splatableObject.gameObject.name);
            }
        }
    }
}
