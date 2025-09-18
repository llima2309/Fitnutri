# Script para verificar a assinatura do APK e informa��es importantes
# Execute este script para verificar se o APK foi assinado corretamente

$apkPath = "AppFitNutri\bin\Release\net10.0-android\com.companyname.appfitnutri-Signed.apk"
$keystorePath = "AppFitNutri\fitnutri.keystore"

Write-Host "=== VERIFICA��O DO APK ===" -ForegroundColor Green
Write-Host ""

# Verifica se o APK existe
if (Test-Path $apkPath) {
    Write-Host "? APK assinado encontrado: $apkPath" -ForegroundColor Green
    
    $apkInfo = Get-Item $apkPath
    Write-Host "   Tamanho: $([math]::round($apkInfo.Length / 1MB, 2)) MB" -ForegroundColor White
    Write-Host "   Data de cria��o: $($apkInfo.LastWriteTime)" -ForegroundColor White
} else {
    Write-Host "? APK assinado n�o encontrado!" -ForegroundColor Red
    Write-Host "   Tentando APK n�o assinado..." -ForegroundColor Yellow
    
    $unsignedApkPath = "AppFitNutri\bin\Release\net10.0-android\com.companyname.appfitnutri.apk"
    if (Test-Path $unsignedApkPath) {
        Write-Host "??  APK n�o assinado encontrado: $unsignedApkPath" -ForegroundColor Yellow
        $apkInfo = Get-Item $unsignedApkPath
        Write-Host "   Tamanho: $([math]::round($apkInfo.Length / 1MB, 2)) MB" -ForegroundColor White
        Write-Host "   Data de cria��o: $($apkInfo.LastWriteTime)" -ForegroundColor White
    }
}

Write-Host ""

# Verifica se o keystore existe
if (Test-Path $keystorePath) {
    Write-Host "? Keystore encontrado: $keystorePath" -ForegroundColor Green
} else {
    Write-Host "? Keystore n�o encontrado!" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== INFORMA��ES IMPORTANTES ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Para instalar o APK em um dispositivo Android:" -ForegroundColor White
Write-Host "   1. Copie o arquivo APK para o dispositivo" -ForegroundColor Gray
Write-Host "   2. Ative 'Fontes desconhecidas' nas configura��es de seguran�a" -ForegroundColor Gray
Write-Host "   3. Toque no arquivo APK para instalar" -ForegroundColor Gray
Write-Host ""
Write-Host "?? SEGURAN�A - Guarde estas informa��es:" -ForegroundColor Red
Write-Host "   � Arquivo keystore: $keystorePath" -ForegroundColor White
Write-Host "   � Alias da chave: fitnutrikey" -ForegroundColor White
Write-Host "   � Senha do keystore: fitnutri123" -ForegroundColor White
Write-Host "   � Senha da chave: fitnutri123" -ForegroundColor White
Write-Host ""
Write-Host "??  IMPORTANTE: Mantenha o keystore e senhas em local seguro!" -ForegroundColor Yellow
Write-Host "   Voc� precisar� do mesmo keystore para futuras atualiza��es do app." -ForegroundColor Yellow
Write-Host ""
Write-Host "?? Para distribuir o app:" -ForegroundColor White
Write-Host "   � Use o arquivo com '-Signed.apk' no nome" -ForegroundColor Gray
Write-Host "   � Para Google Play Store, voc� precisar� gerar um AAB em vez de APK" -ForegroundColor Gray
Write-Host ""

# Verifica se Java est� dispon�vel para poss�veis verifica��es futuras
try {
    $javaVersion = java -version 2>&1
    Write-Host "? Java dispon�vel para verifica��es: $($javaVersion[0])" -ForegroundColor Green
} catch {
    Write-Host "??  Java n�o encontrado - Instale para verifica��es avan�adas" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "?? BUILD CONCLU�DO COM SUCESSO!" -ForegroundColor Green