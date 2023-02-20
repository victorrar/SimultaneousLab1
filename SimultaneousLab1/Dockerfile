FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SimultaneousLab1/SimultaneousLab1.csproj", "SimultaneousLab1/"]
RUN dotnet restore "SimultaneousLab1/SimultaneousLab1.csproj"
COPY . .
WORKDIR "/src/SimultaneousLab1"
RUN dotnet build "SimultaneousLab1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimultaneousLab1.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimultaneousLab1.dll"]
