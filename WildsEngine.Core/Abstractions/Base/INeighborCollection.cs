using System;
using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    public interface INeighborCollection
    {
        int Count { get; }

        IReadOnlyList<INeighborRelationship> Relationships { get; }

        bool Add(
            IBaseObject neighbor,
            string membership,
            double distance);

        bool Contains(
            Guid neighborId,
            string membership);

        bool TryGet(
            Guid neighborId,
            string membership,
            out INeighborRelationship? relationship);

        bool SetDistance(
            Guid neighborId,
            string membership,
            double distance);

        bool UpdateDistance(
            Guid neighborId,
            string membership,
            Func<double, double> update);

        bool Remove(
            Guid neighborId,
            string membership);

        int Remove(
            Guid neighborId);

        void Clear();
    }
}