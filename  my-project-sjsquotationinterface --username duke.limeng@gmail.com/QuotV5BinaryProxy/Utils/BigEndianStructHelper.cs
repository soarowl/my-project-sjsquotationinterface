using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.InteropServices;

namespace QuotV5
{
    public class BigEndianStructHelper<T> where T : struct
    {
        static int tSize = Marshal.SizeOf(typeof(T));
        static DynamicMethodExecutor structToBytesExecutor;
        static DynamicMethodExecutor bytesToStructExecutor;

        static BigEndianStructHelper()
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
            }
            else
            {
                // 编译后的程序集 
                Assembly ass = cr.CompiledAssembly;
                Type type = ass.GetType(string.Format("{0}.{1}", typeof(BigEndianStructHelper<T>).Namespace, GenerateStructHelperClassName(typeof(T))));
                MethodInfo structToBytes = type.GetMethod("StructToBytes");
                MethodInfo bytesToStruct = type.GetMethod("BytesToStruct");
                structToBytesExecutor = new DynamicMethodExecutor(structToBytes);
                bytesToStructExecutor = new DynamicMethodExecutor(bytesToStruct);
            }
        }

        #region 生成代码
        static string GenerateCode()
        {

            Type type = typeof(T);
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Runtime.InteropServices;");
            sb.AppendLine("namespace " + typeof(BigEndianStructHelper<T>).Namespace);
            sb.AppendLine("{");//namespace start
            GenerateCode_StructHelperClass(sb, type);
            sb.AppendLine("}");//namespace end
            return sb.ToString();


        }

        private static void GenerateCode_StructHelperClass(StringBuilder sb, Type type)
        {

            foreach (var field in type.GetFields())
            {
                if (!field.IsStatic && field.IsPublic)
                {
                    if (field.FieldType.GetFields().Any(f => f.IsPublic && !f.IsStatic))
                        GenerateCode_StructHelperClass(sb, field.FieldType);
                }
            }

            sb.AppendLine("public class " + GenerateStructHelperClassName(type));
            sb.AppendLine("{");//class start

            GenerateCode_StructToBytes(sb, type);
            GenerateCode_BytesToStruct(sb, type);
            sb.AppendLine("}");//class end
        }
        private static string GenerateStructHelperClassName(Type type)
        {
            return "StructHelper_" + type.FullName.Replace(".", "_").Replace("+", "__");
        }
        private static void GenerateCode_StructToBytes(StringBuilder sb, Type type)
        {
            sb.AppendLine(string.Format("public static byte[] StructToBytes({0} obj,System.Text.Encoding encoding)", type.FullName.Replace("+",".")));
            sb.AppendLine("{");//method start


            var size = Marshal.SizeOf(type);

            sb.AppendLine(string.Format("byte[] rtn=new byte[{0}];", size));

            foreach (var field in type.GetFields())
            {
                if (field.IsStatic || !field.IsPublic)
                    // don't process static fields
                    continue;
                var fieldType = field.FieldType;

                int offset = Marshal.OffsetOf(type, field.Name).ToInt32();
                if (fieldType == typeof(string))
                {
                    sb.AppendLine(string.Format("var fieldBytes_{0}=encoding.GetBytes(obj.{0});", field.Name));
                    sb.AppendLine(string.Format("fieldBytes_{0}.CopyTo(rtn,{1});", field.Name, offset));
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
                    )
                {

                    sb.AppendLine(string.Format("var fieldBytes_{0}= BitConverter.GetBytes (obj.{0});", field.Name));
                    sb.AppendLine(string.Format("Array.Reverse(fieldBytes_{0});", field.Name));
                    sb.AppendLine(string.Format("fieldBytes_{0}.CopyTo(rtn,{1});", field.Name, offset));
                }
                else if (fieldType == typeof(Boolean))
                {
                    sb.AppendLine(string.Format("var fieldBytes_{0}= BitConverter.GetBytes (obj.{0});", field.Name));
                    sb.AppendLine(string.Format("fieldBytes_{0}.CopyTo(rtn,{1});", field.Name, offset));
                }
                else if (fieldType == typeof(Char))
                {
                    sb.AppendLine(string.Format("var fieldBytes_{0}=encoding.GetBytes(new char[]{{obj.{0}}});", field.Name));
                    sb.AppendLine(string.Format("fieldBytes_{0}.CopyTo(rtn,{1});", field.Name, offset));
                }
                else
                {
                    sb.AppendLine(string.Format("var fieldBytes_{0}={1}.StructToBytes(obj.{0},encoding);", field.Name, GenerateStructHelperClassName(fieldType)));
                    sb.AppendLine(string.Format("fieldBytes_{0}.CopyTo(rtn,{1});", field.Name, offset));
                }
            }
            sb.AppendLine("return rtn;");

            sb.AppendLine("}");//method end
        }

        private static void GenerateCode_BytesToStruct(StringBuilder sb, Type type)
        {

            sb.AppendLine(string.Format("public static {0} BytesToStruct(byte[] bytes,System.Text.Encoding encoding)", type.FullName.Replace("+", ".")));
            sb.AppendLine("{");//method start


            var size = Marshal.SizeOf(type);

            sb.AppendLine(string.Format("{0} rtn=new {0}();", type.FullName.Replace("+", ".")));

            foreach (var field in type.GetFields())
            {
                if (field.IsStatic)
                    // don't process static fields
                    continue;
                var fieldType = field.FieldType;

                int offset = Marshal.OffsetOf(type, field.Name).ToInt32();
                if (fieldType == typeof(string))
                {
                    MarshalAsAttribute attr = field.GetCustomAttributes(typeof(MarshalAsAttribute), false).First() as MarshalAsAttribute;
                    sb.AppendLine(string.Format("rtn.{0}=encoding.GetString(bytes,{1},{2}).Trim('\\0');", field.Name, offset, attr.SizeConst));
                }
                else if (
                    fieldType == typeof(Int16)
                    || fieldType == typeof(Int32)
                    || fieldType == typeof(Int64)
                    || fieldType == typeof(UInt16)
                    || fieldType == typeof(UInt32)
                    || fieldType == typeof(UInt64)
                    || fieldType == typeof(Single)
                    || fieldType == typeof(Double)
                    || fieldType == typeof(Decimal))
                {
                    var fieldLength = Marshal.SizeOf(fieldType);
                    sb.AppendLine(string.Format("Array.Reverse(bytes, {0}, {1});", offset, fieldLength));
                    sb.AppendLine(string.Format("rtn.{0}= BitConverter.To{2}(bytes,{1});", field.Name, offset, fieldType.Name));

                }
                else if (fieldType == typeof(Boolean))
                {
                    sb.AppendLine(string.Format("rtn.{0}= BitConverter.To{2}(bytes,{1});", field.Name, offset, fieldType.Name));
                }
                else if (fieldType == typeof(Char))
                {
                    var fieldLength = Marshal.SizeOf(fieldType);
                    sb.AppendLine(string.Format("rtn.{0}=encoding.GetString(bytes,{1},{2}).First();", field.Name, offset, fieldLength));

                }
                else
                {
                    var fieldLength = Marshal.SizeOf(fieldType);
                    sb.AppendLine(string.Format("var fieldBytes_{0}=bytes.Skip({1}).Take({2}).ToArray();", field.Name, offset, fieldLength));
                    sb.AppendLine(string.Format("rtn.{0}={1}.BytesToStruct(fieldBytes_{0},encoding);", field.Name, GenerateStructHelperClassName(fieldType)));
                }
            }
            sb.AppendLine("return rtn;");

            sb.AppendLine("}");//method end
        }
        #endregion

        public static byte[] StructToBytes(T obj, Encoding encoding)
        {
            return structToBytesExecutor.Execute(null, new object[] { obj, encoding }) as byte[];
        }
        
        public static T BytesToStruct(byte[] bytes, Encoding encoding)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            if (bytes.Length != tSize)
                throw new ArgumentException(string.Format ("数据长度与预期不一致，可能是数据结构不匹配，预期长度{0}，实际长度{1}",tSize,bytes.Length));
            return (T)bytesToStructExecutor.Execute(null, new object[] { bytes, encoding });
        }
    }

}
