using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace QuotV5
{
    public class ObjectLogHelper<T>
    {
        static DynamicMethodExecutor structToStringExecutor;
        static List<Type> types = new List<Type>();
        static ObjectLogHelper()
        {


            string code = GenerateCode();
            CodeDomProvider cdp = CodeDomProvider.CreateProvider("C#");

            // 编译器的参数 
            CompilerParameters cp = new CompilerParameters();
            cp.ReferencedAssemblies.Add(typeof(T).Assembly.Location);
            cp.ReferencedAssemblies.Add("System.Core.dll");
            foreach (var ass in typeof(T).Assembly.GetReferencedAssemblies())
            {
                if (ass.Name != "System.Core")
                {
                    var refAss = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == ass.Name);
                    if (refAss != null)
                        cp.ReferencedAssemblies.Add(refAss.Location);
                }
            }

            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;
            CompilerResults cr = cdp.CompileAssemblyFromSource(cp, code);


            if (cr.Errors.HasErrors)
            {
                Exception ex = new Exception();
                ex.Data["code"] = code;
                throw ex;
            }
            else
            {
                // 编译后的程序集 
                Assembly ass = cr.CompiledAssembly;
                Type type = ass.GetType(string.Format("{0}.{1}", typeof(ObjectLogHelper<T>).Namespace, GenerateObjectToStringClassName(typeof(T))));
                MethodInfo objectToString = type.GetMethod("ObjectToString");
                structToStringExecutor = new DynamicMethodExecutor(objectToString);

            }
        }

        #region 生成代码
        static string GenerateCode()
        {

            Type type = typeof(T);
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Runtime.InteropServices;");
            sb.AppendLine("namespace " + typeof(ObjectLogHelper<T>).Namespace);
            sb.AppendLine("{");//namespace start
            GenerateCode_ObjectToStringClass(sb, type);
            sb.AppendLine("}");//namespace end
            return sb.ToString();


        }


        private static void GenerateCode_ObjectToStringClass(StringBuilder sb, Type type)
        {
            Trace.WriteLine(string.Format("GenerateCode_ObjectToStringClass:{0}", type.FullName));
            if (IsCommonType(type))
                return;

            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                if (!IsCommonType(elementType))
                {
                    GenerateCode_ObjectToStringClass(sb, elementType);
                }

            }
            else if (type.IsGenericType)
            {
                if (type.Name == "List`1")
                {
                    Type elementType = type.GetGenericArguments().First();
                    if (!IsCommonType(elementType))
                    {
                        GenerateCode_ObjectToStringClass(sb, elementType);
                    }
                }
                else
                {
                    throw new Exception(string.Format("暂不支持对该类型的处理:{0}", type.FullName));
                }
            }
            else
            {

                foreach (var field in type.GetFields())
                {
                    if (!field.IsStatic && field.IsPublic && !field.FieldType.IsEnum && field.FieldType != typeof(string))
                    {
                        GenerateCode_ObjectToStringClass(sb, field);
                    }
                }
                foreach (var property in type.GetProperties())
                {
                    if (types.Contains(property.PropertyType))
                        continue;
                    if (property.CanRead && !property.PropertyType.IsEnum && property.PropertyType != typeof(string))
                    {
                        GenerateCode_ObjectToStringClass(sb, property);
                    }
                }
            }
            sb.AppendLine("public class " + GenerateObjectToStringClassName(type));
            sb.AppendLine("{");//class start

            GenerateCode_ObjectToStringMethord(sb, type);

            sb.AppendLine("}");//class end
        }


        private static void GenerateCode_ObjectToStringClass(StringBuilder sb, MemberInfo memberInfo)
        {
            Type memberType;
            if (memberInfo is FieldInfo)
                memberType = (memberInfo as FieldInfo).FieldType;
            else if (memberInfo is PropertyInfo)
                memberType = (memberInfo as PropertyInfo).PropertyType;
            else
                return;

            if (types.Contains(memberType))
                return;

            if (memberType.GetFields().Any(f => f.IsPublic && !f.IsStatic))
                GenerateCode_ObjectToStringClass(sb, memberType);
            else if (memberType.GetProperties().Any(p => p.CanRead))
                GenerateCode_ObjectToStringClass(sb, memberType);

            types.Add(memberType);
        }

        private static void GenerateCode_ObjectToStringMethord(StringBuilder sb, Type type)
        {
            sb.AppendLine(string.Format("public static string ObjectToString(global::{0} obj)", type.FullName.Replace("+", ".")));
            sb.AppendLine("{");//method start

            sb.AppendLine("StringBuilder sb=new StringBuilder();");
            sb.AppendLine(string.Format("sb.AppendLine(\"[{0}]\");", GetTypeNameStr(type)));

            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                GenerateCode_ArrayMemberToString(sb, elementType);
            }
            else if (type.IsGenericType)
            {
                if (type.Name == "List`1")
                {
                    Type elementType = type.GetGenericArguments().First();
                    GenerateCode_ArrayMemberToString(sb, elementType);
                }
                else
                {
                    throw new Exception(string.Format("暂不支持对该类型的处理:{0}", type.FullName));
                }
            }
            else
            {
                foreach (var field in type.GetFields())
                {
                    if (field.IsStatic || !field.IsPublic)
                        // don't process static fields
                        continue;
                    GenerateCode_MemberInfoToString(sb, field);
                }
                foreach (var property in type.GetProperties())
                {
                    if (!property.CanRead)
                        // don't process static fields
                        continue;
                    GenerateCode_MemberInfoToString(sb, property);
                }
            }


            sb.AppendLine("return sb.ToString();");

            sb.AppendLine("}");//method end
        }

        private static void GenerateCode_ArrayMemberToString(StringBuilder sb, Type elementType)
        {


            sb.AppendLine("if(obj!=null)");
            sb.AppendLine("{");//if start
            sb.AppendLine("foreach(var item in obj)");
            sb.AppendLine("{");//foreach start


            if (elementType == typeof(string))
            {
                sb.AppendLine(string.Format("if(item!=null)"));
                sb.AppendLine(string.Format("sb.AppendLine(item);"));
                sb.AppendLine(string.Format("else"));
                sb.AppendLine(string.Format("sb.AppendLine();"));
            }
            else if (IsCommonType(elementType))
            {
                sb.AppendLine(string.Format("sb.AppendLine(item.ToString());"));
            }
            else if (elementType.IsEnum)
            {
                sb.AppendLine(string.Format("sb.AppendLine(item.ToString());"));
            }
            else
            {
                sb.AppendLine(string.Format("sb.AppendLine({0}.ObjectToString(item));", GenerateObjectToStringClassName(elementType)));
            }

            sb.AppendLine("}");//foreach end
            sb.AppendLine("}");//if end
        }

        private static void GenerateCode_MemberInfoToString(StringBuilder sb, MemberInfo memberInfo)
        {

            string memberName;
            Type memberType;
            if (memberInfo is FieldInfo)
            {
                FieldInfo fi = memberInfo as FieldInfo;
                memberName = fi.Name;
                memberType = fi.FieldType;
            }
            else if (memberInfo is PropertyInfo)
            {
                PropertyInfo pi = memberInfo as PropertyInfo;
                memberName = pi.Name;
                memberType = pi.PropertyType;
            }
            else
                return;
            if (memberType == typeof(string))
            {
                sb.AppendLine(string.Format("sb.AppendLine(string.Format(\"{0}={{0}}\",obj.{0}));", memberName));
            }
            else if (IsCommonType(memberType))
            {
                sb.AppendLine(string.Format("sb.AppendLine(string.Format(\"{0}={{0}}\",obj.{0}));", memberName));
            }
            else if (memberType.IsEnum)
            {
                sb.AppendLine(string.Format("sb.AppendLine(string.Format(\"{0}={{0}}\",obj.{0}));", memberName));
            }
            else
            {
                sb.AppendLine(string.Format("sb.AppendLine(string.Format(\"{0}={{0}}\",{1}.ObjectToString(obj.{0})));", memberName, GenerateObjectToStringClassName(memberType)));
            }
        }


        private static string GenerateObjectToStringClassName(Type type)
        {
            return "ObjectToString_" + GetTypeNameStr(type);
        }

        private static string GetTypeNameStr(Type type)
        {
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                return string.Format("{0}_Array", GetTypeNameStr(elementType));
            }
            else if (type.IsGenericType)
            {
                if (type.Name == "List`1")
                {
                    Type elementType = type.GetGenericArguments().First();
                    return string.Format("{0}_List", GetTypeNameStr(elementType));
                }
                else
                {
                    throw new Exception(string.Format("暂不支持对该类型的处理:{0}", type.FullName));
                }
            }
            else
            {
                return type.FullName.Replace(".", "_").Replace("+", "__"); ;
            }
        }


        private static bool IsCommonType(Type type)
        {
            if (
                   type == typeof(string)
                  || type == typeof(Int16)
                   || type == typeof(Int32)
                   || type == typeof(Int64)
                   || type == typeof(UInt16)
                   || type == typeof(UInt32)
                   || type == typeof(UInt64)
                   || type == typeof(float)
                   || type == typeof(Double)
                   || type == typeof(Decimal)
                   || type == typeof(Boolean)
                   || type == typeof(Char)
                   || type == typeof(DateTime)
                   || type.IsEnum
                   )
            {
                return true;
            }
            else
                return false;
        }
        #endregion

        public static string ObjectToString(T obj)
        {
            return structToStringExecutor.Execute(null, new object[] { obj }) as string;
        }

    }
}
