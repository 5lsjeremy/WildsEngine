using System;
using QSL.Core;

namespace WildsEngine.Qsl
{
    /// <summary>
    /// Represents an unsuccessful QSL processing result
    /// encountered while resolving a Wilds WorkUnit.
    ///
    /// Throwing this exception allows WildsEngine to record
    /// the QSL failure as a failed WorkUnit, failed Pulse,
    /// and failed Moment.
    /// </summary>
    public sealed class WildsQslExecutionException :
        Exception
    {
        public WildsQslExecutionException(
            QslProcessingResult result)
            : base(
                CreateMessage(
                    result),
                result != null
                    ? result.Exception
                    : null)
        {
            Result = result ??
                throw new ArgumentNullException(
                    nameof(result));
        }

        public QslProcessingResult Result { get; }

        private static string CreateMessage(
            QslProcessingResult result)
        {
            if (result == null)
            {
                return "QSL processing failed without a result.";
            }

            if (result.HasException &&
                result.Exception != null)
            {
                return
                    "QSL processing raised an exception: " +
                    result.Exception.Message;
            }

            if (result.ParseFailed)
            {
                return
                    "QSL parsing failed with " +
                    result.ParserDiagnostics.Count +
                    " diagnostic(s).";
            }

            if (result.EvaluationFailed)
            {
                return
                    "QSL evaluation failed with " +
                    result.RuntimeDiagnostics.Count +
                    " runtime diagnostic(s).";
            }

            return "QSL processing did not succeed.";
        }
    }
}
