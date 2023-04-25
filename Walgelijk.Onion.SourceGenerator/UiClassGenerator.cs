using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Walgelijk.Onion.SourceGenerator
{
    [Generator]
    public class UiClassGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxTrees = context.Compilation.SyntaxTrees;

            IEnumerable<INamedTypeSymbol> classesImplementingInterface =
                context.Compilation.SyntaxTrees
                .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>())
                .Select(classSyntax => context.Compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax))
                .OfType<INamedTypeSymbol>()
                .Where(classSymbol => classSymbol.AllInterfaces.Any(interfaceSymbol => interfaceSymbol.Name == "IControl"));

            var sourceBuilder = new StringBuilder();

            sourceBuilder.AppendLine("using System.Runtime.CompilerServices;");
            sourceBuilder.AppendLine("using System;");
            sourceBuilder.AppendLine("using System.Collections;");
            sourceBuilder.AppendLine("using System.Collections.Generic;");
            sourceBuilder.AppendLine("using Walgelijk;");
            sourceBuilder.AppendLine("using Walgelijk.SimpleDrawing;");
            sourceBuilder.AppendLine("using Walgelijk.Onion;");
            sourceBuilder.AppendLine("using Walgelijk.Onion.Controls;");
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine("namespace Walgelijk.Onion;");
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine("public static class Ui");
            sourceBuilder.AppendLine("{");
            foreach (var controlStruct in classesImplementingInterface)
            {
                var controlFunctions = GetControlFunctions(controlStruct).ToArray();
                foreach (var func in controlFunctions)
                {
                    var funcName = controlStruct.Name;

                    if (controlFunctions.Length > 1 && func.Name != "Start")
                        funcName = func.Name + funcName;

                    string typeParamStr = string.Empty;

                    for (int i = 0; i < func.TypeParameters.Length; i++)
                    {
                        var t = func.TypeParameters[i];

                        if (i == 0)
                            typeParamStr += '<';

                        typeParamStr += t.ToDisplayString();

                        if (func.TypeParameters.Length > 1)
                            typeParamStr += ", ";

                        if (i == func.TypeParameters.Length - 1)
                            typeParamStr += '>';
                    }

                    sourceBuilder.AppendLine(PrintXmlDocs(func.GetDocumentationCommentXml()));
                    sourceBuilder.Append($"\tpublic static {func.ReturnType.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)} {funcName}{typeParamStr}({GetParameterString(func)})");
                    if (!func.TypeParameters.IsEmpty)
                    {
                        foreach (var typeParameter in func.TypeParameters)
                        {
                            var constraintBuilder = new StringBuilder(" where ");
                            constraintBuilder.Append(typeParameter.Name);
                            constraintBuilder.Append(" : ");
                            bool multiple = false;

                            if (typeParameter.HasConstructorConstraint)
                            {
                                constraintBuilder.Append("new()");
                                multiple = true;
                            }

                            if (typeParameter.HasNotNullConstraint)
                            {
                                if (multiple) constraintBuilder.Append(", ");
                                constraintBuilder.Append("notnull");
                                multiple = true;
                            }

                            if (typeParameter.HasReferenceTypeConstraint)
                            {
                                if (multiple) constraintBuilder.Append(", ");
                                constraintBuilder.Append("class");
                                multiple = true;
                            }

                            if (typeParameter.HasUnmanagedTypeConstraint)
                            {
                                if (multiple) constraintBuilder.Append(", ");
                                constraintBuilder.Append("unmanaged");
                                multiple = true;
                            }

                            if (typeParameter.HasValueTypeConstraint)
                            {
                                if (multiple) constraintBuilder.Append(", ");
                                constraintBuilder.Append("struct");
                                multiple = true;
                            }

                            if (!typeParameter.ConstraintTypes.IsEmpty)
                            {
                                if (multiple) constraintBuilder.Append(", ");
                                for (int i = 0; i < typeParameter.ConstraintTypes.Length; i++)
                                {
                                    var typeParamConstraintType = typeParameter.ConstraintTypes[i];

                                    constraintBuilder.Append(typeParamConstraintType.Name);
                                }
                                multiple = true;
                            }

                            if (multiple)
                                sourceBuilder.Append(constraintBuilder.ToString());
                        }
                    }
                    sourceBuilder.AppendLine();

                    sourceBuilder.AppendLine("\t{");
                    var ff = string.Join("", controlStruct.ToDisplayParts().Where(a => a.Kind != SymbolDisplayPartKind.TypeParameterName));
                    if (!controlStruct.TypeParameters.IsEmpty)
                    {
                        ff = ff.Substring(0, ff.LastIndexOf('.'));
                        ff += '.' + controlStruct.Name;
                    }
                    sourceBuilder.AppendLine($"\t\t{(func.ReturnsVoid ? string.Empty : "return ")}{ff}{typeParamStr}.{func.Name}({GetParameterNamesString(func)});");
                    sourceBuilder.AppendLine("\t}");
                    sourceBuilder.AppendLine();
                }
            }

            sourceBuilder.AppendLine("\tpublic static void End() { Onion.Tree.End(); }");

            sourceBuilder.AppendLine("}");
            sourceBuilder.AppendLine();

            context.AddSource("Ui.g.cs", sourceBuilder.ToString());
        }

        public void Initialize(GeneratorInitializationContext context)
        {

        }

        public static string PrintXmlDocs(string input)
        {
            var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = "/// " + lines[i];
            }
            return lines.Length >= 3 ? string.Join("\n", lines.Skip(1).Take(lines.Length - 3)) : string.Empty;
        }

        public static string GetParameterString(IMethodSymbol func)
        {
            var parameterStrings = func.Parameters.Select(p =>
            {
                var attrStr = string.Empty;
                var attr = p.GetAttributes();

                if (attr.Length > 0)
                {
                    attrStr += '[';
                    for (int i = 0; i < attr.Length; i++)
                    {
                        var item = attr[i];
                        attrStr += item.ApplicationSyntaxReference?.GetSyntax()?.GetText().ToString();
                        if (i == attr.Length - 1 && attr.Length > 1)
                            attrStr += ", ";
                    }
                    attrStr += "] ";
                }

                var defaultValueStr = string.Empty;
                if (p.HasExplicitDefaultValue)
                {
                    defaultValueStr = $" = {p.ExplicitDefaultValue ?? ("default")}".ToLower();
                }

                return $"{attrStr}{(p.RefKind == RefKind.None ? string.Empty : $"{p.RefKind.ToString().ToLower()} ")}{p.Type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)} {p.Name}{defaultValueStr}";
            });

            return string.Join(", ", parameterStrings);
        }

        public static string GetParameterNamesString(IMethodSymbol methodSymbol)
        {
            var parameterStrings = methodSymbol.Parameters.Select(p =>
            {
                return $"{(p.RefKind == RefKind.None ? string.Empty : $"{p.RefKind.ToString().ToLower()} ")}{p.Name}";
            });
            return string.Join(", ", parameterStrings);
        }

        public static IEnumerable<IMethodSymbol> GetControlFunctions(INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where(method =>
                    method.MethodKind == MethodKind.Ordinary &&
                    method.DeclaredAccessibility == Accessibility.Public &&
                    method.IsStatic &&
                    //method.TypeParameters.IsEmpty &&
                    //method.TypeArguments.IsEmpty &&
                    method.Parameters.Any(parameter =>
                        HasAttribute(parameter, "System.Runtime.CompilerServices.CallerLineNumberAttribute")));
        }

        private static bool HasAttribute(IParameterSymbol parameterSymbol, string attributeName)
        {
            return parameterSymbol
                .GetAttributes()
                .Any(attribute => attribute.AttributeClass?.ToDisplayString() == attributeName);
        }
    }
}
