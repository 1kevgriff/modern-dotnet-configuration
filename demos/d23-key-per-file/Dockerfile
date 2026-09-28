# Build and run the demo in a container so the mount really is /run/secrets.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish D23.KeyPerFile.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .
ENV SECRETS_PATH=/run/secrets
ENTRYPOINT ["dotnet", "D23.KeyPerFile.dll"]
