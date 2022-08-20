using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//reqs type of class name w/ inheriting this:
public class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance = null;

    public static T Instance
    {
        get
        {
            if(_instance == null)
            {
                //find all references of singleton:
                T[] results = Resources.FindObjectsOfTypeAll<T>();

                //if dont have any refs to singleton:
                if(results.Length == 0)
                {
                    //havent created singleton yet:
                    Debug.LogError("SingletonScritpableObject -> Instance -> results length is 0 for type " + typeof(T).ToString() + ".");
                    return null;
                }
                //if have too many refs to singleton:
                if(results.Length > 1)
                {
                    //created more than 1 singleton:
                    Debug.LogError("SingletonScritpableObject -> Instance -> results length is greater than 1 for type " + typeof(T).ToString() + ".");
                    return null;
                }

                _instance = results[0];

                //singlton doesnt get eaten by garbage collection:
                _instance.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }

            return _instance;
        }
    }
}
