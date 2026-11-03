using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using QSL.Core;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;
using WildsEngine.Core.Abstractions;
using WildsEngine.Qsl;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;

namespace WildsEngine.Core.Tests.Certification
{
    internal sealed class WildsQslCertificationSuite
    {
        [JsonPropertyName("tests")]
        public List<WildsQslCertificationTest> Tests { get; set; } =
            new List<WildsQslCertificationTest>();
    }

    internal sealed class WildsQslCertificationTest
    {
        [JsonPropertyName("category")]
        public string Category { get; set; } =
            string.Empty;

        [JsonPropertyName("statement")]
        public string Statement { get; set; } =
            string.Empty;

        [JsonPropertyName("expected")]
        public string Expected { get; set; } =
            string.Empty;
        
        [JsonPropertyName("dependency")]
        public string? Dependency { get; set; }
    }

    public sealed class WildsQslCertificationReport
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("passed")]
        public int Passed { get; set; }

        [JsonPropertyName("failed")]
        public int Failed { get; set; }
        
        [JsonPropertyName("missingDependencies")]
        public int MissingDependencies { get; set; }

        [JsonPropertyName("completedMoments")]
        public int CompletedMoments { get; set; }

        [JsonPropertyName("failedMoments")]
        public int FailedMoments { get; set; }

        [JsonPropertyName("timelineMoments")]
        public int TimelineMoments { get; set; }

        [JsonPropertyName("epochTimelines")]
        public int EpochTimelines { get; set; }

        [JsonPropertyName("results")]
        public List<WildsQslCertificationResult> Results { get; set; } =
            new List<WildsQslCertificationResult>();

        [JsonIgnore]
        public bool IsCertified =>
            Total > 0 &&
            Passed == Total &&
            Failed == 0 &&
            MissingDependencies == 0;
    }

    public sealed class WildsQslCertificationResult
    {
        [JsonPropertyName("category")]
        public string Category { get; set; } =
            string.Empty;

        [JsonPropertyName("statement")]
        public string Statement { get; set; } =
            string.Empty;

        [JsonPropertyName("expected")]
        public string Expected { get; set; } =
            string.Empty;

        [JsonPropertyName("output")]
        public string Output { get; set; } =
            string.Empty;

        [JsonPropertyName("dependency")]
        public string? Dependency { get; set; }

        [JsonPropertyName("status")]
        public WildsQslCertificationStatus Status { get; set; }

        [JsonPropertyName("passed")]
        public bool Passed { get; set; }

        [JsonPropertyName("qslSucceeded")]
        public bool QslSucceeded { get; set; }

        [JsonPropertyName("enteredWilds")]
        public bool EnteredWilds { get; set; }

        [JsonPropertyName("momentCompleted")]
        public bool MomentCompleted { get; set; }

        [JsonPropertyName("timelineRecorded")]
        public bool TimelineRecorded { get; set; }

        [JsonPropertyName("epochRecorded")]
        public bool EpochRecorded { get; set; }

        [JsonPropertyName("momentOutcome")]
        public string MomentOutcome { get; set; } =
            string.Empty;

        [JsonPropertyName("parseFailed")]
        public bool ParseFailed { get; set; }

        [JsonPropertyName("evaluationFailed")]
        public bool EvaluationFailed { get; set; }

        [JsonPropertyName("parserDiagnostics")]
        public string[] ParserDiagnostics { get; set; } =
            Array.Empty<string>();

        [JsonPropertyName("runtimeDiagnostics")]
        public string[] RuntimeDiagnostics { get; set; } =
            Array.Empty<string>();

        [JsonPropertyName("exception")]
        public string? Exception { get; set; }
    }

    public static class WildsQslCertification
    {
        private const string SuiteFileName =
            "qsl_statement_testing.json";

        private const string ReportFileName =
            "wilds_qsl_statement_testing_output.json";

        private static readonly List<string> Passed =
            new List<string>();

        public static async Task<WildsQslCertificationReport> RunAllAsync()
        {
            Passed.Clear();

            var suitePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Certification",
                    SuiteFileName);

            var suite =
                LoadSuite(
                    suitePath);

            var qslEngine =
                CreateQslEngine();

            var component =
                CreateComponent(
                    "certification.qsl.200");

            using var wildsEngine =
                CreateWildsEngine(
                    component);

            Assert(
                wildsEngine.Initialize(),
                "WildsEngine initialization should succeed.");

            var executor =
                new WildsQslExecutor(
                    qslEngine,
                    wildsEngine);

            var report =
                new WildsQslCertificationReport();

            TestLog.Write(
                "=== Wilds / QSL 200-Case Tests ===");

            TestLog.State(
                "Route: QSL source -> WildsQslExecutor -> " +
                "WildsEngine -> Pulse -> WorkUnit -> " +
                "QSL -> Moment -> Timeline -> Epoch");

            TestLog.State(
                "Suite: " +
                suitePath);

            TestLog.State(
                "Tests: " +
                suite.Tests.Count);

            Console.WriteLine();

            for (
                var index = 0;
                index < suite.Tests.Count;
                index++)
            {
                var test =
                    suite.Tests[index];

                var result =
                    await ExecuteTestAsync(
                            executor,
                            wildsEngine,
                            test)
                        .ConfigureAwait(false);

                report.Results.Add(
                    result);

                switch (result.Status)
                {
                    case WildsQslCertificationStatus.Passed:
                        report.Passed++;
                        Passed.Add(
                            CreateTestName(
                                index,
                                test));
                        break;

                    case WildsQslCertificationStatus.MissingDependency:
                        report.MissingDependencies++;
                        break;

                    case WildsQslCertificationStatus.Failed:
                        report.Failed++;
                        break;

                    default:
                        throw new InvalidOperationException(
                            "Unknown certification status: " +
                            result.Status);
                }

                if (result.MomentOutcome ==
                    MomentOutcome.Completed.ToString())
                {
                    report.CompletedMoments++;
                }

                if (result.MomentOutcome ==
                    MomentOutcome.Failed.ToString())
                {
                    report.FailedMoments++;
                }

                Console.Write(
                    result.Status switch
                    {
                        WildsQslCertificationStatus.Passed => ".",
                        WildsQslCertificationStatus.MissingDependency => "M",
                        WildsQslCertificationStatus.Failed => "F",
                        _ => "?"
                    });

                if ((index + 1) % 50 == 0)
                {
                    Console.WriteLine();
                }
            }

            if (suite.Tests.Count % 50 != 0)
            {
                Console.WriteLine();
            }

            report.Total =
                report.Results.Count;

            report.TimelineMoments =
                wildsEngine.Timeline.Count;

            report.EpochTimelines =
                wildsEngine.Epoch.Timelines.Count;

            ValidateCompleteHistory(
                suite,
                wildsEngine);

            WriteSummary(
                report);

            if (report.Failed > 0)
            {
                WriteFailures(
                    report.Results);
            }

            if (report.MissingDependencies > 0)
            {
                WriteMissingDependencies(
                    report.Results);
            }

            WriteReport(
                report);

            Assert(
                report.Total ==
                suite.Tests.Count,
                "Certification report must contain one result " +
                "for every suite case.");

            Assert(
                report.Total ==
                report.Passed +
                report.Failed +
                report.MissingDependencies,
                "Every certification result must be accounted for " +
                "as passed, failed, or missing a dependency.");

            TestLog.Write(
                "Wilds / QSL diagnostic run completed: " +
                report.Passed +
                "/" +
                report.Total +
                " behavioral cases passed.");

            TestLog.State(
                "Certification status: " +
                GetCertificationStatus(
                    report));
            TestLog.State(
                "Missing Dependencies: " +
                report.MissingDependencies);

            TestLog.State(
                "Completed Moments:    " +
                report.CompletedMoments);

            TestLog.State(
                "Failed Moments: " +
                report.FailedMoments);

            TestLog.State(
                "Timeline Moments: " +
                report.TimelineMoments);

            TestLog.State(
                "Epoch Timelines: " +
                report.EpochTimelines);

            return report;

        }

        private static WildsQslCertificationSuite LoadSuite(
            string suitePath)
        {
            if (!File.Exists(
                    suitePath))
            {
                throw new FileNotFoundException(
                    "Wilds / QSL certification suite " +
                    "was not found.",
                    suitePath);
            }

            try
            {
                var json =
                    File.ReadAllText(
                        suitePath);

                var suite =
                    JsonSerializer.Deserialize
                        <WildsQslCertificationSuite>(
                            json,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive =
                                    true
                            });

                if (suite == null)
                {
                    throw new InvalidOperationException(
                        "Certification suite deserialized to null.");
                }

                if (suite.Tests.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Certification suite contains no tests.");
                }

                return suite;
            }
            catch (Exception exception)
                when (exception is not
                      FileNotFoundException)
            {
                throw new InvalidOperationException(
                    "Certification suite could not be loaded.",
                    exception);
            }
        }

        private static async Task<WildsQslCertificationResult>
            ExecuteTestAsync(
                WildsQslExecutor executor,
                global::WildsEngine.Core.WildsEngine
                    wildsEngine,
                WildsQslCertificationTest test)
        {
            var expectsError =
                string.Equals(
                    test.Category,
                    "Error",
                    StringComparison.OrdinalIgnoreCase);

            var timelineBefore =
                wildsEngine.Timeline.Count;

            IMoment moment;

            try
            {
                /*
                 * This is the important certification boundary.
                 *
                 * The QSL source enters Wilds directly.
                 * Wilds owns the Pulse and WorkUnit before QSL
                 * evaluates the statement.
                 */
                moment =
                    await executor.ExecuteAsync(
                            test.Statement)
                        .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                /*
                 * WildsEngine.ExecuteAsync is expected to turn
                 * operation failures into failed Moments.
                 *
                 * An exception escaping the executor means the
                 * complete Wilds route did not resolve honestly.
                 */
                return new WildsQslCertificationResult
                {
                    Category =
                        test.Category,

                    Statement =
                        test.Statement,

                    Expected =
                        test.Expected,

                    Output =
                        string.Empty,
                    Dependency =
                        test.Dependency,
                    Status =
                        WildsQslCertificationStatus.Failed,
                    Passed =
                        false,

                    QslSucceeded =
                        false,

                    EnteredWilds =
                        true,

                    MomentCompleted =
                        false,

                    TimelineRecorded =
                        false,

                    EpochRecorded =
                        false,

                    MomentOutcome =
                        string.Empty,

                    ParseFailed =
                        false,

                    EvaluationFailed =
                        false,

                    ParserDiagnostics =
                        Array.Empty<string>(),

                    RuntimeDiagnostics =
                        Array.Empty<string>(),

                    Exception =
                        exception.GetType().Name +
                        ": " +
                        exception.Message
                };
            }

            var timelineAfter =
                wildsEngine.Timeline.Count;

            var timelineRecorded =
                timelineAfter ==
                timelineBefore + 1 &&
                timelineAfter > 0 &&
                ReferenceEquals(
                    moment,
                    wildsEngine.Timeline
                        .Moments[timelineAfter - 1]);

            var epochRecorded =
                wildsEngine.Epoch.Count == 1 &&
                wildsEngine.Epoch.Timelines.Count == 1 &&
                ReferenceEquals(
                    wildsEngine.Timeline,
                    wildsEngine.Epoch.Timelines[0]) &&
                wildsEngine.Epoch
                    .Timelines[0]
                    .Moments
                    .Count ==
                    timelineAfter &&
                ReferenceEquals(
                    moment,
                    wildsEngine.Epoch
                        .Timelines[0]
                        .Moments[timelineAfter - 1]);

            if (moment.Outcome ==
                MomentOutcome.Completed)
            {
                var output =
                    FormatValue(
                        moment.Result);

                var passed =
                    !expectsError &&
                    moment.Exception == null &&
                    moment.WorkUnitIds.Count == 1 &&
                    timelineRecorded &&
                    epochRecorded &&
                    string.Equals(
                        output,
                        test.Expected,
                        StringComparison.Ordinal);

                return new WildsQslCertificationResult
                {
                    Category =
                        test.Category,

                    Statement =
                        test.Statement,

                    Expected =
                        test.Expected,

                    Output =
                        output,
                    Dependency =
                        test.Dependency,
                    Status =
                        ClassifyResult(
                            test,
                            passed,
                            dependencyMissing: false),
                    Passed =
                        passed,

                    QslSucceeded =
                        true,

                    EnteredWilds =
                        true,

                    MomentCompleted =
                        true,

                    TimelineRecorded =
                        timelineRecorded,

                    EpochRecorded =
                        epochRecorded,

                    MomentOutcome =
                        moment.Outcome.ToString(),

                    ParseFailed =
                        false,

                    EvaluationFailed =
                        false,

                    ParserDiagnostics =
                        Array.Empty<string>(),

                    RuntimeDiagnostics =
                        Array.Empty<string>(),

                    Exception =
                        null
                };
            }

            if (moment.Outcome ==
                    MomentOutcome.Failed &&
                moment.Exception is
                    WildsQslExecutionException
                    qslException)
            {
                var processingResult =
                    qslException.Result;

                var parserDiagnostics =
                    CopyParserDiagnostics(
                        processingResult);

                var runtimeDiagnostics =
                    CopyRuntimeDiagnostics(
                        processingResult);

                var output =
                    FormatValue(
                        processingResult.Value);
                var dependencyMissing =
                    IsDeclaredDependencyMissing(
                        test,
                        runtimeDiagnostics);
                var passed =
                    expectsError &&
                    !processingResult.Succeeded &&
                    moment.Result == null &&
                    moment.WorkUnitIds.Count == 1 &&
                    timelineRecorded &&
                    epochRecorded;

                return new WildsQslCertificationResult
                {
                    Category =
                        test.Category,

                    Statement =
                        test.Statement,

                    Expected =
                        test.Expected,

                    Output =
                        output,
                    Dependency =
                        test.Dependency,
                    Status =
                        ClassifyResult(
                            test,
                            passed,
                            dependencyMissing),
                    Passed =
                        passed &&
                        !dependencyMissing,
                    QslSucceeded =
                        processingResult.Succeeded,

                    EnteredWilds =
                        true,

                    MomentCompleted =
                        false,

                    TimelineRecorded =
                        timelineRecorded,

                    EpochRecorded =
                        epochRecorded,

                    MomentOutcome =
                        moment.Outcome.ToString(),

                    ParseFailed =
                        processingResult.ParseFailed,

                    EvaluationFailed =
                        processingResult.EvaluationFailed,

                    ParserDiagnostics =
                        parserDiagnostics,

                    RuntimeDiagnostics =
                        runtimeDiagnostics,

                    Exception =
                        processingResult.Exception?
                            .Message ??
                        qslException.Message
                };
            }

            return new WildsQslCertificationResult
            {
                Category =
                    test.Category,

                Statement =
                    test.Statement,

                Expected =
                    test.Expected,

                Output =
                    FormatValue(
                        moment.Result),
                Dependency =
                    test.Dependency,
                Status =
                    WildsQslCertificationStatus.Failed,
                Passed =
                    false,

                QslSucceeded =
                    false,

                EnteredWilds =
                    true,

                MomentCompleted =
                    false,

                TimelineRecorded =
                    timelineRecorded,

                EpochRecorded =
                    epochRecorded,

                MomentOutcome =
                    moment.Outcome.ToString(),

                ParseFailed =
                    false,

                EvaluationFailed =
                    false,

                ParserDiagnostics =
                    Array.Empty<string>(),

                RuntimeDiagnostics =
                    Array.Empty<string>(),

                Exception =
                    moment.Exception?
                        .Message ??
                    "Unexpected terminal Moment outcome: " +
                    moment.Outcome
            };
        }

        private static void ValidateCompleteHistory(
            WildsQslCertificationSuite suite,
            global::WildsEngine.Core.WildsEngine
                wildsEngine)
        {
            Assert(
                wildsEngine.Timeline.Count ==
                suite.Tests.Count,
                "Wilds Timeline must contain exactly one " +
                "Moment per certification statement.");

            Assert(
                wildsEngine.Epoch.Count == 1,
                "The isolated certification Epoch must " +
                "contain exactly one Timeline.");

            Assert(
                wildsEngine.Epoch.Timelines.Count == 1,
                "The certification Epoch Timeline collection " +
                "must contain exactly one Timeline.");

            Assert(
                ReferenceEquals(
                    wildsEngine.Timeline,
                    wildsEngine.Epoch.Timelines[0]),
                "The Epoch must retain the engine Timeline.");

            Assert(
                wildsEngine.Epoch
                    .Timelines[0]
                    .Moments
                    .Count ==
                suite.Tests.Count,
                "The Epoch must expose the complete " +
                "certification Moment history.");
        }

        private static string[]
            CopyParserDiagnostics(
                QslProcessingResult processingResult)
        {
            var diagnostics =
                new string[
                    processingResult
                        .ParserDiagnostics
                        .Count
                ];

            for (
                var index = 0;
                index < diagnostics.Length;
                index++)
            {
                diagnostics[index] =
                    processingResult
                        .ParserDiagnostics[index];
            }

            return diagnostics;
        }

        private static string[]
            CopyRuntimeDiagnostics(
                QslProcessingResult processingResult)
        {
            var diagnostics =
                new string[
                    processingResult
                        .RuntimeDiagnostics
                        .Count
                ];

            for (
                var index = 0;
                index < diagnostics.Length;
                index++)
            {
                var entry =
                    processingResult
                        .RuntimeDiagnostics[index];

                diagnostics[index] =
                    "[" +
                    entry.EntryType +
                    "] " +
                    entry.Source +
                    ": " +
                    entry.Message;
            }

            return diagnostics;
        }

        private static string FormatValue(
            object? value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is bool boolean)
            {
                return boolean
                    ? "true"
                    : "false";
            }

            if (value is string text)
            {
                return text;
            }

            if (value is char character)
            {
                return character.ToString();
            }

            if (value is IFormattable formattable)
            {
                return formattable.ToString(
                           null,
                           CultureInfo.InvariantCulture) ??
                       string.Empty;
            }

            return value.ToString() ??
                   string.Empty;
        }

        private static QslEngine CreateQslEngine()
        {
            var registry =
                new QslRuntimeRegistry();

            registry.RegisterDefaultEvaluators();

            var context =
                new QslRuntimeContext(
                    registry);

            return new QslEngine(
                context);
        }

        private static IComponent CreateComponent(
            string layer)
        {
            return new TestComponent(
                layer);
        }

        private static
            global::WildsEngine.Core.WildsEngine
            CreateWildsEngine(
                IComponent component)
        {
            return new
                global::WildsEngine.Core.WildsEngine(
                    component,
                    new WildsEngineOptions
                    {
                        Layer =
                            "certification.qsl.200",

                        RetainedInstructionsProcessors =
                            1,

                        ConcurrentInstructionsProcessors =
                            1,

                        RetainedTaskProcessors =
                            1,

                        ConcurrentTaskProcessors =
                            1
                    });
        }

        private static string CreateTestName(
            int index,
            WildsQslCertificationTest test)
        {
            return
                (index + 1) +
                ": [" +
                test.Category +
                "] " +
                test.Statement;
        }

        private static void WriteSummary(
            WildsQslCertificationReport report)
        {
            Console.WriteLine();
            Console.WriteLine(
                "========================================");

            Console.WriteLine(
                "Total:                " +
                report.Total);

            Console.WriteLine(
                "Passed:               " +
                report.Passed);

            Console.WriteLine(
                "Failed:               " +
                report.Failed);
            Console.WriteLine(
                "Missing Dependencies: " +
                report.MissingDependencies);

            Console.WriteLine(
                "Completed Moments:    " +
                report.CompletedMoments);

            Console.WriteLine(
                "Failed Moments:       " +
                report.FailedMoments);

            Console.WriteLine(
                "Timeline Moments:     " +
                report.TimelineMoments);

            Console.WriteLine(
                "Epoch Timelines:      " +
                report.EpochTimelines);

            Console.WriteLine(
                "========================================");
        }

        private static void WriteFailures(
            IReadOnlyList<WildsQslCertificationResult>
                results)
        {
            Console.WriteLine();
            Console.WriteLine(
                "Failures:");

            foreach (var result in results)
            {
                if (result.Status !=
                    WildsQslCertificationStatus.Failed)
                {
                    continue;
                }

                Console.WriteLine();
                Console.WriteLine(
                    "[" +
                    result.Category +
                    "] " +
                    result.Statement);

                Console.WriteLine(
                    "  Status:   " +
                    result.Status);
                Console.WriteLine(
                    "  Expected: " +
                    (
                        string.IsNullOrEmpty(
                            result.Expected)
                            ? "<error>"
                            : result.Expected
                    ));

                Console.WriteLine(
                    "  Actual:   " +
                    (
                        string.IsNullOrEmpty(
                            result.Output)
                            ? "<null>"
                            : result.Output
                    ));

                Console.WriteLine(
                    "  Outcome: " +
                    (
                        string.IsNullOrEmpty(
                            result.MomentOutcome)
                            ? "<none>"
                            : result.MomentOutcome
                    ));

                Console.WriteLine(
                    "  QSL succeeded: " +
                    result.QslSucceeded);

                Console.WriteLine(
                    "  Entered Wilds: " +
                    result.EnteredWilds);

                Console.WriteLine(
                    "  Moment completed: " +
                    result.MomentCompleted);

                Console.WriteLine(
                    "  Timeline recorded: " +
                    result.TimelineRecorded);

                Console.WriteLine(
                    "  Epoch recorded: " +
                    result.EpochRecorded);

                foreach (var diagnostic in
                         result.ParserDiagnostics)
                {
                    Console.WriteLine(
                        "  Parser: " +
                        diagnostic);
                }

                foreach (var diagnostic in
                         result.RuntimeDiagnostics)
                {
                    Console.WriteLine(
                        "  Runtime: " +
                        diagnostic);
                }

                if (!string.IsNullOrEmpty(
                        result.Exception))
                {
                    Console.WriteLine(
                        "  Exception: " +
                        result.Exception);
                }
            }
        }

        private static void WriteMissingDependencies(
            IReadOnlyList<WildsQslCertificationResult> results)
        {
            Console.WriteLine();
            Console.WriteLine("Missing consumer dependencies:");
            foreach (var result in results)
            {
                if (result.Status != WildsQslCertificationStatus.MissingDependency)
                {
                    continue;
                }
                Console.WriteLine();
                Console.WriteLine("[" + result.Category + "] " + result.Statement);
                Console.WriteLine("  Dependency: " + (result.Dependency ?? "<unspecified>"));
                Console.WriteLine("  Moment outcome: " + result.MomentOutcome);
                foreach (var diagnostic in result.RuntimeDiagnostics)
                {
                    Console.WriteLine("  Runtime: " + diagnostic);
                }
            }
        }

        private static void WriteReport(
            WildsQslCertificationReport report)
        {
            var reportPath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Certification",
                    ReportFileName);

            try
            {
                var reportDirectory = Path.GetDirectoryName(reportPath);
                if (!string.IsNullOrEmpty(reportDirectory))
                {
                    Directory.CreateDirectory(reportDirectory);
                }
                var reportJson =
                    JsonSerializer.Serialize(
                        report,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Converters = { new JsonStringEnumConverter() }
                        });

                File.WriteAllText(
                    reportPath,
                    reportJson);

                TestLog.State(
                    "Certification report: " +
                    reportPath);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Certification report could not be written.",
                    exception);
            }
        }
        
        private static WildsQslCertificationStatus ClassifyResult(
            WildsQslCertificationTest test,
            bool behavioralPass,
            bool dependencyMissing)
        {
            if (!string.IsNullOrWhiteSpace(test.Dependency) && dependencyMissing)
            {
                return WildsQslCertificationStatus.MissingDependency;
            }
            return behavioralPass
                ? WildsQslCertificationStatus.Passed
                : WildsQslCertificationStatus.Failed;
        }

        private static bool IsDeclaredDependencyMissing(
            WildsQslCertificationTest test,
            IReadOnlyList<string> runtimeDiagnostics)
        {
            if (string.IsNullOrWhiteSpace(test.Dependency))
            {
                return false;
            }
            var expected = "Unresolved identifier: " + test.Dependency;
            foreach (var diagnostic in runtimeDiagnostics)
            {
                if (diagnostic.Contains(expected, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        private static string GetCertificationStatus(
            WildsQslCertificationReport report)
        {
            if (report.IsCertified)
            {
                return "CERTIFIED";
            }
            if (report.Failed == 0 && report.MissingDependencies > 0)
            {
                return "INCOMPLETE - CONSUMER DEPENDENCIES MISSING";
            }
            return "NOT CERTIFIED";
        }

        private static void Assert(
            bool condition,
            string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(
                    message);
            }
        }
    }
}