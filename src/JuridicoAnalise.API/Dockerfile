FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/JuridicoAnalise.API/JuridicoAnalise.API.csproj", "JuridicoAnalise.API/"]
COPY ["src/JuridicoAnalise.Application/JuridicoAnalise.Application.csproj", "JuridicoAnalise.Application/"]
COPY ["src/JuridicoAnalise.Domain/JuridicoAnalise.Domain.csproj", "JuridicoAnalise.Domain/"]
COPY ["src/JuridicoAnalise.Infrastructure/JuridicoAnalise.Infrastructure.csproj", "JuridicoAnalise.Infrastructure/"]
RUN dotnet restore "JuridicoAnalise.API/JuridicoAnalise.API.csproj"
COPY src/ .
WORKDIR "/src/JuridicoAnalise.API"
RUN dotnet build "JuridicoAnalise.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JuridicoAnalise.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JuridicoAnalise.API.dll"]
