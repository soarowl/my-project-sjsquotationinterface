using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Concurrent;

namespace QuotV5
{
    /// <summary>  
    /// ObjectCreator class used to create object at runtime.  
    /// </summary>  
    public class ObjectCreator<T>
    {
        #region Singleton
        private ObjectCreator() { }
        public static ObjectCreator<T> Instance
        {
            get { return Nested._instance; }
        }
        private class Nested
        {
            static Nested() { }
            internal static readonly ObjectCreator<T> _instance = new ObjectCreator<T>();
        }
        #endregion

        private ObjectActivator _rowCreatedActivator = null;

        /// <summary>  
        /// Constructor of delegate  
        /// </summary>  
        /// <typeparam name="T">Type</typeparam>  
        /// <param name="args">arguments of Constructor</param>  
        /// <returns>Type</returns>  
        private delegate T ObjectActivator(params object[] args);

        public T Create(Type[] types, params object[] args)
        {
            if (null == _rowCreatedActivator)
            {
                var constructorInfo = typeof(T).GetConstructor(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    types,
                    null);
                _rowCreatedActivator = ObjectCreator<T>.Instance.GetActivator(constructorInfo);
            }
            return _rowCreatedActivator(args);
        }

        /// <summary>  
        /// Create object type at runtime.  
        /// </summary>  
        /// <typeparam name="T">Type</typeparam>  
        /// <param name="ctor">ConstructorInfo of Type</param>  
        /// <returns>Constructor of delegate</returns>  
        private ObjectActivator GetActivator(ConstructorInfo ctor)
        {
            ParameterInfo[] paramsInfo = ctor.GetParameters();

            //create a single param of type object[]  
            ParameterExpression param = Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp = new Expression[paramsInfo.Length];

            //pick each arg from the params array   
            //and create a typed expression of them  
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp = Expression.ArrayIndex(param, index);

                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the  
            //ctor with the args we just created  
            NewExpression newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New  
            //Expression as body and our param object[] as arg  
            LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator), newExp, param);

            //compile it  
            ObjectActivator compiled = (ObjectActivator)lambda.Compile();
            return compiled;
        }
    }

    public class ObjectCreator
    {
        /// <summary>  
        /// Constructor of delegate  
        /// </summary>  
        /// <param name="args">arguments of Constructor</param>  
        /// <returns>Type</returns>  
        private delegate object ObjectActivator(params object[] args);

        private static ConcurrentDictionary<Type, ObjectActivator> objActivators = new ConcurrentDictionary<Type, ObjectActivator>();

        public static object Create(Type rtnType, Type[] types, params object[] args)
        {
            ObjectActivator objActivator = objActivators.GetOrAdd(rtnType,
                (t) =>
                {
                    var constructorInfo = t.GetConstructor(
                       BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                       null,
                       types,
                       null);
                    return GetActivator(constructorInfo);
                });
            return objActivator.Invoke(args);
        }


        /// <summary>  
        /// Create object type at runtime.  
        /// </summary>  
        /// <typeparam name="T">Type</typeparam>  
        /// <param name="ctor">ConstructorInfo of Type</param>  
        /// <returns>Constructor of delegate</returns>  
        private static ObjectActivator GetActivator(ConstructorInfo ctor)
        {
            ParameterInfo[] paramsInfo = ctor.GetParameters();

            //create a single param of type object[]  
            ParameterExpression param = Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp = new Expression[paramsInfo.Length];

            //pick each arg from the params array   
            //and create a typed expression of them  
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp = Expression.ArrayIndex(param, index);

                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the  
            //ctor with the args we just created  
            NewExpression newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New  
            //Expression as body and our param object[] as arg  
            LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator), newExp, param);

            //compile it  
            ObjectActivator compiled = (ObjectActivator)lambda.Compile();
            return compiled;
        }
    }
}
