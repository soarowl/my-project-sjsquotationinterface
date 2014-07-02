using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.InteropServices;

namespace QuotV5
{
    public class ObjectLogHelper<T> 
    {
        static DynamicMethodExecutor structToStringExecutor;

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

            foreach (var field in type.GetFields())
            {
                if (!field.IsStatic && field.IsPublic && !field.FieldType.IsEnum)
                {
                    if (field.FieldType.GetFields().Any(f => f.IsPublic && !f.IsStatic))
                        GenerateCode_ObjectToStringClass(sb, field.FieldType);
                }
            }
            foreach (var property in type.GetProperties())
            {
                if (property.CanRead)
                {
                    if (!property.PropertyType.IsEnum)
                    {
                        if (property.PropertyType.GetFields().Any(f => f.IsPublic && !f.IsStatic))
                            GenerateCode_ObjectToStringClass(sb, property.PropertyType);
                        else if (property.PropertyType.GetProperties().Any(p => p.CanRead))
                            GenerateCode_ObjectToStringClass(sb, property.PropertyType);
                    }
                }
            }
            sb.AppendLine("public class " + GenerateObjectToStringClassName(type));
            sb.AppendLine("{");//class start

            GenerateCode_ObjectToString(sb, type);
   
            sb.AppendLine("}");//class end
        }
        private static string GenerateObjectToStringClassName(Type type)
        {
            return "ObjectToString_" + type.FullName.Replace(".", "_").Replace("+", "__");
        }
        private static void GenerateCode_ObjectToString(StringBuilder sb, Type type)
        {
            sb.AppendLine(string.Format("public static string ObjectToString({0} obj)", type.FullName.Replace("+",".")));
            sb.AppendLine("{");//method start

            sb.AppendLine("StringBuilder sb=new StringBuilder();");
            sb.AppendLine(string.Format ("sb.AppendLine(\"[{0}]\");",type.FullName));
            foreach (var field in type.GetFields())
            {
                if (field.IsStatic || !field.IsPublic)
                    // don't process static fields
                    continue;
                var fieldType = field.FieldType;
                if (fieldType == typeof(string))
                {
                    sb.AppendLine(string.Format("sb.AppendLine(string.Format(\"{0}={{0}}\",obj.{0}));", field.Name));
                }
                else if (
                    fieldType == typeof(Int16)
                    || fieldType == typeof(Int32)
                    || fieldType == typeof(Int64)
                    || fieldType == typeof(UInt16)
                    || fieldType == typeof(UInt32)
                    || fieldType == typeof(UInt64)
                    || fieldType == typeof(float)
                    || fieldType == typeof(Double)
                    || fieldType == typeof(Decimal)
                    || fieldType == typeof(Boolean)
                    || fieldType == typeof(Char)
                    )
                {
                    sb.AppendLine(string.Format("sb.AppendLine(string.Format(\"{0}={{0}}\",obj.{0}));", field.Name));
                }
                else if (fieldType.IsEnum)
                {
                    sb.AppendLine(string.Format("sb.AppendLine(string.Format(\"{0}={{0}}\",obj.{0}));", field.Name));
                }
                else
                {
                    sb.AppendLine(string.Format("sb.AppendLine(string.Format(\"{0}={{0}}\",{1}.ObjectToString(obj.{0})));", field.Name, GenerateObjectToStringClassName(fieldType)));

                }
            }

            foreach (var property in type.GetProperties())
            {
                if (!property.CanRead)
                    // don't process static fields
                    continue;
                var propertyType = property.PropertyType;

                if (propertyType == typeof(string))
                {
                    sb.AppendLine(string.Format("sb.AppendLine(string.Format(\"{0}={{0}}\",obj.{0}));", property.Name));
                }
                else if (
                    propertyType == typeof(Int16)
                    || propertyType == typeof(Int32)
                    || propertyType == typeof(Int64)
                    || propertyType == typeof(UInt16)
                    || propertyType == typeof(UInt32)
                    || propertyType == typeof(UInt64)
                    || propertyType == typeof(float)
                    || propertyType == typeof(Double)
                    || propertyType == typeof(Decimal)
                    || propertyType == typeof(Boolean)
                    || propertyType == typeof(Char)
                    )
                {
                    sb.AppendLine(string.Format("sb.AppendLine(string.Format(\"{0}={{0}}\",obj.{0}));", property.Name));
                }
                else if (propertyType.IsEnum)
                {
                    sb.AppendLine(string.Format("sb.AppendLine(string.Format(\"{0}={{0}}\",obj.{0}));", property.Name));
                }
                else
                {
                    sb.AppendLine(string.Format("sb.AppendLine(string.Format(\"{0}={{0}}\",{1}.ObjectToString(obj.{0})));", property.Name, GenerateObjectToStringClassName(propertyType)));

                }
            }



            sb.AppendLine("return sb.ToString();");

            sb.AppendLine("}");//method end
        }

      
        #endregion

        public static string ObjectToString(T obj)
        {
            return structToStringExecutor.Execute(null, new object[] { obj }) as string;
        }
        
       
    }
}
