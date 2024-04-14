using System;
using System.Collections.Generic;
using UnityEngine;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using System.Linq;
using System.IO;

namespace THOIF_Mod_Loader
{
    internal class Program
    {
        public static string managedDirectory;

        static void Main(string[] args)
        {
            Console.WriteLine("[*]                    Welcome to ValeriansBox's ModLoader                     ");
            Console.WriteLine("[*] make sure to place the THOIF_Mod_Loader.dll (get it on github) in managed! \n");

            Console.WriteLine("[*]     ...Checking if this was placed in the game's managed directory...        ");
            managedDirectory = GetManagedDirectory();

            if (string.IsNullOrEmpty(managedDirectory))
            {
                Console.WriteLine("[-] This should be placed inside of the game's managed directory ");
                Console.ReadKey();
                return;
            }

            try
            {
                InitCodeWeaving();
                Console.WriteLine("[+] Complete please press enter. ");
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("[-] Failed... ");
                
            }

            Console.ReadKey();
        }

        public static string GetManagedDirectory()
        {
            string currentPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (currentPath.Contains("Managed"))
            {
                return currentPath;
            }

            return null;
        }

        public static void InitCodeWeaving()
        {
            var targetPath = Path.Combine(managedDirectory, "Assembly-CSharp.dll");
            var copyPath = Path.Combine(managedDirectory, "Assembly-CSharp-Copy.dll");

            if (!File.Exists(copyPath)) 
            {
                File.Copy(targetPath, copyPath);
                Console.WriteLine("test");
            }

            var targetModule = ModuleDefinition.ReadModule(copyPath);
            var typeToInject = targetModule.Types.FirstOrDefault(t => t.Name == "GameManager");
            var targetMethod = typeToInject.Methods.First(m => m.Name == "Awake");

            var assemblyLoadFileRef = targetModule.ImportReference(typeof(System.Reflection.Assembly).GetMethod("LoadFile", new Type[] { typeof(string) }));
            var assemblyGetType = targetModule.ImportReference(typeof(System.Reflection.Assembly).GetMethod("GetType", new Type[] { typeof(string) }));

            var getEntryAssemblyRef = targetModule.ImportReference(typeof(System.Reflection.Assembly).GetMethod("GetExecutingAssembly", new Type[0]));
            var getLocationRef = targetModule.ImportReference(typeof(System.Reflection.Assembly).GetMethod("get_Location", new Type[0]));
            var getDirectoryNameRef = targetModule.ImportReference(typeof(System.IO.Path).GetMethod("GetDirectoryName", new Type[] { typeof(string) }));
            var combineRef = targetModule.ImportReference(typeof(System.IO.Path).GetMethod("Combine", new Type[] { typeof(string), typeof(string) }));

            var gameObjectctor = targetModule.ImportReference(typeof(UnityEngine.GameObject).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null));
            var addComponentRef = targetModule.ImportReference(typeof(UnityEngine.GameObject).GetMethod("AddComponent", new Type[] { typeof(Type) }));

            List<Instruction> loadModLoader = new List<Instruction>()
            {
                Instruction.Create(OpCodes.Newobj, gameObjectctor),
                Instruction.Create(OpCodes.Call, getEntryAssemblyRef),
                Instruction.Create(OpCodes.Callvirt, getLocationRef),
                Instruction.Create(OpCodes.Call, getDirectoryNameRef),
                Instruction.Create(OpCodes.Ldstr, "THOIF_Mod_Loader.dll"),
                Instruction.Create(OpCodes.Call, combineRef),
                Instruction.Create(OpCodes.Call, assemblyLoadFileRef),
                Instruction.Create(OpCodes.Ldstr, "THOIF_Mod_Loader.Loader"),
                Instruction.Create(OpCodes.Callvirt, assemblyGetType),
                Instruction.Create(OpCodes.Callvirt, addComponentRef),
                Instruction.Create(OpCodes.Pop),
            };

            InsertInstructions(loadModLoader, targetMethod);

            targetModule.Write(targetPath);
        }

        public static void InsertInstructions(List<Instruction> instructions, MethodDefinition targetMethod)
        {
            var targetProcessor = targetMethod.Body.GetILProcessor();
            targetProcessor.InsertBefore(targetMethod.Body.Instructions[0], instructions[0]);

            for (int i = 1; i < instructions.Count; i++)
            {
                targetProcessor.InsertBefore(targetMethod.Body.Instructions[i], instructions[i]);
            }
        }
    }
}
