using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Strazh.Domain;

namespace Strazh.Analysis;

public class MetadataReferenceVisitor
{
    private readonly ProjectNode _projectNode;
    private readonly List<Triple> _triples;

    public MetadataReferenceVisitor(ProjectNode projectNode, List<Triple> triples)
    {
        _projectNode = projectNode;
        _triples = triples;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="reference"></param>
    /// <exception cref="ArgumentException"></exception>
    public void Visit(MetadataReference reference)
    {
        switch (reference)
        {
            case CompilationReference cref:
                Visit(cref);
                break;
            case PortableExecutableReference pref:
                Visit(pref);
                break;
            default:
                throw new ArgumentException($"Unknown metadata reference type: {reference.GetType()}");
        }
    }
    
    public void Visit(CompilationReference reference)
    {
        var version = reference.Compilation.Assembly.Identity.Version.ToString();
        var node = new PackageNode(reference.Display, reference.Compilation.AssemblyName, version);
        _triples.Add(new TripleDependsOnPackage(_projectNode, node));
    }
    
    public void Visit(PortableExecutableReference reference)
    {
        var version = "";
        var node = new PackageNode(reference.Display, Path.GetFileName(reference.Display), version);
        _triples.Add(new TripleDependsOnPackage(_projectNode, node));
    }
}