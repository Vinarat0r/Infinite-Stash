using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using EFT.InventoryLogic;
using UnityEngine;

namespace InfiniteStashClient
{
    [BepInPlugin("com.vinarator.infinitestash", "InfiniteStash", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmony = new Harmony("com.vinarator.infinitestash");
            Type compoundItemType = typeof(CompoundItem);
            PropertyInfo gridsProperty = compoundItemType.GetProperty("Grids");
            Type gridType = null;

            if (gridsProperty != null)
            {
                Type propertyType = gridsProperty.PropertyType;
                if (propertyType.IsArray)
                {
                    gridType = propertyType.GetElementType();
                }
                else if (propertyType.IsGenericType)
                {
                    gridType = propertyType.GetGenericArguments()[0];
                }
            }

            if (gridType != null)
            {
                ConstructorInfo constructor = gridType.GetConstructors()
                    .FirstOrDefault(c => {
                        var parameters = c.GetParameters();
                        return parameters.Length >= 3 &&
                               parameters[0].ParameterType == typeof(string) &&
                               parameters[1].ParameterType == typeof(int) &&
                               parameters[2].ParameterType == typeof(int);
                    });

                if (constructor != null)
                {
                    var prefixMethod = typeof(GridConstructorPatch).GetMethod("Prefix");
                    harmony.Patch(constructor, prefix: new HarmonyMethod(prefixMethod));
                }
            }
        }
    }

    public static class GridConstructorPatch
    {
        public static void Prefix(string __0, ref int __2)
        {
            if (__0 != null && __0.Equals("hideout", StringComparison.OrdinalIgnoreCase))
            {
                __2 = 10000;
            }
        }
    }
}