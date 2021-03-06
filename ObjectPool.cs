﻿/*  ╔═════════════════════════════════════════════╡  DinoTank - ObjectPool 2018 ╞═══════════════════════════════════════════════════════╗            
    ║ Authors:  Dmitrii Roets                       Email:    roetsd@icloud.com                                                         ║
    ╟───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╢░ 
    ║ Purpose: This system is designed to replace standard Instantiation procedure and Destroy. The objects created via this system are ║░
    ║          recycled. Saves memory and performance.                                                                                  ║░
    ║ Usage:   Use ObjectPool.InstantiateGameObject(somePrefab, position, rotation) and  ObjectPool.DestroyGameObject(someObject) to    ║░
    ║          make objects you want                                                                                                    ║░
    ╚═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝░
       ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectPool : MonoBehaviour
{
    public bool prewarmObjectPool = true;                               // Preload Objects into the pool at start ?
    public List<GameObject> prewarmedObjects = new List<GameObject>();  // List of objects to preload
    public List<GameObject> objects = new List<GameObject>();           // Primary Objects container

    // Debugging 
    public bool debugMode = true;                                       // Keep track of spawned object names, uses and show objects destroyed outside of object pool
    public List<string> objectsNames = new List<string>();              // List of names
    public List<int> used = new List<int>();                            // list of uses

    // Singleton
    private static ObjectPool objectPool;
    public static ObjectPool Instance
    {
        get
        {
            if (!objectPool)
            {
                objectPool = FindObjectOfType(typeof(ObjectPool)) as ObjectPool;
                if (!objectPool)
                {
                    Debug.Log("Could not find ObjectPool in scene, will make one");
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
            objectPool = this;
        else
            Destroy(gameObject);
        Init();
        transform.SetParent(null);
    }

    /// <summary>
    /// Preloads objects if requested 
    /// </summary>
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
                    Instance.objects.Add(newObject);

                    if (Instance.debugMode)
                    {
                        Instance.objectsNames.Add(newObject.name + "_PRE");
                        Instance.used.Add(0);
                    }
                    newObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Replaces the Instantiate(GameObject); Will traverse the already created objects and return one if available, otherwise Instantiates a new one.   
    /// No overload version
    /// </summary>
    /// <param name="_object">The Prefab to Instanatinate</param>
    /// <param name="_activate">Return already activated?</param>
    /// <returns>The Instance of the requested object.</returns>
    public static GameObject InstantiateGameObject(GameObject _object, bool _activate = true)
    {
        if (_object == null)
        {
            Debug.Log("<Color=Maroon><b>ObjectPool Error</b></Color>: <Color=Blue>Null object</Color> passed into instantiate");
            return null;
        }
        for (int i = 0; i < Instance.objects.Count; i++)
        {
            if (Instance.objects[i])
            {
                if ((!Instance.objects[i].activeInHierarchy) && (Instance.objects[i].name == _object.name))
                {
                    if (Instance.debugMode)
                        Instance.used[i]++;
                    if (_activate)
                        Instance.objects[i].SetActive(true);
                    return Instance.objects[i];
                }
            }
            else
            {
                if (Instance.debugMode)
                {
                    Debug.LogWarning("ObjectPool warning has null for " + Instance.objectsNames[i] + " object in list when called to create " + _object.name + ", make shure calling ObjectPool.DestroyGameObject() to return objects to the pool");
                }
            }
        }

        GameObject newObject = Instantiate(_object);
        newObject.name = _object.name;
        Instance.objects.Add(newObject);

        if (Instance.debugMode)
        {
            Instance.objectsNames.Add(newObject.name + "_DYN");
            Instance.used.Add(1);
        }
        if (_activate)
            newObject.SetActive(true);

        return newObject;
    }

    /// <summary>
    /// Replaces the Instantiate(GameObject); Will traverse the already created objects and return one if available, otherwise Instantiates a new one.   
    /// No overload version
    /// </summary>
    /// <param name="_object">The Prefab to Instanatinate</param>
    /// <param name="_position">World Position</param>
    /// <param name="_rotation">World Rotation</param>
    /// <param name="_activate">Return already activated?</param>
    /// <returns>The Instance of the requested object.</returns>
    public static GameObject InstantiateGameObject(GameObject _object, Vector3 _position, Quaternion _rotation, bool _activate = true)
    {
        if (_object == null)
        {
            Debug.Log("<Color=Maroon><b>ObjectPool Error</b></Color>: <Color=Blue>Null object</Color> passed into instantiate");
            return null;
        }

        for (int i = 0; i < Instance.objects.Count; i++)
        {
            if (Instance.objects[i])
            {
                if ((Instance.objects[i].name == _object.name) && (!Instance.objects[i].activeInHierarchy))
                {
                    Instance.objects[i].transform.position = _position;
                    Instance.objects[i].transform.rotation = _rotation;
                    if (Instance.debugMode)
                        Instance.used[i]++;
                    if (_activate)
                        Instance.objects[i].SetActive(true);
                    return Instance.objects[i];
                }
            }
        }

        GameObject newObject = Instantiate(_object);
        newObject.name = _object.name;
        newObject.transform.position = _position;
        newObject.transform.rotation = _rotation;
        if (_activate)
            newObject.SetActive(true);
        Instance.objects.Add(newObject);
        if (Instance.debugMode)
        {
            Instance.objectsNames.Add(newObject.name + "_DYN");
            Instance.used.Add(1);
        }
        return newObject;
    }

    /// <summary>
    /// Replaces the Destroy(GameObject) function, will first determine if requested object is in the pool and deactivate. Otherwise Destroy.
    /// </summary>
    /// <param name="_obj">The object to destroy</param>
    public static void DestroyGameObject(GameObject _obj)
    {
        for (int i = 0; i < Instance.objects.Count; i++)
        {
            if (Instance.objects[i] && Instance.objects[i] == _obj)
            {
                _obj.transform.SetParent(null);
                _obj.SetActive(false);
                return;
            }
        }
        Destroy(_obj);
    }
}