# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
COPY trabajodb.db ./trabajodb.db  

RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build-env /app/out ./
COPY --from=build-env /app/trabajodb.db ./trabajodb.db  

ENV ASPNETCORE_URLS=http://*:10000
CMD ["dotnet", "Lab01-Grupal.dll"]