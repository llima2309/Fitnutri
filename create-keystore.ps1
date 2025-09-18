# Script para criar keystore para assinatura do Android APK
# Execute este script antes de fazer o build Release

$keystorePath = "AppFitNutri\fitnutri.keystore"
$keyAlias = "fitnutrikey"
$keystorePassword = "fitnutri123"
$keyPassword = "fitnutri123"

Write-Host "Criando keystore para assinatura do APK..." -ForegroundColor Green

# Verifica se o Java está instalado
try {
    $javaVersion = java -version 2>&1
    Write-Host "Java encontrado: $($javaVersion[0])" -ForegroundColor Green
} catch {
    Write-Host "ERRO: Java não encontrado. Instale o JDK para continuar." -ForegroundColor Red
    Write-Host "Download: https://adoptium.net/" -ForegroundColor Yellow
    exit 1
}

# Remove keystore existente se houver
if (Test-Path $keystorePath) {
    Write-Host "Removendo keystore existente..." -ForegroundColor Yellow
    Remove-Item $keystorePath -Force
}

# Cria novo keystore
Write-Host "Criando novo keystore..." -ForegroundColor Green

$keytoolCommand = @"
keytool -genkey -v -keystore "$keystorePath" -alias $keyAlias -keyalg RSA -keysize 2048 -validity 10000 -storepass $keystorePassword -keypass $keyPassword -dname "CN=FitNutri, OU=Development, O=FitNutri, L=City, S=State, C=BR"
"@

try {
    Invoke-Expression $keytoolCommand
    Write-Host "Keystore criado com sucesso!" -ForegroundColor Green
    
    # Atualiza o arquivo .csproj com as senhas
    $csprojPath = "AppFitNutri\AppFitNutri.csproj"
    $content = Get-Content $csprojPath -Raw
    
    # Substitui as senhas vazias pelas senhas reais
    $content = $content -replace '<AndroidSigningKeyPass></AndroidSigningKeyPass>', "<AndroidSigningKeyPass>$keyPassword</AndroidSigningKeyPass>"
    $content = $content -replace '<AndroidSigningStorePass></AndroidSigningStorePass>', "<AndroidSigningStorePass>$keystorePassword</AndroidSigningStorePass>"
    
    Set-Content $csprojPath -Value $content
    
    Write-Host "Configuração do projeto atualizada!" -ForegroundColor Green
    Write-Host ""
    Write-Host "INFORMAÇÕES DO KEYSTORE:" -ForegroundColor Cyan
    Write-Host "Arquivo: $keystorePath" -ForegroundColor White
    Write-Host "Alias: $keyAlias" -ForegroundColor White
    Write-Host "Senha do Keystore: $keystorePassword" -ForegroundColor White
    Write-Host "Senha da Chave: $keyPassword" -ForegroundColor White
    Write-Host ""
    Write-Host "IMPORTANTE: Guarde essas informações em local seguro!" -ForegroundColor Red
    Write-Host "Você precisará das mesmas senhas para futuras atualizações do app." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Agora você pode fazer o build Release do projeto:" -ForegroundColor Green
    Write-Host "dotnet build -c Release -f net10.0-android" -ForegroundColor White
    
} catch {
    Write-Host "ERRO ao criar keystore: $_" -ForegroundColor Red
    exit 1
}