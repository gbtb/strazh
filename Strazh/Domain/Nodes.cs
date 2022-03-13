using System.Collections.Generic;
using System.Linq;

namespace Strazh.Domain
{
    public abstract record Node(string FullName, string Name)
    {
        private string? _pk;
        public abstract string Label { get; }

        /// <summary>
        /// Primary Key used to compare Matching of nodes on MERGE operation
        /// </summary>
        public string Pk
        {
            get
            {
                if (_pk == null)
                    _pk = GetPrimaryKey();

                return _pk;
            } 
        }

        protected virtual string GetPrimaryKey()
        {
            return FullName.GetHashCode().ToString();
        }

        public virtual (string, IDictionary<string, object>) GetSetQueryAndParameters(string node)
        {
            var upNode = node.ToUpper();
            var dict = new Dictionary<string, object>(3)
            {
                {$"node{upNode}Pk", Pk},
                {$"node{upNode}FullName", FullName},
                {$"node{upNode}Name", Name}
            };
            var query = $"{node}.pk = $node{upNode}Pk, {node}.fullName = $node{upNode}FullName, {node}.name = $node{upNode}Name";

            return (query, dict);
        }
    }

    // Code

    public abstract record CodeNode : Node
    {
        public CodeNode(string fullName, string name, string[] modifiers = null)
            : base(fullName, name)
        {

            Modifiers = modifiers == null ? "" : string.Join(", ", modifiers);
        }

        public string Modifiers { get; }

        public override (string, IDictionary<string, object>) GetSetQueryAndParameters(string node)
        {
            var upNode = node.ToUpper();
            var (query, parameters) = base.GetSetQueryAndParameters(node);
            if (string.IsNullOrEmpty(Modifiers))
                return (query, parameters);

            query = $"{query}, {node}.modifiers = $node{upNode}Modifiers";
            parameters.Add($"node{upNode}Modifiers", Modifiers);
            return (query, parameters);
        }
    }

    public abstract record TypeNode : CodeNode
    {
        public TypeNode(string fullName, string name, string[] modifiers = null)
            : base(fullName, name, modifiers)
        {
        }
    }

    public record ClassNode : TypeNode
    {
        public ClassNode(string fullName, string name, string[] modifiers = null)
            : base(fullName, name, modifiers)
        {
        }

        public override string Label { get; } = "Class";
    }

    public record InterfaceNode : TypeNode
    {
        public InterfaceNode(string fullName, string name, string[] modifiers = null)
            : base(fullName, name, modifiers)
        {
        }

        public override string Label { get; } = "Interface";
    }

    public record MethodNode : CodeNode
    {
        public MethodNode(string fullName, string name, (string name, string type)[] args, string returnType, string[] modifiers = null)
            : base(fullName, name, modifiers)
        {
            Arguments = string.Join(", ", args.Select(x => $"{x.type} {x.name}"));
            ReturnType = returnType;
        }

        public override string Label { get; } = "Method";

        public string Arguments { get; }

        public string ReturnType { get; }

        public override (string, IDictionary<string, object>) GetSetQueryAndParameters(string node)
        {
            var upNode = node.ToUpper();
            var (query, parameters) = base.GetSetQueryAndParameters(node);

            query = $"{query}, {node}.returnType = $node{upNode}ReturnType, {node}.arguments = $node{upNode}Arguments";
            parameters.Add($"node{upNode}ReturnType", ReturnType);
            parameters.Add($"node{upNode}Arguments", Arguments);
            return (query, parameters);
        }
        
        protected override string GetPrimaryKey()
        {
            return $"{FullName}{Arguments}{ReturnType}".GetHashCode().ToString();
        }
    }

    // Structure

    public record FileNode(string FullName, string Name) : Node(FullName, Name)
    {
        public override string Label { get; } = "File";
    }

    public record FolderNode(string FullName, string Name) : Node(FullName, Name)
    {
        public override string Label { get; } = "Folder";
    }

    public record ProjectNode(string FullName, string Name) : Node(FullName, Name)
    {
        public ProjectNode(string name)
            : this(name, name) { }

        public override string Label { get; } = "Project";
    }

    public record PackageNode(string FullName, string Name, string Version) : Node(FullName, Name)
    {
        public override string Label { get; } = "Package";

        public override (string, IDictionary<string, object>) GetSetQueryAndParameters(string node)
        {
            var upNode = node.ToUpper();
            var (query, parameters) = base.GetSetQueryAndParameters(node);

            query = $"{query}, {node}.version = $node{upNode}Version";
            parameters.Add($"node{upNode}Version", Version);
            return (query, parameters);
        }
        
        protected override string GetPrimaryKey()
        {
            return $"{FullName}{Version}".GetHashCode().ToString();
        }
    }
}