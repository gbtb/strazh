namespace Strazh.Domain
{
    public interface ITriple
    {
        public Node NodeA { get; }
        
        public Node NodeB { get; }
        
        public Relationship Relationship { get; }
    }
    
    public abstract record GenericTriple<TNodeA, TNodeB, TRel>(TNodeA NodeA, TNodeB NodeB, TRel Relationship) : ITriple
        where TNodeA: Node
        where TNodeB: Node
        where TRel: Relationship
    {
        Node ITriple.NodeA => NodeA;
        Node ITriple.NodeB => NodeB;
        Relationship ITriple.Relationship => Relationship;
    }

    // Structure

    public record TripleDependsOnProject(ProjectNode NodeA, ProjectNode NodeB) : 
        GenericTriple<ProjectNode, ProjectNode, DependsOnRelationship>(NodeA, NodeB, DependsOnRelationship.Instance);

    public record TripleDependsOnPackage(ProjectNode NodeA,
        PackageNode NodeB) : GenericTriple<ProjectNode, PackageNode, DependsOnRelationship>(NodeA, NodeB, DependsOnRelationship.Instance);

    public record TripleIncludedIn : GenericTriple<Node, FolderNode, IncludedInRelationship>
    {
        public TripleIncludedIn(
            ProjectNode contentA,
            FolderNode contentB)
            : base(contentA, contentB, IncludedInRelationship.Instance)
        { }

        public TripleIncludedIn(
            FolderNode contentA,
            FolderNode contentB)
            : base(contentA, contentB, IncludedInRelationship.Instance)
        { }

        public TripleIncludedIn(
            FileNode contentA,
            FolderNode contentB)
            : base(contentA, contentB, IncludedInRelationship.Instance)
        { }
    }

    public record TripleDeclaredAt(TypeNode NodeA,
        FileNode NodeB) : GenericTriple<TypeNode, FileNode, DeclaredAtRelationship>(NodeA, NodeB, DeclaredAtRelationship.Instance);

    // Code

    public record TripleInvoke(MethodNode NodeA,
        MethodNode NodeB) : GenericTriple<MethodNode, MethodNode, InvokeRelationship>(NodeA, NodeB, InvokeRelationship.Instance);

    public record TripleHave(TypeNode NodeA,
        MethodNode NodeB) : GenericTriple<TypeNode, MethodNode, HaveRelationship>(NodeA, NodeB, HaveRelationship.Instance);

    public record TripleConstruct(MethodNode NodeA, ClassNode NodeB)
        : GenericTriple<MethodNode, ClassNode, ConstructRelationship>(NodeA, NodeB, ConstructRelationship.Instance)
    {
        //public TripleConstruct(
        //    ClassNode classA,
        //    ClassNode classB)
        //    : base(classA, classB, new ConstructRelationship())
        //{ }
    }

    public record TripleOfType : GenericTriple<TypeNode, TypeNode, OfTypeRelationship>
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