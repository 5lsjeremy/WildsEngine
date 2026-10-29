using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Units
{
    /// <summary>
    /// Maintains an ordered collection of completed moments.
    ///
    /// Timeline order reflects the order in which moments
    /// are recorded. It does not depend on system clock time.
    /// </summary>
    public sealed class Timeline :
        BaseUnit,
        ITimeline
    {
        private readonly object _sync =
            new object();

        private readonly List<IMoment> _moments =
            new List<IMoment>();

        private readonly HashSet<Guid> _momentIds =
            new HashSet<Guid>();

        public Timeline()
            : base()
        {
        }

        public Timeline(
            Guid id)
            : base(
                id)
        {
        }

        public int Count
        {
            get
            {
                lock (_sync)
                {
                    return _moments.Count;
                }
            }
        }

        public IReadOnlyList<IMoment> Moments
        {
            get
            {
                lock (_sync)
                {
                    return _moments.ToArray();
                }
            }
        }

        public bool AddMoment(
            IMoment moment)
        {
            if (moment == null)
            {
                throw new ArgumentNullException(
                    nameof(moment));
            }

            lock (_sync)
            {
                if (!_momentIds.Add(
                        moment.Id))
                {
                    return false;
                }

                _moments.Add(
                    moment);

                return true;
            }
        }
    }
}
