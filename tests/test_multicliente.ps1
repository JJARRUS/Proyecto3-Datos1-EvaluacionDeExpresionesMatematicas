# US7 Task 40: Script de prueba multicliente
# Prueba que el servidor responde correctamente sin interferencia

param(
    [string]$ServerHost = "127.0.0.1",
    [int]$ServerPort = 5000,
    [int]$ClientCount = 3
)

Write-Host "================================" -ForegroundColor Cyan
Write-Host "US7 Task 40: Prueba Multicliente" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

function Test-ClientConnection {
    param([int]$Id, [string]$Host, [int]$Port)
    
    $name = "Cliente-$Id"
    Write-Host "[$name] Conectando..." -ForegroundColor Yellow
    
    try {
        $client = New-Object System.Net.Sockets.TcpClient
        $client.Connect($Host, $Port)
        
        $stream = $client.GetStream()
        $writer = New-Object System.IO.StreamWriter($stream, [Text.UTF8Encoding]::UTF8)
        $writer.AutoFlush = $true
        $reader = New-Object System.IO.StreamReader($stream, [Text.UTF8Encoding]::UTF8)
        
        Write-Host "[$name] OK Conectado" -ForegroundColor Green
        
        $expressions = @("2 3 +", "10 5 -", "4 2 *")
        $expr = $expressions[$Id % 3]
        
        Write-Host "[$name] EVAL $expr" -ForegroundColor Cyan
        $writer.WriteLine("EVAL $expr")
        
        $response = $reader.ReadLine()
        Write-Host "[$name] Resp: $response" -ForegroundColor Green
        
        if (-not ($response -like "OK*")) {
            Write-Host "[$name] X EVAL fallo" -ForegroundColor Red
            $client.Close()
            return $false
        }
        
        Start-Sleep -Milliseconds 100
        
        Write-Host "[$name] HIST..." -ForegroundColor Cyan
        $writer.WriteLine("HIST")
        
        $count = 0
        do {
            $line = $reader.ReadLine()
            if ($line -ne "END" -and $null -ne $line) {
                $count++
            }
        } while ($line -ne "END")
        
        Write-Host "[$name] OK Historial: $count lineas" -ForegroundColor Green
        $client.Close()
        return $true
    }
    catch {
        Write-Host "[$name] X Error: $_" -ForegroundColor Red
        return $false
    }
}

Write-Host "Lanzando $ClientCount clientes..." -ForegroundColor Yellow
Write-Host ""

$jobs = @()
for ($i = 0; $i -lt $ClientCount; $i++) {
    $job = Start-Job -ScriptBlock ${function:Test-ClientConnection} -ArgumentList @($i, $ServerHost, $ServerPort)
    $jobs += $job
    Start-Sleep -Milliseconds 20
}

$results = @()
foreach ($job in $jobs) {
    $result = $job | Wait-Job | Receive-Job
    $results += $result
    Remove-Job $job
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
$success = ($results | Measure-Object -Sum).Sum
Write-Host "Resultado: $success / $ClientCount OK" -ForegroundColor $(if ($success -eq $ClientCount) { "Green" } else { "Red" })
Write-Host "================================" -ForegroundColor Cyan
