using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Motion
{
    /// <summary>
    /// Represents one scoped, ordered history of immutable
    /// Discovery-vector evaluations.
    ///
    /// The evaluation series is also the authoritative
    /// Discovery-vector series for the same history.
    ///
    /// Evaluation consumers retain access to evaluation identity
    /// and timestamps, while Truth and other vector-series
    /// consumers receive the exact vectors carried by those
    /// evaluations.
    ///
    /// No second temporal ownership model is introduced.
    /// </summary>
    public sealed class DiscoveryVectorEvaluationSeries :
        IDiscoveryVectorEvaluationSeries,
        IDiscoveryVectorSeries
    {
        private readonly IReadOnlyList<IDiscoveryVector>
            _vectors;

        public DiscoveryVectorEvaluationSeries(
            string scope,
            IReadOnlyList<IDiscoveryVectorInput> inputs,
            IReadOnlyList<IDiscoveryVectorEvaluation> evaluations)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException(
                    "Series requires a scope.",
                    nameof(scope));
            }

            if (inputs == null)
            {
                throw new ArgumentNullException(
                    nameof(inputs));
            }

            if (evaluations == null)
            {
                throw new ArgumentNullException(
                    nameof(evaluations));
            }

            if (inputs.Count == 0)
            {
                throw new ArgumentException(
                    "Series requires at least one input.",
                    nameof(inputs));
            }

            if (evaluations.Count == 0)
            {
                throw new ArgumentException(
                    "Series requires at least one evaluation.",
                    nameof(evaluations));
            }

            var inputCopy =
                new IDiscoveryVectorInput[
                    inputs.Count];

            for (var index = 0;
                 index < inputs.Count;
                 index++)
            {
                inputCopy[index] =
                    inputs[index] ??
                    throw new ArgumentException(
                        "Inputs cannot contain null.",
                        nameof(inputs));
            }

            var evaluationCopy =
                new IDiscoveryVectorEvaluation[
                    evaluations.Count];

            var vectorCopy =
                new IDiscoveryVector[
                    evaluations.Count];

            DateTimeOffset? prior = null;

            for (var index = 0;
                 index < evaluations.Count;
                 index++)
            {
                var evaluation =
                    evaluations[index] ??
                    throw new ArgumentException(
                        "Evaluations cannot contain null.",
                        nameof(evaluations));

                if (prior.HasValue &&
                    evaluation.EvaluatedAt <= prior.Value)
                {
                    throw new ArgumentException(
                        "Evaluation timestamps must be strictly increasing.",
                        nameof(evaluations));
                }

                ValidateInputs(
                    inputCopy,
                    evaluation);

                evaluationCopy[index] =
                    evaluation;

                vectorCopy[index] =
                    evaluation.Vector ??
                    throw new ArgumentException(
                        "Evaluations cannot contain a null vector.",
                        nameof(evaluations));

                prior =
                    evaluation.EvaluatedAt;
            }

            Scope =
                scope.Trim();

            Inputs =
                new ReadOnlyCollection<IDiscoveryVectorInput>(
                    inputCopy);

            Evaluations =
                new ReadOnlyCollection<IDiscoveryVectorEvaluation>(
                    evaluationCopy);

            _vectors =
                new ReadOnlyCollection<IDiscoveryVector>(
                    vectorCopy);
        }

        public string Scope { get; }

        public IReadOnlyList<IDiscoveryVectorInput>
            Inputs { get; }

        public IReadOnlyList<IDiscoveryVectorEvaluation>
            Evaluations { get; }

        public IDiscoveryVectorEvaluation? Current =>
            Evaluations.Count == 0
                ? null
                : Evaluations[
                    Evaluations.Count - 1];

        public IDiscoveryVectorEvaluation? Previous =>
            Evaluations.Count < 2
                ? null
                : Evaluations[
                    Evaluations.Count - 2];

        IReadOnlyList<IDiscoveryVector>
            IDiscoveryVectorSeries.Vectors =>
                _vectors;

        IDiscoveryVector?
            IDiscoveryVectorSeries.Current =>
                Current?.Vector;

        IDiscoveryVector?
            IDiscoveryVectorSeries.Previous =>
                Previous?.Vector;

        private static void ValidateInputs(
            IReadOnlyList<IDiscoveryVectorInput> inputs,
            IDiscoveryVectorEvaluation evaluation)
        {
            for (var contributionIndex = 0;
                 contributionIndex <
                 evaluation.Vector.Contributions.Count;
                 contributionIndex++)
            {
                var candidate =
                    evaluation.Vector
                        .Contributions[
                            contributionIndex]
                        .Input;

                var found =
                    false;

                for (var inputIndex = 0;
                     inputIndex < inputs.Count;
                     inputIndex++)
                {
                    if (ReferenceEquals(
                            candidate,
                            inputs[inputIndex]))
                    {
                        found =
                            true;

                        break;
                    }
                }

                if (!found)
                {
                    throw new ArgumentException(
                        "Evaluation contains a contribution " +
                        "from outside the series input population.");
                }
            }
        }
    }
}