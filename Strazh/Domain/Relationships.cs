namespace Strazh.Domain
{
    public abstract class Relationship
    {
        public abstract string Type { get; }
    }

    public class HaveRelationship : Relationship
    {
        public override string Type => "HAVE";

        public static HaveRelationship Instance { get; } = new();
    }

    public class InvokeRelationship : Relationship
    {
        public override string Type => "INVOKE";
        public static InvokeRelationship Instance { get; } = new();
    }

    public class ConstructRelationship : Relationship
    {
        public override string Type => "CONSTRUCT";
        public static ConstructRelationship Instance { get; } = new();
    }

    public class OfTypeRelationship : Relationship
    {
        public override string Type => "OF_TYPE";
        public static OfTypeRelationship Instance { get; } = new();
    }

    public class DeclaredAtRelationship : Relationship
    {
        public override string Type => "DECLARED_AT";
        public static DeclaredAtRelationship Instance { get; } = new();
    }

    public class IncludedInRelationship : Relationship
    {
        public override string Type => "INCLUDED_IN";
        public static IncludedInRelationship Instance { get; } = new();
    }

    public class DependsOnRelationship : Relationship
    {
        public override string Type => "DEPENDS_ON";
        public static DependsOnRelationship Instance { get; } = new();
    }
}