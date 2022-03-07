using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Strazh.Analysis;

public class ProjectDependencyGraphWrapper
{
    private static Type _type = typeof(ProjectDependencyGraph);
    public ProjectDependencyGraph Value { get; private set;  }

    public ProjectDependencyGraphWrapper()
    {
        Value = (ProjectDependencyGraph) _type.GetField("Empty").GetValue(null);
    }

    public void WithAdditionalProject(ProjectId projectId)
    {
        Value = (ProjectDependencyGraph) _type.GetMethod("WithAdditionalProject").Invoke(Value, BindingFlags.Instance | BindingFlags.NonPublic, null,
            new[] {projectId}, null)!;
    }
}