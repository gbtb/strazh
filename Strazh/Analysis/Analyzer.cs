using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis;
using Strazh.Domain;
using System.Collections.Generic;
using System;
using Strazh.Database;
using static Strazh.Analysis.AnalyzerConfig;
using System.IO;
using Microsoft.CodeAnalysis.MSBuild;

namespace Strazh.Analysis
{
    public static class Analyzer
    {
        public static async Task Analyze(AnalyzerConfig config)
        {
            Console.WriteLine($"Setup analyzer...");

            var projects = new List<Project>();
            using var workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += HandleWorkspaceFailure();
            Solution? solution = null;
            if (config.IsSolutionBased)
            {
                solution = await workspace.OpenSolutionAsync(config.Solution);
                projects.AddRange(solution.Projects);
                solution.GetProjectDependencyGraph();
            }
            else
            {
                foreach (var projectPath in config.Projects)
                {
                    var project = await workspace.OpenProjectAsync(projectPath);
                    projects.Add(project);
                }

                solution = workspace.CurrentSolution;
            }


            var g = solution.GetProjectDependencyGraph();
            Console.WriteLine($"Analyzer ready to analyze {projects.Count} project/s.");


            //var sortedProjectAnalyzers = g.GetTopologicallySortedProjects(); //todo: can be parallelized with dependency sets ?

            for (var index = 0; index < projects.Count; index++)
            {
                var triples = await AnalyzeProject(index + 1, solution, projects[index], config.Tier);
                triples = triples.GroupBy(x => x.ToString()).Select(x => x.First()).OrderBy(x => x.NodeA.Label).ToList();
                await DbManager.InsertData(triples, config.Credentials, config.IsDelete && index == 0);
            }
        }

        private static async Task<IList<Triple>> AnalyzeProject(int index, Solution? solution, Project project, Tiers mode)
        {
            Console.WriteLine($"Project #{index}:");
            var root = GetRoot(project.FilePath);
            var rootNode = new FolderNode(root, root);
            var projectName = GetProjectName(project.Name);
            Console.WriteLine($"Analyzing {projectName} project...");

            var triples = new List<Triple>();
            if (mode is Tiers.All or Tiers.Project && solution != null)
            {
                Console.WriteLine($"Analyzing Project tier...");
                //var projectBuild = item.projectAnalyzer.Build().FirstOrDefault();
                var projectNode = new ProjectNode(projectName);
                triples.Add(new TripleIncludedIn(projectNode, rootNode));
                foreach (var x in project.ProjectReferences)
                {
                    
                    var referencedProject = solution.GetProject(x.ProjectId);
                    var node = new ProjectNode(referencedProject?.Name ?? $"[UnknownProject:{x.ProjectId}]");
                    triples.Add(new TripleDependsOnProject(projectNode, node));
                }

                foreach (var x in project.MetadataReferences.ToList())
                {
                    if (x is not CompilationReference cref)
                        continue;
                    
                    var version = cref.Compilation.Assembly.Identity.Version.ToString();
                    var node = new PackageNode(cref.Display, cref.Compilation.AssemblyName, version);
                    triples.Add(new TripleDependsOnPackage(projectNode, node));
                }

                Console.WriteLine($"Analyzing Project tier complete.");
            }

            if (project.SupportsCompilation && mode is Tiers.All or Tiers.Code)
            {
                Console.WriteLine($"Analyzing Code tier...");
                var compilation = await project.GetCompilationAsync();
                var syntaxTreeRoot = compilation.SyntaxTrees.Where(x => !x.FilePath.Contains("obj"));
                foreach (var st in syntaxTreeRoot)
                {
                    var sem = compilation.GetSemanticModel(st);
                    Extractor.AnalyzeTree<InterfaceDeclarationSyntax>(triples, st, sem, rootNode);
                    Extractor.AnalyzeTree<ClassDeclarationSyntax>(triples, st, sem, rootNode);
                }
                Console.WriteLine($"Analyzing Code tier complete.");
            }

            Console.WriteLine($"Analyzing {projectName} project complete.");
            return triples;
        }

        private static string GetProjectName(string fullName)
            => fullName.Split(Path.DirectorySeparatorChar).Last().Replace(".csproj", "");

        private static string GetRoot(string? filePath)
            => filePath?.Split(Path.DirectorySeparatorChar).Reverse().Skip(1).FirstOrDefault();
        
        private static EventHandler<WorkspaceDiagnosticEventArgs> HandleWorkspaceFailure()
        {
            return (object sender, WorkspaceDiagnosticEventArgs e) => Console.WriteLine($"Error/Warning while opening solution: {e.Diagnostic.Message}");
        }
    }
}