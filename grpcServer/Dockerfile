FROM mcr.microsoft.com/dotnet/core/sdk:2.2 as sdk
# copy csproj and restore as distinct layers
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet build -c Release
RUN dotnet publish . -c Release -o /published --no-build --no-restore
    
# clean runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=sdk /published ./

EXPOSE 8000

ENTRYPOINT ["dotnet", "grpcServer.dll"]
