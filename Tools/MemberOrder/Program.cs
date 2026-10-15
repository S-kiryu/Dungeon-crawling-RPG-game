using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

if (args.Length != 1 || !Directory.Exists(args[0]))
{
    Console.Error.WriteLine("Usage: MemberOrder <source-directory>");
    return 1;
}

string sourceDirectory = Path.GetFullPath(args[0]);
MemberOrderRewriter rewriter = new();
int changedFileCount = 0;

foreach (string file in Directory.EnumerateFiles(
             sourceDirectory,
             "*.cs",
             SearchOption.AllDirectories))
{
    string source = File.ReadAllText(file);
    SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
    SyntaxNode root = tree.GetRoot();
    SyntaxNode rewritten = rewriter.Visit(root)!;
    string result = rewritten.ToFullString();

    if (result == source)
        continue;

    File.WriteAllText(file, result);
    changedFileCount++;
}

Console.WriteLine($"Reordered {changedFileCount} file(s).");
return 0;

internal sealed class MemberOrderRewriter : CSharpSyntaxRewriter
{
    private static readonly HashSet<string> UnityMessageNames = new()
    {
        "Awake",
        "OnEnable",
        "Start",
        "FixedUpdate",
        "Update",
        "LateUpdate",
        "OnDisable",
        "OnDestroy",
        "OnValidate",
        "Reset"
    };

    public override SyntaxNode? VisitClassDeclaration(
        ClassDeclarationSyntax node)
    {
        ClassDeclarationSyntax visited =
            (ClassDeclarationSyntax)base.VisitClassDeclaration(node)!;

        return visited.WithMembers(Order(visited.Members));
    }

    public override SyntaxNode? VisitStructDeclaration(
        StructDeclarationSyntax node)
    {
        StructDeclarationSyntax visited =
            (StructDeclarationSyntax)base.VisitStructDeclaration(node)!;

        return visited.WithMembers(Order(visited.Members));
    }

    private static SyntaxList<MemberDeclarationSyntax> Order(
        SyntaxList<MemberDeclarationSyntax> members)
    {
        IEnumerable<MemberDeclarationSyntax> ordered = members
            .Select((member, index) => new
            {
                Member = member,
                Index = index,
                Rank = GetRank(member)
            })
            .OrderBy(item => item.Rank)
            .ThenBy(item => item.Index)
            .Select(item => item.Member);

        return SyntaxFactory.List(ordered);
    }

    private static int GetRank(MemberDeclarationSyntax member)
    {
        if (member is FieldDeclarationSyntax field)
        {
            if (field.Modifiers.Any(SyntaxKind.ConstKeyword))
                return 0;

            if (field.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
            {
                return 1;
            }

            return IsPublic(field.Modifiers) ? 2 : 6;
        }

        if (IsPublicMember(member))
        {
            return member is ConstructorDeclarationSyntax ? 4 : 3;
        }

        if (member is MethodDeclarationSyntax method &&
            UnityMessageNames.Contains(method.Identifier.ValueText))
        {
            return 8;
        }

        if (member is ConstructorDeclarationSyntax)
            return 7;

        if (member is PropertyDeclarationSyntax or
            EventDeclarationSyntax or
            IndexerDeclarationSyntax)
        {
            return 7;
        }

        if (member is MethodDeclarationSyntax)
            return 9;

        return 10;
    }

    private static bool IsPublicMember(MemberDeclarationSyntax member)
    {
        return member switch
        {
            BaseFieldDeclarationSyntax field => IsPublic(field.Modifiers),
            BaseMethodDeclarationSyntax method => IsPublic(method.Modifiers),
            BasePropertyDeclarationSyntax property => IsPublic(property.Modifiers),
            BaseTypeDeclarationSyntax type => IsPublic(type.Modifiers),
            DelegateDeclarationSyntax declaration => IsPublic(declaration.Modifiers),
            _ => false
        };
    }

    private static bool IsPublic(SyntaxTokenList modifiers)
    {
        return modifiers.Any(SyntaxKind.PublicKeyword);
    }
}
