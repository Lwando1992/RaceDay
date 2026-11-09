# RaceDay.Web (MVC client)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY RaceDay.Contracts/RaceDay.Contracts.csproj RaceDay.Contracts/
COPY RaceDay.Web/RaceDay.Web.csproj RaceDay.Web/
RUN dotnet restore RaceDay.Web/RaceDay.Web.csproj

COPY RaceDay.Contracts/ RaceDay.Contracts/
COPY RaceDay.Web/ RaceDay.Web/
RUN dotnet publish RaceDay.Web/RaceDay.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]
