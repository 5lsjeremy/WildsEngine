using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Units
{
    /// <summary>
    /// Maintains a collection of Wilds timelines.
    ///
    /// An epoch is the highest currently defined Wilds
    /// temporal unit.
    /// </summary>
    public sealed class Epoch :
        BaseUnit,
        IEpoch
    {
        private readonly object _sync =
            new object();

        private readonly List<ITimeline> _timelines =
            new List<ITimeline>();

        private readonly HashSet<Guid> _timelineIds =
            new HashSet<Guid>();

        public Epoch()
            : base()
        {
        }

        public Epoch(
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
                    return _timelines.Count;
                }
            }
        }

        public IReadOnlyList<ITimeline> Timelines
        {
            get
            {
                lock (_sync)
                {
                    return _timelines.ToArray();
                }
            }
        }

        public bool AddTimeline(
            ITimeline timeline)
        {
            if (timeline == null)
            {
                throw new ArgumentNullException(
                    nameof(timeline));
            }

            lock (_sync)
            {
                if (!_timelineIds.Add(
                        timeline.Id))
                {
                    return false;
                }

                _timelines.Add(
                    timeline);

                return true;
            }
        }
    }
}
