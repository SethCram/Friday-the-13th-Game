using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Manager/GameSettings")]
public class GameSettings : ScriptableObject
{
    public string gameVersion = "0.0.0";

    private string _nickname = "BiteMe";

    //return w/ random # on end to differentiate tween same names:
    public string Nickname
    {
        get
        {
            //return random int at end:
            int randomInt = Random.Range(0, 9999);
            return _nickname + randomInt.ToString();
        }
    }
}
