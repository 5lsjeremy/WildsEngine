# Wilds P0 Processing Audit

Generated: 2026-10-27 06:32:52

Repository: `C:\Users\jmatt\source\WildsEngine`

## Certification

**P0 READY WITH WARNINGS**

All required checks passed, but non-blocking audit warnings remain.

## Summary

- Passed: 54
- Warnings: 1
- Failed: 0

## Findings

| Status | Area | Finding | Evidence |
|---|---|---|---|
| PASS | Required files | Required file exists. | WildsEngine.Core\Abstractions\IBaseObject.cs |
| PASS | Required files | Required file exists. | WildsEngine.Core\Abstractions\IComponent.cs |
| PASS | Required files | Required file exists. | WildsEngine.Core\Abstractions\IState.cs |
| PASS | Required files | Required file exists. | WildsEngine.Core\Abstractions\IStateInstruction.cs |
| PASS | Required files | Required file exists. | WildsEngine.Core\Abstractions\IContext.cs |
| PASS | Required files | Required file exists. | WildsEngine.Core\Abstractions\ISignal.cs |
| PASS | Required files | Required file exists. | WildsEngine.Core\Objects\BaseObject.cs |
| PASS | Required files | Required file exists. | WildsEngine.Core\Components\BaseComponent.cs |
| PASS | Required files | Required file exists. | WildsEngine.Core.Tests\ArbitraryObjectProcessingTests.cs |
| PASS | Required files | Required file exists. | WildsEngine.Core.Tests\NullValueContractTests.cs |
| PASS | Target framework | WildsEngine.Core targets netstandard2.1. | WildsEngine.Core.csproj |
| PASS | IBaseObject | Canonical GUID identity is present. | IBaseObject.cs |
| PASS | IBaseObject | Slug identity is present. | IBaseObject.cs |
| PASS | IBaseObject | Neighbor exposure is read-only. | IBaseObject.cs |
| PASS | IBaseObject | Component gateway is present. | IBaseObject.cs |
| PASS | IBaseObject | Appendable slug behavior is present. | IBaseObject.cs |
| PASS | IBaseObject | Neighbor addition reports topology change. | IBaseObject.cs |
| PASS | IBaseObject | Neighbor removal uses GUID identity. | IBaseObject.cs |
| PASS | IBaseObject ownership | Component-owned shortcut is not redeclared on IBaseObject. | \bIState\s+State\s*\{ |
| PASS | IBaseObject ownership | Component-owned shortcut is not redeclared on IBaseObject. | \bISignal\s+Signal\s*\{ |
| PASS | IBaseObject ownership | Component-owned shortcut is not redeclared on IBaseObject. | IDictionary\s*<[^>]+>\s+Metadata\s*\{ |
| PASS | IComponent | IComponent extends IBaseObject. | IComponent.cs |
| PASS | IComponent | IComponent owns State. | IComponent.cs |
| PASS | IComponent | IComponent owns Signal. | IComponent.cs |
| PASS | IComponent | Component metadata explicitly supports nullable arbitrary values. | IComponent.cs |
| PASS | BaseObject shortcuts | State shortcut resolves through Component.State. | BaseObject.cs |
| PASS | BaseObject shortcuts | Signal shortcut resolves through Component.Signal. | BaseObject.cs |
| PASS | BaseObject shortcuts | Metadata shortcut resolves through Component.Metadata. | BaseObject.cs |
| PASS | BaseComponent | BaseComponent resolves Component to itself. | BaseComponent.cs |
| PASS | BaseComponent | BaseComponent owns nullable component metadata. | BaseComponent.cs |
| PASS | BaseComponent | Concrete components must provide State. | BaseComponent.cs |
| PASS | BaseComponent | Concrete components must provide Signal. | BaseComponent.cs |
| PASS | IState | IState extends IBaseObject. | IState.cs |
| PASS | IState | IState owns Context. | IState.cs |
| PASS | IState | IState processes a Wilds-native instruction. | IState.cs |
| PASS | IState | Process does not receive duplicate Context. | IState.cs |
| PASS | IStateInstruction | Wilds-native instruction boundary exists. | IStateInstruction.cs |
| PASS | IContext | Variables explicitly support nullable arbitrary values. | IContext.cs |
| PASS | IContext | Context Metadata explicitly supports nullable arbitrary values. | IContext.cs |
| PASS | IContext | SetVariable accepts null. | IContext.cs |
| PASS | IContext | SetMetadata accepts null. | IContext.cs |
| PASS | QSL isolation | WildsEngine.Core contains no direct QSL references. | WildsEngine.Core |
| PASS | P0 test matrix | Reference-type consumer object evidence was found. | State_processes_external_reference_type_without_core_knowledge |
| PASS | P0 test matrix | Value-type consumer object evidence was found. | State_processes_external_value_type_without_core_knowledge |
| PASS | P0 test matrix | Reference identity preservation evidence was found. | Assert\.Same\s*\(\s*payload\s*,\s*stored\s*\) |
| PASS | P0 test matrix | Null present versus missing evidence was found. | NullValue_IsDistinctFromMissingKey |
| PASS | P0 test matrix | State value-to-null transition evidence was found. | State_processing_can_transition_existing_value_to_null |
| PASS | P0 test matrix | State null-to-value transition evidence was found. | State_processing_can_transition_null_to_consumer_object |
| PASS | P0 test matrix | Nullable instruction payload evidence was found. | public\s+object\?\s+Value |
| PASS | P0 test matrix | Nullable TestContext variables evidence was found. | Dictionary\s*<\s*string\s*,\s*object\?\s*>\s+_variables |
| PASS | P0 test matrix | Nullable TestContext setters evidence was found. | SetVariable\s*\(\s*string\s+key\s*,\s*object\?\s+value\s*\) |
| PASS | Build | WildsEngine.sln builds successfully. | Determining projects to restore...<br>  All projects are up-to-date for restore.<br>  WildsEngine.Core -> C:\Users\jmatt\source\WildsEngine\WildsEngine.Core\bin\Debug\netstandard2.1\WildsEngine.Core.dll<br>  WildsEngine.Qsl -> C:\Users\jmatt\source\WildsEngine\WildsEngine.Qsl\bin\Debug\netstandard2.1\WildsEngine.Qsl.dll<br>  WildsEngine.Console -> C:\Users\jmatt\source\WildsEngine\WildsEngine.Console\bin\Debug\net10.0\WildsEngine.Console.dll<br>  WildsEngine.Core.Tests -> C:\Users\jmatt\source\WildsEngine\WildsEngine.Core.Tests\bin\Debug\net10.0\WildsEngine.Core.Tests.dll<br><br>Build succeeded.<br>    0 Warning(s)<br>    0 Error(s)<br><br>Time Elapsed 00:00:00.87 |
| PASS | Tests | WildsEngine.Core.Tests passed. | Test run for C:\Users\jmatt\source\WildsEngine\WildsEngine.Core.Tests\bin\Debug\net10.0\WildsEngine.Core.Tests.dll (.NETCoreApp,Version=v10.0)<br>A total of 1 test files matched the specified pattern.<br><br>Passed!  - Failed:     0, Passed:    14, Skipped:     0, Total:    14, Duration: 41 ms - WildsEngine.Core.Tests.dll (net10.0) |
| PASS | Compiler warnings | Solution build contains no compiler warnings. | dotnet build |
| WARN | Test count | The expected 14-test summary was not detected; inspect test output. | Test run for C:\Users\jmatt\source\WildsEngine\WildsEngine.Core.Tests\bin\Debug\net10.0\WildsEngine.Core.Tests.dll (.NETCoreApp,Version=v10.0)<br>A total of 1 test files matched the specified pattern.<br><br>Passed!  - Failed:     0, Passed:    14, Skipped:     0, Total:    14, Duration: 41 ms - WildsEngine.Core.Tests.dll (net10.0) |

## Ownership Checkpoint

- `IBaseObject` owns identity, slug, neighbors, and the Component gateway.
- `IComponent` owns State, Signal, and component Metadata.
- `BaseObject` supplies State, Signal, and Metadata shortcuts through Component.
- `IState` owns Context and processes `IStateInstruction`.
- `IContext` owns nullable Variables and contextual Metadata.
- Context is not passed into `IState.Process()`.
- WildsEngine.Core does not depend directly on QSL.

## Build Output

``text
  Determining projects to restore...
  All projects are up-to-date for restore.
  WildsEngine.Core -> C:\Users\jmatt\source\WildsEngine\WildsEngine.Core\bin\Debug\netstandard2.1\WildsEngine.Core.dll
  WildsEngine.Qsl -> C:\Users\jmatt\source\WildsEngine\WildsEngine.Qsl\bin\Debug\netstandard2.1\WildsEngine.Qsl.dll
  WildsEngine.Console -> C:\Users\jmatt\source\WildsEngine\WildsEngine.Console\bin\Debug\net10.0\WildsEngine.Console.dll
  WildsEngine.Core.Tests -> C:\Users\jmatt\source\WildsEngine\WildsEngine.Core.Tests\bin\Debug\net10.0\WildsEngine.Core.Tests.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:00.87
``

## Test Output

``text
Test run for C:\Users\jmatt\source\WildsEngine\WildsEngine.Core.Tests\bin\Debug\net10.0\WildsEngine.Core.Tests.dll (.NETCoreApp,Version=v10.0)
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    14, Skipped:     0, Total:    14, Duration: 41 ms - WildsEngine.Core.Tests.dll (net10.0)
``
