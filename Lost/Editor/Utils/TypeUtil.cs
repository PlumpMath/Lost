//-----------------------------------------------------------------------
// <copyright file="TypeUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public static class TypeUtil
    {
        /// <summary>
        /// 
        /// </summary>
        private static Dictionary<Type, HashSet<Type>> typesCache = new Dictionary<Type, HashSet<Type>>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> GetAllTypesOf<T>()
        {
            Type type = typeof(T);
            HashSet<Type> types;

            if (typesCache.TryGetValue(type, out types) == false)
            {
                lock (string.Intern(type.FullName))
                {
                    if (typesCache.TryGetValue(type, out types) == false)
                    {
                        types = new HashSet<Type>();
                        
                        foreach (System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                        {
                            foreach (Type assemblyType in assembly.GetTypes())
                            {
                                if (type.IsAssignableFrom(assemblyType) &&
                                    assemblyType.IsInterface == false &&
                                    assemblyType.IsAbstract == false &&
                                    types.Contains(assemblyType) == false)
                                {
                                    types.Add(assemblyType);
                                }
                            }
                        }
                        
                        typesCache.Add(type, types);
                    }
                }
            }

            foreach (Type t in types)
            {
                yield return t;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> CreateAllInstancesOf<T>() where T : class
        {
            foreach (Type type in GetAllTypesOf<T>())
            {
                yield return Activator.CreateInstance(type) as T;
            }
        }
    }
}