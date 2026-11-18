using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Motion
{
    public sealed class DiscoveryVectorEvaluationSeries : IDiscoveryVectorEvaluationSeries
    {
        public DiscoveryVectorEvaluationSeries(
            string scope,
            IReadOnlyList<IDiscoveryVectorInput> inputs,
            IReadOnlyList<IDiscoveryVectorEvaluation> evaluations)
        {
            if (string.IsNullOrWhiteSpace(scope)) throw new ArgumentException("Series requires a scope.", nameof(scope));
            if (inputs == null) throw new ArgumentNullException(nameof(inputs));
            if (evaluations == null) throw new ArgumentNullException(nameof(evaluations));
            if (inputs.Count == 0) throw new ArgumentException("Series requires at least one input.", nameof(inputs));
            if (evaluations.Count == 0) throw new ArgumentException("Series requires at least one evaluation.", nameof(evaluations));
            var inputCopy = new IDiscoveryVectorInput[inputs.Count];
            for (var i = 0; i < inputs.Count; i++) inputCopy[i] = inputs[i] ?? throw new ArgumentException("Inputs cannot contain null.", nameof(inputs));
            var evalCopy = new IDiscoveryVectorEvaluation[evaluations.Count];
            DateTimeOffset? prior = null;
            for (var i = 0; i < evaluations.Count; i++)
            {
                var evaluation = evaluations[i] ?? throw new ArgumentException("Evaluations cannot contain null.", nameof(evaluations));
                if (prior.HasValue && evaluation.EvaluatedAt <= prior.Value)
                    throw new ArgumentException("Evaluation timestamps must be strictly increasing.", nameof(evaluations));
                ValidateInputs(inputCopy, evaluation);
                evalCopy[i] = evaluation; prior = evaluation.EvaluatedAt;
            }
            Scope = scope.Trim();
            Inputs = new ReadOnlyCollection<IDiscoveryVectorInput>(inputCopy);
            Evaluations = new ReadOnlyCollection<IDiscoveryVectorEvaluation>(evalCopy);
        }
        public string Scope { get; }
        public IReadOnlyList<IDiscoveryVectorInput> Inputs { get; }
        public IReadOnlyList<IDiscoveryVectorEvaluation> Evaluations { get; }
        public IDiscoveryVectorEvaluation? Current => Evaluations.Count == 0 ? null : Evaluations[Evaluations.Count - 1];
        public IDiscoveryVectorEvaluation? Previous => Evaluations.Count < 2 ? null : Evaluations[Evaluations.Count - 2];

        private static void ValidateInputs(
            IReadOnlyList<IDiscoveryVectorInput> inputs,
            IDiscoveryVectorEvaluation evaluation)
        {
            for (var c = 0; c < evaluation.Vector.Contributions.Count; c++)
            {
                var candidate = evaluation.Vector.Contributions[c].Input;
                var found = false;
                for (var i = 0; i < inputs.Count; i++)
                    if (ReferenceEquals(candidate, inputs[i])) { found = true; break; }
                if (!found) throw new ArgumentException("Evaluation contains a contribution from outside the series input population.");
            }
        }
    }
}
