﻿using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace NSoft.NFramework.Reflections {
    /// <summary>
    /// Lambda Expression으로 만들어진 object를 Dump 합니다.
    /// </summary>
    public class ObjectDumper {
        private readonly int depth;
        private readonly TextWriter writer;
        private int level;
        private int pos;

        private ObjectDumper(int depth) {
            writer = Console.Out;
            this.depth = depth;
        }

        /// <summary>
        /// 지정된 인스턴스의 정보를 Dump 뜹니다.
        /// </summary>
        /// <param name="o"></param>
        public static void Write(object o) {
            Write(o, 0);
        }

        /// <summary>
        /// 지정된 인스턴스의 정보를 Dump 뜹니다.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="depth"></param>
        public static void Write(object o, int depth) {
            var dumper = new ObjectDumper(depth);
            dumper.WriteObject(null, o);
        }

        private void Write(string s) {
            if(s != null) {
                writer.Write(s);
                pos += s.Length;
            }
        }

        private void WriteIndent() {
            for(int i = 0; i < level; i++)
                writer.Write("  ");
        }

        private void WriteLine() {
            writer.WriteLine();
            pos = 0;
        }

        private void WriteTab() {
            Write("  ");
            while(pos % 8 != 0)
                Write(" ");
        }

        private void WriteObject(string prefix, object o) {
            if(o == null || o is ValueType || o is string) {
                WriteIndent();
                Write(prefix);
                WriteValue(o);
                WriteLine();
            }
            else if(o is IEnumerable) {
                foreach(object element in (IEnumerable)o) {
                    if(element is IEnumerable && !(element is string)) {
                        WriteIndent();
                        Write(prefix);
                        Write("...");
                        WriteLine();
                        if(level < depth) {
                            level++;
                            WriteObject(prefix, element);
                            level--;
                        }
                    }
                    else {
                        WriteObject(prefix, element);
                    }
                }
            }
            else {
                MemberInfo[] members = o.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                WriteIndent();
                Write(prefix);
                bool propWritten = false;
                foreach(MemberInfo m in members) {
                    var f = m as FieldInfo;
                    var p = m as PropertyInfo;
                    if(f != null || p != null) {
                        if(propWritten) {
                            WriteTab();
                        }
                        else {
                            propWritten = true;
                        }
                        Write(m.Name);
                        Write("=");
                        Type t = f != null ? f.FieldType : p.PropertyType;
                        if(t.IsValueType || t == typeof(string)) {
                            WriteValue(f != null ? f.GetValue(o) : p.GetValue(o, null));
                        }
                        else {
                            if(typeof(IEnumerable).IsAssignableFrom(t)) {
                                Write("...");
                            }
                            else {
                                Write("{ }");
                            }
                        }
                    }
                }
                if(propWritten)
                    WriteLine();
                if(level < depth) {
                    foreach(MemberInfo m in members) {
                        var f = m as FieldInfo;
                        var p = m as PropertyInfo;
                        if(f != null || p != null) {
                            Type t = f != null ? f.FieldType : p.PropertyType;
                            if(!(t.IsValueType || t == typeof(string))) {
                                object value = f != null ? f.GetValue(o) : p.GetValue(o, null);
                                if(value != null) {
                                    level++;
                                    WriteObject(m.Name + ": ", value);
                                    level--;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WriteValue(object o) {
            if(o == null) {
                Write("null");
            }
            else if(o is DateTime) {
                Write(((DateTime)o).ToShortDateString());
            }
            else if(o is ValueType || o is string) {
                Write(o.ToString());
            }
            else if(o is IEnumerable) {
                Write("...");
            }
            else {
                Write("{ }");
            }
        }
    }
}