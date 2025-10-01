# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/fftivc.utility.modloader/*" -Force -Recurse
dotnet publish "./fftivc.utility.modloader.csproj" -c Release -o "$env:RELOADEDIIMODS/fftivc.utility.modloader" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location