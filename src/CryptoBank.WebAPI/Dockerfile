FROM mcr.microsoft.com/dotnet/sdk:7.0 AS restore

WORKDIR /src

COPY ["CryptoBank.WebAPI/CryptoBank.WebAPI.csproj", "CryptoBank.WebAPI/"]
COPY ["CryptoBank.Common/CryptoBank.Common.csproj", "CryptoBank.Common/"]
COPY ["CryptoBank.Database/CryptoBank.Database.csproj", "CryptoBank.Database/"]
COPY ["CryptoBank.Domain/CryptoBank.Domain.csproj", "CryptoBank.Domain/"]

RUN dotnet restore "CryptoBank.WebAPI/CryptoBank.WebAPI.csproj"

FROM restore AS publish

COPY . .

WORKDIR "/src/CryptoBank.WebAPI"

RUN dotnet publish "CryptoBank.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final

WORKDIR /app

EXPOSE 80

COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT Production
ENV ASPNETCORE_URLS http://*:80

ENTRYPOINT ["dotnet", "CryptoBank.WebAPI.dll"]
