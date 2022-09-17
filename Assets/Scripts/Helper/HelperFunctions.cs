using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

static class HelperFunctions 
{
    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                //Ignore deprecated props
                bool obsolete = false;
                IEnumerable attrData = pinfo.CustomAttributes;
                foreach (CustomAttributeData data in attrData)
                {
                    if (data.AttributeType == typeof(System.ObsoleteAttribute))
                    {
                        obsolete = true;
                        break;
                    }
                }
                if (obsolete)
                {
                    continue;
                }

                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }

    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }
}
