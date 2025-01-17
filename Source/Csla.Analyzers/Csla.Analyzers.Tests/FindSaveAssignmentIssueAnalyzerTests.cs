﻿using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Csla.Analyzers.Tests
{
  [TestClass]
  public sealed class FindSaveAssignmentIssueAnalyzerTests
  {
    [TestMethod]
    public void VerifySupportedDiagnostics()
    {
      var analyzer = new FindSaveAssignmentIssueAnalyzer();
      var diagnostics = analyzer.SupportedDiagnostics;
      Assert.AreEqual(2, diagnostics.Length);

      var saveDiagnostic = diagnostics.Single(_ => _.Id == FindSaveAssignmentIssueAnalyzerConstants.DiagnosticId);
      Assert.AreEqual(saveDiagnostic.Title.ToString(), FindSaveAssignmentIssueAnalyzerConstants.Title,
        nameof(DiagnosticDescriptor.Title));
      Assert.AreEqual(saveDiagnostic.MessageFormat.ToString(), FindSaveAssignmentIssueAnalyzerConstants.Message,
        nameof(DiagnosticDescriptor.MessageFormat));
      Assert.AreEqual(saveDiagnostic.Category, FindSaveAssignmentIssueAnalyzerConstants.Category,
        nameof(DiagnosticDescriptor.Category));
      Assert.AreEqual(saveDiagnostic.DefaultSeverity, DiagnosticSeverity.Error,
        nameof(DiagnosticDescriptor.DefaultSeverity));

      var saveAsyncDiagnostic = diagnostics.Single(_ => _.Id == FindSaveAsyncAssignmentIssueAnalyzerConstants.DiagnosticId);
      Assert.AreEqual(saveAsyncDiagnostic.Title.ToString(), FindSaveAsyncAssignmentIssueAnalyzerConstants.Title,
        nameof(DiagnosticDescriptor.Title));
      Assert.AreEqual(saveAsyncDiagnostic.MessageFormat.ToString(), FindSaveAsyncAssignmentIssueAnalyzerConstants.Message,
        nameof(DiagnosticDescriptor.MessageFormat));
      Assert.AreEqual(saveAsyncDiagnostic.Category, FindSaveAsyncAssignmentIssueAnalyzerConstants.Category,
        nameof(DiagnosticDescriptor.Category));
      Assert.AreEqual(saveAsyncDiagnostic.DefaultSeverity, DiagnosticSeverity.Error,
        nameof(DiagnosticDescriptor.DefaultSeverity));
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveIsCalledOnAnObjectThatIsNotABusinessBase()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveIsCalledOnAnObjectThatIsNotABusinessBase))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveAsyncIsCalledOnAnObjectThatIsNotABusinessBase()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveAsyncIsCalledOnAnObjectThatIsNotABusinessBase))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveIsCalledOnAnObjectThatIsABusinessBaseAndResultIsAssigned()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveIsCalledOnAnObjectThatIsABusinessBaseAndResultIsAssigned))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveAsyncIsCalledOnAnObjectThatIsABusinessBaseAndResultIsAssigned()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveAsyncIsCalledOnAnObjectThatIsABusinessBaseAndResultIsAssigned))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveIsCalledOnAnObjectThatIsABusinessBaseAndResultIsNotAssigned()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveIsCalledOnAnObjectThatIsABusinessBaseAndResultIsNotAssigned))}.cs",
        new[] { FindSaveAssignmentIssueAnalyzerConstants.DiagnosticId });
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveAsyncIsCalledOnAnObjectThatIsABusinessBaseAndResultIsNotAssigned()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveAsyncIsCalledOnAnObjectThatIsABusinessBaseAndResultIsNotAssigned))}.cs",
        new[] { FindSaveAsyncAssignmentIssueAnalyzerConstants.DiagnosticId });
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveIsCalledOnAnObjectThatIsABusinessBaseAndResultIsReturned()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveIsCalledOnAnObjectThatIsABusinessBaseAndResultIsReturned))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveAsyncIsCalledOnAnObjectThatIsABusinessBaseAndResultIsReturned()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveAsyncIsCalledOnAnObjectThatIsABusinessBaseAndResultIsReturned))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveIsCalledOnAnObjectThatIsABusinessBaseAndResultIsReturnedInLambda()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveIsCalledOnAnObjectThatIsABusinessBaseAndResultIsReturnedInLambda))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveAsyncIsCalledOnAnObjectThatIsABusinessBaseAndResultIsReturnedInLambda()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveAsyncIsCalledOnAnObjectThatIsABusinessBaseAndResultIsReturnedInLambda))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveIsCalledOnAnObjectThatIsABusinessBaseAndResultIsReturnedInLambdaWithBlock()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveIsCalledOnAnObjectThatIsABusinessBaseAndResultIsReturnedInLambdaWithBlock))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveAsyncIsCalledOnAnObjectThatIsABusinessBaseAndResultIsReturnedInLambdaWithBlock()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveAsyncIsCalledOnAnObjectThatIsABusinessBaseAndResultIsReturnedInLambdaWithBlock))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveIsCalledOnABusinessObjectWithinItself()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveIsCalledOnABusinessObjectWithinItself))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveAsyncIsCalledOnABusinessObjectWithinItself()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveAsyncIsCalledOnABusinessObjectWithinItself))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveIsCalledOnABusinessObjectAsThis()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveIsCalledOnABusinessObjectAsThis))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveAsyncIsCalledOnABusinessObjectAsThis()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveAsyncIsCalledOnABusinessObjectAsThis))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveIsCalledOnABusinessObjectAsBase()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveIsCalledOnABusinessObjectAsBase))}.cs",
        new string[0]);
    }

    [TestMethod]
    public async Task AnalyzeWhenSaveAsyncIsCalledOnABusinessObjectAsBase()
    {
      await TestHelpers.RunAnalysisAsync<FindSaveAssignmentIssueAnalyzer>(
        $@"Targets\{nameof(FindSaveAssignmentIssueAnalyzerTests)}\{(nameof(this.AnalyzeWhenSaveAsyncIsCalledOnABusinessObjectAsBase))}.cs",
        new string[0]);
    }
  }
}
