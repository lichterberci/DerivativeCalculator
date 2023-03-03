#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DerivativeCalculatorAPI/DerivativeCalculatorAPI.csproj", "DerivativeCalculatorAPI/"]
COPY ["DerivativeCalculator/DerivativeCalculator.csproj", "DerivativeCalculator/"]
RUN dotnet restore "DerivativeCalculatorAPI/DerivativeCalculatorAPI.csproj"
COPY . .
WORKDIR "/src/DerivativeCalculatorAPI"
RUN dotnet build "DerivativeCalculatorAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DerivativeCalculatorAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DerivativeCalculatorAPI.dll"]