# E7-H2 Combined Package

Includes H2A immutable evaluation series, H2B finite-difference motion service, H2C explicit momentum curves, certification tests, and one combined runner.

Extract the `E7-H2-Package` folder at the repository root, then run:

```powershell
Unblock-File .\E7-H2-Package\Install-E7H2.ps1
.\E7-H2-Package\Install-E7H2.ps1
Unblock-File .\WildsEngine.Core.Tests\Entities\Runners\Invoke-E7H2.ps1
.\WildsEngine.Core.Tests\Entities\Runners\Invoke-E7H2.ps1
```

The installer refuses to proceed if WildsEngine.Core has changes and never writes production files into Core.
