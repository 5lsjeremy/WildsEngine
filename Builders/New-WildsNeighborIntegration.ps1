param([string]$Root,[switch]$Force)
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($Root)) { $Root=(Get-Location).Path }
if (Test-Path (Join-Path $Root "WildsEngine.Core.csproj")) { $CoreRoot=$Root }
elseif (Test-Path (Join-Path $Root "WildsEngine.Core\WildsEngine.Core.csproj")) { $CoreRoot=Join-Path $Root "WildsEngine.Core" }
else { throw "Run from solution root or WildsEngine.Core, or pass -Root." }
function Put([string]$Rel,[string]$Text){$p=Join-Path $CoreRoot $Rel;$d=Split-Path $p -Parent;New-Item -ItemType Directory -Force -Path $d|Out-Null;if((Test-Path $p)-and-not $Force){Write-Host "[SKIP] $Rel" -ForegroundColor Yellow;return};Set-Content $p $Text -Encoding UTF8;Write-Host "[WRITE] $Rel" -ForegroundColor Green}

Put "Processing\Instructions\AddNeighborInstruction.cs" @'
using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Topology;

namespace WildsEngine.Core.Processing
{
    public sealed class AddNeighborInstruction : StateInstructionBase
    {
        public const string IntentName = "topology.neighbor.add";

        public AddNeighborInstruction(
            IBaseObject neighbor,
            string membership,
            double distance,
            IDictionary<string, object?>? metadata = null)
            : base(
                IntentName,
                ValidateNeighbor(neighbor),
                CreateArguments(membership, distance),
                metadata)
        {
            Neighbor = neighbor;
            Membership = ValidateMembership(membership);
            Distance = NeighborDistance.Validate(distance, nameof(distance));
        }

        public IBaseObject Neighbor { get; }
        public string Membership { get; }
        public double Distance { get; }

        private static IBaseObject ValidateNeighbor(IBaseObject neighbor) =>
            neighbor ?? throw new ArgumentNullException(nameof(neighbor));

        private static string ValidateMembership(string membership)
        {
            if (string.IsNullOrWhiteSpace(membership))
                throw new ArgumentException("Neighbor membership cannot be empty.", nameof(membership));
            return membership.Trim();
        }

        private static IDictionary<string, object?> CreateArguments(string membership, double distance)
        {
            return new Dictionary<string, object?>
            {
                ["membership"] = ValidateMembership(membership),
                ["distance"] = NeighborDistance.Validate(distance, nameof(distance))
            };
        }
    }
}
'@

Put "Processing\Instructions\RemoveNeighborInstruction.cs" @'
using System;
using System.Collections.Generic;

namespace WildsEngine.Core.Processing
{
    public sealed class RemoveNeighborInstruction : StateInstructionBase
    {
        public const string IntentName = "topology.neighbor.remove";

        public RemoveNeighborInstruction(
            Guid neighborId,
            string membership,
            IDictionary<string, object?>? metadata = null)
            : base(IntentName, ValidateId(neighborId), CreateArguments(membership), metadata)
        {
            NeighborId = neighborId;
            Membership = ValidateMembership(membership);
        }

        public Guid NeighborId { get; }
        public string Membership { get; }

        private static Guid ValidateId(Guid neighborId)
        {
            if (neighborId == Guid.Empty)
                throw new ArgumentException("Neighbor removal requires a non-empty GUID.", nameof(neighborId));
            return neighborId;
        }

        private static string ValidateMembership(string membership)
        {
            if (string.IsNullOrWhiteSpace(membership))
                throw new ArgumentException("Neighbor membership cannot be empty.", nameof(membership));
            return membership.Trim();
        }

        private static IDictionary<string, object?> CreateArguments(string membership) =>
            new Dictionary<string, object?> { ["membership"] = ValidateMembership(membership) };
    }
}
'@

Put "Processing\Instructions\SetNeighborDistanceInstruction.cs" @'
using System;
using System.Collections.Generic;
using WildsEngine.Core.Topology;

namespace WildsEngine.Core.Processing
{
    public sealed class SetNeighborDistanceInstruction : StateInstructionBase
    {
        public const string IntentName = "topology.neighbor.distance.set";

        public SetNeighborDistanceInstruction(
            Guid neighborId,
            string membership,
            double distance,
            IDictionary<string, object?>? metadata = null)
            : base(IntentName, ValidateId(neighborId), CreateArguments(membership, distance), metadata)
        {
            NeighborId = neighborId;
            Membership = ValidateMembership(membership);
            Distance = NeighborDistance.Validate(distance, nameof(distance));
        }

        public Guid NeighborId { get; }
        public string Membership { get; }
        public double Distance { get; }

        private static Guid ValidateId(Guid value)
        {
            if (value == Guid.Empty) throw new ArgumentException("Neighbor distance update requires a non-empty GUID.", nameof(value));
            return value;
        }

        private static string ValidateMembership(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Neighbor membership cannot be empty.", nameof(value));
            return value.Trim();
        }

        private static IDictionary<string, object?> CreateArguments(string membership, double distance) =>
            new Dictionary<string, object?>
            {
                ["membership"] = ValidateMembership(membership),
                ["distance"] = NeighborDistance.Validate(distance, nameof(distance))
            };
    }
}
'@

Write-Host "Neighbor instruction integration generated." -ForegroundColor Cyan
Write-Host "Next: dotnet build" -ForegroundColor Cyan
