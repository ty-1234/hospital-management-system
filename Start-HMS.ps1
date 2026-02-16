param(
    [int]$HttpsPort = 7099,
    [int]$HttpPort = 5099
)

$ports = @($HttpsPort, $HttpPort)

foreach ($port in $ports) {
    $pids = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty OwningProcess -Unique

    foreach ($processId in $pids) {
        try {
            Stop-Process -Id $processId -Force -ErrorAction Stop
            Write-Host "Stopped process $processId using port $port."
        }
        catch {
            Write-Host ("Could not stop process {0} on port {1}: {2}" -f $processId, $port, $_.Exception.Message)
        }
    }
}

dotnet run --project "HospitalManagementSystem.Web/HospitalManagementSystem.Web.csproj" --urls "https://localhost:$HttpsPort;http://localhost:$HttpPort"
