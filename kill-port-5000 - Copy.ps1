# Script to find and kill process using port 5000

Write-Host "Checking for processes using port 5000..." -ForegroundColor Yellow

# Find process using port 5000
$connection = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue

if ($connection) {
    $pid = $connection.OwningProcess
    $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
    
    if ($process) {
        Write-Host "Found process using port 5000:" -ForegroundColor Red
        Write-Host "  Process Name: $($process.Name)" -ForegroundColor Red
        Write-Host "  Process ID: $pid" -ForegroundColor Red
        Write-Host "  Path: $($process.Path)" -ForegroundColor Red
        
        $confirm = Read-Host "Kill this process? (Y/N)"
        if ($confirm -eq 'Y' -or $confirm -eq 'y') {
            Stop-Process -Id $pid -Force
            Write-Host "Process killed successfully!" -ForegroundColor Green
        } else {
            Write-Host "Process not killed." -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "No process found using port 5000." -ForegroundColor Green
    Write-Host ""
    Write-Host "Checking if port is reserved by Windows..." -ForegroundColor Yellow
    
    # Check for port reservations
    $reservations = netsh interface ipv4 show excludedportrange protocol=tcp | Select-String "5000"
    if ($reservations) {
        Write-Host "Port 5000 may be in an excluded range. Check output:" -ForegroundColor Yellow
        netsh interface ipv4 show excludedportrange protocol=tcp | Select-String -Pattern "5000" -Context 2,2
    } else {
        Write-Host "Port 5000 is not reserved." -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "You can now try running: docker-compose up" -ForegroundColor Cyan
