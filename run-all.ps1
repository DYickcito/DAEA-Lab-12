# Inicia los tres proyectos en ventanas separadas (desarrollo local).
# Uso:  .\run-all.ps1
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Start-Process powershell -ArgumentList @(
    "-NoExit", "-Command",
    "Set-Location '$root'; dotnet run --project 'DAEA Lab 12'"
)

Start-Process powershell -ArgumentList @(
    "-NoExit", "-Command",
    "Set-Location '$root'; dotnet run --project 'CitasApi'"
)

Start-Process powershell -ArgumentList @(
    "-NoExit", "-Command",
    "Set-Location '$root'; dotnet run --project 'GatewayApi'"
)

Write-Host "Servicios iniciados: Uma Salud (5120), CitasApi (5121) y GatewayApi (5000)."
Write-Host "Prueba todo por http://localhost:5000 con Gateway.http."
