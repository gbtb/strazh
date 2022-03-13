namespace Strazh.Domain
{
    public abstract record Triple(Node NodeA, Node NodeB, Relationship Relationship)
    {
        //public override string ToString()
        //    => $"MERGE (a:{NodeA.Label} {{ pk: \"{NodeA.Pk}\" }}) ON CREATE SET {NodeA.Set("a")} ON MATCH SET {NodeA.Set("a")} MERGE (b:{NodeB.Label} {{ pk: \"{NodeB.Pk}\" }}) ON CREATE SET {NodeB.Set("b")} ON MATCH SET {NodeB.Set("b")} MERGE (a)-[:{Relationship.Type}]->(b);";
    }

    // Structure

    public record TripleDependsOnProject : Triple
    {
        public TripleDependsOnProject(
            ProjectNode projectA,
            ProjectNode projectB)
            : base(projectA, projectB, new DependsOnRelationship())
        { }
    }

    public record TripleDependsOnPackage : Triple
    {
        public TripleDependsOnPackage(
            ProjectNode projectA,
            PackageNode packageB)
            : base(projectA, packageB, new DependsOnRelationship())
        { }
    }

    public record TripleIncludedIn : Triple
    {
        public TripleIncludedIn(
            ProjectNode contentA,
            FolderNode contentB)
            : base(contentA, contentB, new IncludedInRelationship())
        { }

        public TripleIncludedIn(
            FolderNode contentA,
            FolderNode contentB)
            : base(contentA, contentB, new IncludedInRelationship())
        { }

        public TripleIncludedIn(
            FileNode contentA,
            FolderNode contentB)
            : base(contentA, contentB, new IncludedInRelationship())
        { }
    }

    public record TripleDeclaredAt : Triple
    {
        public TripleDeclaredAt(
            TypeNode typeA,
            FileNode fileB)
            : base(typeA, fileB, new DeclaredAtRelationship())
        { }
    }

    // Code

    public record TripleInvoke : Triple
    {
        public TripleInvoke(
            MethodNode methodA,
            MethodNode methodB)
            : base(methodA, methodB, new InvokeRelationship())
        { }
    }

    public record TripleHave : Triple
    {
        public TripleHave(
            TypeNode typeA,
            MethodNode methodB)
            : base(typeA, methodB, new HaveRelationship())
        { }
    }

    public record TripleConstruct : Triple
    {
        //public TripleConstruct(
        //    ClassNode classA,
        //    ClassNode classB)
        //    : base(classA, classB, new ConstructRelationship())
        //{ }

        public TripleConstruct(
            MethodNode methodA,
            ClassNode classB)
            : base(methodA, classB, new ConstructRelationship())
        { }
    }

    public record TripleOfType : Triple
    {
        public TripleOfType(
            ClassNode classA,
            TypeNode typeB)
            : base(classA, typeB, new OfTypeRelationship())
        { }

        public TripleOfType(
            InterfaceNode interfaceA,
            InterfaceNode interfaceB)
            : base(interfaceA, interfaceB, new OfTypeRelationship())
        { }
    }
}