﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq.Expressions;
using QuotV5.Utils;

namespace QuotV5
{
    public delegate void Action<TObject, TValue>(TObject obj, TValue value);
    class FastReflection<T> 
    {
        private static ConcurrentDictionary<PropertyInfo, Delegate> setPropertyDelegateDic = new ConcurrentDictionary<PropertyInfo, Delegate>();
        /// <summary>
        /// 为对象的指定属性赋值
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetPropertyValue<TProperty>(T obj, PropertyInfo propertyInfo, TProperty value)
        { 
            Action<T, TProperty> act = setPropertyDelegateDic.GetOrAdd(propertyInfo, p => { return GetPropertySetter<T, TProperty>(p); }) as Action<T, TProperty>;
            if (act != null)
            {
                act(obj, value);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static Action<TObject, TProperty> GetPropertySetter<TObject, TProperty>(PropertyInfo propertyInfo)
        {
            ParameterExpression objParamExpression = Expression.Parameter(typeof(TObject));

            ParameterExpression propertyParamExpression = Expression.Parameter(typeof(TProperty), propertyInfo.Name);

            MemberExpression propertyExpression = Expression.Property(objParamExpression, propertyInfo.Name);
           
            Type propertyDeclaringType = propertyInfo.PropertyType;

            Expression castPropertyParaExpression = GetCastOrConvertExpression(propertyParamExpression, propertyDeclaringType);

            Action<TObject, TProperty> result = Expression.Lambda<Action<TObject, TProperty>>
            (
                Expression.Assign(propertyExpression, castPropertyParaExpression), objParamExpression, propertyParamExpression
            ).Compile();

            return result;
        }

        /// <summary>
        /// 产生一个类型转换的表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        private static Expression GetCastOrConvertExpression(Expression expression, Type targetType)
        {
            Expression result;
            Type expressionType = expression.Type;

            // Check if a cast or conversion is required.
            if (targetType.IsAssignableFrom(expressionType))
            {
                result = expression;
            }
            else
            {
                // Check if we can use the as operator for casting or if we must use the convert method
                if (targetType.IsValueType && !IsNullableType(targetType))
                {
                    result = Expression.Convert(expression, targetType);
                }
                else
                {
                    result = Expression.TypeAs(expression, targetType);
                }
            }

            return result;
        }

        /// <summary>
        /// 判断指定类型是否为一个可空类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsNullableType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            bool result = false;
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                result = true;
            }

            return result;
        }

    }
}
