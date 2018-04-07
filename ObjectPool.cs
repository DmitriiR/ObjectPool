/*  ╔═════════════════════════════╡  ObjectPool 2018 ╞══════════════════════════════╗            
    ║ Authors:  Dmitrii Roets                       Email:    roetsd@icloud.com      ║
    ╟────────────────────────────────────────────────────────────────────────────────╢░ 
    ║ Purpose: This system is designed to replace standard Instantiation             ║░
    ║          procedure and Destroy. The objects created via this system are        ║░
    ║          recycled. Saves memory and performance.                               ║░
    ║ Usage:   Use ObjectPool.InstantiateGameObject(somePrefab, position, rotation)  ║░
    ║          and  ObjectPool.DestroyGameObject(someObject) to make objects you want║░                                                               
    ╚════════════════════════════════════════════════════════════════════════════════╝░
       ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectPool : MonoBehaviour {
  
    public bool prewarmObjectPool = true;
    public List<GameObject> prewarmedObjects = new List<GameObject>();
    public List<GameObject> objects = new List<GameObject>(); 

    // Debugging 
    public bool debugMode = true;
    public List<string> objectsNames = new List<string>(); 
    public List<int> used = new List<int>(); 

    // Singleton
    private static ObjectPool objectPool;
    public static ObjectPool instance
    {
        get
        {
            if (!objectPool)
            {
                objectPool = FindObjectOfType(typeof(ObjectPool)) as ObjectPool;
                if (!objectPool)
                {
                    Debug.Log("Could not find EventManager in scene, will make one");
                    GameObject manager = Instantiate(Resources.Load("Managers/ObjectPool", typeof(GameObject)) as GameObject);
                    if (manager)
                        objectPool = manager.GetComponent<ObjectPool>();
                    else
                    {
                        Debug.Log("<Color=Maroon><b>ObjectPool</b></Color>: <Color=Yellow>No Prefab</Color> detected, will make one for this run (good idea to make a prefab in resources)");
                        GameObject newObjectPool = new GameObject("TempObjectPool");
                        newObjectPool.AddComponent<ObjectPool>();
                        objectPool = newObjectPool.GetComponent<ObjectPool>();
                    }
                }
            }
            return objectPool;
        }
    }

   
    void Awake()
    {
        if (objectPool == null)
        {
            objectPool = this;
        }
        else
        {
            Destroy(gameObject);
        }
        Init();
        transform.SetParent(null);
    }
    

    public void Init()
    {
        if (!prewarmObjectPool)
            objects.Clear();
        else
        {
            foreach (GameObject item in prewarmedObjects)
            {
                if (item)
                {
                    GameObject newObject = Instantiate(item);
                    newObject.name = item.name;
                    instance.objects.Add(newObject);

                    if (instance.debugMode)
                    {
                        instance.objectsNames.Add(newObject.name + "_PRE");
                        instance.used.Add(0);
                    }
                    newObject.SetActive(false);
                }
            }
        }
    }

    public static GameObject InstantiateGameObject(GameObject _object, bool _activate = true)
    {
        if (_object == null)
        {
            Debug.Log("<Color=Maroon><b>ObjectPool Error</b></Color>: <Color=Blue>Null object</Color> passed into instantiate");
            return null;
        }
        for (int i = 0; i < instance.objects.Count; i++)
        {
            if (instance.objects[i])
            {
                if (!instance.objects[i].activeInHierarchy && instance.objects[i].name.Contains(_object.name))
                {
                    if (instance.debugMode)
                    {
                        instance.used[i]++;
                    }
                    return instance.objects[i];
                }
            }
            else
            {
                if (instance.debugMode)
                {
                    Debug.LogWarning("DinoWarning : ObjectPool has null for " + instance.objectsNames[i] + " object in list when called to create " + _object.name);
                }
            }
        }
         
        GameObject newObject = Instantiate(_object);
        newObject.name = _object.name;
        instance.objects.Add(newObject);

        if (instance.debugMode)
        {
            instance.objectsNames.Add(newObject.name + "_DYN");
            instance.used.Add(1);
        }

        newObject.SetActive(_activate);

        return newObject;
    }
    public static GameObject InstantiateGameObject(GameObject _object, Vector3 _position, Quaternion _rotation)
    {
      if(_object == null)
        {
            Debug.Log("<Color=Maroon><b>ObjectPool Error</b></Color>: <Color=Blue>Null object</Color> passed into instantiate");
            return null;
        }

        for (int i = 0; i < instance.objects.Count; i++)
        {
            if (instance.objects[i])
            {
                if (instance.objects[i].name.Contains(_object.name) && !instance.objects[i].activeInHierarchy)
                {
                    instance.objects[i].transform.position = _position;
                    instance.objects[i].transform.rotation = _rotation;
                    instance.objects[i].SetActive(true);

                    if (instance.debugMode)
                    {
                        instance.used[i]++;
                    }

                    return instance.objects[i];
                }
            }
        }

        GameObject newObject = Instantiate(_object);
        newObject.name = _object.name;
        newObject.transform.position = _position;
        newObject.transform.rotation = _rotation;
        newObject.SetActive(true);
        instance.objects.Add(newObject);

        if (instance.debugMode)
        {
            instance.objectsNames.Add(newObject.name + "_DYN");
            instance.used.Add(1);
        }
        return newObject;
    }

    public static bool Contains(GameObject _object)
    {
        return instance.objects.Contains(_object);
    }
    public static void DestroyGameObject(GameObject _obj)
    {
        for (int i = 0; i < instance.objects.Count; i++)
        {
            if (!instance.objects[i])
            {
            }
            else
            {
                if (instance.objects[i] == _obj)
                {
                    _obj.transform.SetParent(null);
                    _obj.SetActive(false);
                    return;
                }
            }
        }
        Destroy(_obj); 
    }
}
