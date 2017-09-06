using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace net.ajennings.EnumAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EnumAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor NotExhaustedRule = new DiagnosticDescriptor("ENUM001", "Unreferenced enum values", "enum value(s) not referenced in enclosing block: {0}", "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);
        private static DiagnosticDescriptor NotExhaustedNonEnum = new DiagnosticDescriptor("ENUM002", "enum type required", "EnumNotExhaustedException must be used with enum", "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(NotExhaustedRule, NotExhaustedNonEnum); } }

        public override void Initialize(AnalysisContext context) => context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ThrowStatement);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var throwStatement = (ThrowStatementSyntax)context.Node;
            if (!(throwStatement.Expression is ObjectCreationExpressionSyntax))
            {
                return;
            }
            var objectCreation = (ObjectCreationExpressionSyntax)throwStatement.Expression;

            var objectType = context.SemanticModel.GetTypeInfo(objectCreation);
            if (!(objectType.Type is INamedTypeSymbol))
            {
                return;
            }
            var namedType = (INamedTypeSymbol)objectType.Type;

            var enumNotExhaustedType = context.SemanticModel.Compilation.GetTypeByMetadataName("net.ajennings.EnumNotExhaustedException`1");
            if (!namedType.OriginalDefinition.Equals(enumNotExhaustedType) || namedType.TypeArguments.Length == 0)
            {
                return;
            }

            var typeArg = namedType.TypeArguments[0];
            if (typeArg.TypeKind != TypeKind.Enum)
            {
                context.ReportDiagnostic(Diagnostic.Create(NotExhaustedNonEnum, context.Node.GetLocation()));
            }
            else if (typeArg is INamedTypeSymbol)
            {
                var allEnumValues = new HashSet<string>(((INamedTypeSymbol)typeArg).MemberNames);
                if (allEnumValues.Any())
                {
                    foreach (SyntaxNode node in throwStatement.Parent.DescendantNodes())
                    {
                        if (node is MemberAccessExpressionSyntax)
                        {
                            var memberAccess = (MemberAccessExpressionSyntax)node;

                            var memberAccessType = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
                            if (memberAccessType.Type.Equals(typeArg) && memberAccess.Name is IdentifierNameSyntax)
                            {
                                if (allEnumValues.Remove(memberAccess.Name.Identifier.Text) && allEnumValues.Count == 0)
                                {
                                    return;
                                }
                            }
                        }
                    }

                    context.ReportDiagnostic(Diagnostic.Create(NotExhaustedRule, context.Node.GetLocation(), string.Join(",", allEnumValues)));
                }
            }
        }
    }
}
