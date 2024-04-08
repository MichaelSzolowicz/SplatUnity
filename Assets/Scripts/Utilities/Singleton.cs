using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindAnyObjectByType<T>();
                if(instance == null)
                {
                    GameObject gameObject = new GameObject();
                    gameObject.name = typeof(T).Name;
                    instance = gameObject.AddComponent<T>();    
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if(instance == null)
        {
            // We are the first instance.
            instance = this as T;
            DontDestroyOnLoad(instance);
        }
        else
        {
            // Instance already exists
            Destroy(gameObject);
        }
    }
}
