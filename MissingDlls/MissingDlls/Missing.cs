using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MissingDlls
{
    public class Missing
    {
        private readonly string path;
        private readonly bool onlyErrors;
        private readonly bool hideGac;
        private readonly Dictionary<string, Assembly> assembliesInFolder;
        private readonly List<TextLine> lines = new List<TextLine>(); 
        public Missing(string path, bool onlyErrors, bool hideGac)
        {
            this.path = path;
            this.onlyErrors = onlyErrors;
            this.hideGac = hideGac;
            IsSuccess = true;

            assembliesInFolder = GetFiles("*.dll")
                                .Concat(GetFiles("*.exe"))
                                .Select(GetAssemblyFromFile)
                                .Where(x => x != null)
                                .ToDictionary(x => x.GetName().Name, x => x);
            
            var assembly = GetAssemblyFromFile(path);
            if (assembly == null)
            {
                IsSuccess = false;
                return;
            }
            AnalyseTree(assembly);
        }

        public bool IsSuccess { get; private set; }

        private IEnumerable<string> GetFiles(string searchPattern)
        {
            return Directory.GetFiles(Path.GetDirectoryName(path), searchPattern);
        }

        public void PrintTree()
        {
            foreach (var textLine in lines)
            {
                if (onlyErrors && !textLine.IsError)
                    continue;
                if (!textLine.IsError && hideGac && textLine.IsGac)
                    continue;
                textLine.Print();
            }
        }

        private TextLine CurrentLine
        {
            get
            {
                if (lines.Count == 0)
                    lines.Add(new TextLine(0));
                return lines[lines.Count - 1];
            }
        }

        private void NewLine(int tabs)
        {
            lines.Add(new TextLine(tabs));
        }

        private void AnalyseTree(Assembly assembly, int tabLevel = 0)
        {
            var name = assembly.GetName().Name;
            NewLine(tabLevel);
            if (assembly.GlobalAssemblyCache)
            {
                CurrentLine.IsGac = true;
                CurrentLine.AddText("[GAC] ", ConsoleColor.DarkYellow);
            }
            CurrentLine.AddText(name, ConsoleColor.White);
            CurrentLine.AddText(string.Format(" [{0}]", assembly.GetName().Version), ConsoleColor.Yellow);
            if (assembly.GlobalAssemblyCache)
                return;

            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                Assembly referencedAssembly;
                if (assembliesInFolder.TryGetValue(assemblyName.Name, out referencedAssembly))
                    CheckVersion(tabLevel, assemblyName, referencedAssembly);
                else if (!TryLoadAssemblyFromGac(assemblyName, ref referencedAssembly)) 
                        continue;
                
                AnalyseTree(referencedAssembly, tabLevel + 1);
            }
        }

        private void CheckVersion(int tabLevel, AssemblyName assemblyName, Assembly referencedAssembly)
        {
            if (assemblyName.Version != referencedAssembly.GetName().Version)
            {
                NewLine(tabLevel + 1);
                CurrentLine.IsError = true;
                CurrentLine.AddText(
                    string.Format("Version errror {0} [{1}] found [{2}]", assemblyName.Name, assemblyName.Version,
                        referencedAssembly.GetName().Version), ConsoleColor.Red);
                IsSuccess = false;
            }
        }

        private bool TryLoadAssemblyFromGac(AssemblyName referencedAssembly, ref Assembly assembly2)
        {
            try
            {
                assembly2 = Assembly.Load(referencedAssembly);
            }
            catch (Exception ex)
            {
                CurrentLine.IsError = true;
                CurrentLine.AddText(string.Format("Failed to load assembly '{0}': {1}", referencedAssembly.Name, ex.Message),
                    ConsoleColor.Red);
                IsSuccess = false;
                return false;
            }
            return true;
        }

        private Assembly GetAssemblyFromFile(string path)
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.ReflectionOnlyLoadFrom(path);
            }
            catch (Exception ex)
            {
                CurrentLine.IsError = true;
                CurrentLine.AddText(string.Format("Failed to load assembly '{0}': {1}", path, ex.Message), ConsoleColor.Red);
                assembly = null;
            }
            return assembly;
        }
    }
}