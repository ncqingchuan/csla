﻿using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Formatting;

namespace Csla.Analyzers
{
  [ExportCodeFixProvider(PublicNoArgumentConstructorIsMissingConstants.DiagnosticId, LanguageNames.CSharp)]
  [Shared]
  public sealed class EvaluateManagedBackingFieldsCodeFix
    : CodeFixProvider
  {
    public override ImmutableArray<string> FixableDiagnosticIds
    {
      get
      {
        return ImmutableArray.Create(EvaluateManagedBackingFieldsAnalayzerConstants.DiagnosticId);
      }
    }

    public sealed override FixAllProvider GetFixAllProvider()
    {
      return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
      var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

      if (context.CancellationToken.IsCancellationRequested) { return; }

      var diagnostic = context.Diagnostics.First();
      var fieldNode = root.FindNode(diagnostic.Location.SourceSpan) as FieldDeclarationSyntax;

      if (context.CancellationToken.IsCancellationRequested) { return; }

      var newFieldNode = fieldNode;

      newFieldNode = newFieldNode.WithModifiers(SyntaxFactory.TokenList(
          SyntaxFactory.Token(SyntaxKind.PublicKeyword),
          SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword),
          SyntaxFactory.Token(SyntaxKind.StaticKeyword)));

      var newRoot = root.ReplaceNode(fieldNode, newFieldNode);

      context.RegisterCodeFix(
        CodeAction.Create(
          EvaluateManagedBackingFieldsCodeFixConstants.FixManagedBackingFieldDescription,
          _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
          EvaluateManagedBackingFieldsCodeFixConstants.FixManagedBackingFieldDescription), diagnostic);
    }
  }
}