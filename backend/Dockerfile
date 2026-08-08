# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY CicdPractice.Api/CicdPractice.Api.csproj CicdPractice.Api/
RUN dotnet restore CicdPractice.Api/CicdPractice.Api.csproj

COPY CicdPractice.Api/ CicdPractice.Api/
RUN dotnet publish CicdPractice.Api/CicdPractice.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "CicdPractice.Api.dll"]
