# ===== STAGE 1: build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia sln e csprojs para cache eficiente de restore
COPY *.sln ./
COPY Fitnutri/*.csproj ./Fitnutri/
COPY Fitnutri.test/*.csproj ./Fitnutri.test/
RUN dotnet restore

# Copia o restante do código e publica
COPY . .
RUN dotnet publish Fitnutri/Fitnutri.csproj -c Release -o /app/publish /p:UseAppHost=false

# ===== STAGE 2: runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# A API vai ouvir na porta 8080 dentro do container
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
# (opcional) endurecimento leve p/ container
ENV DOTNET_EnableDiagnostics=0

EXPOSE 8080
COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "Fitnutri.dll"]
