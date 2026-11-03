FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Gateway/Gateway.Api/Gateway.Api.csproj ./
RUN dotnet restore
COPY src/Gateway/Gateway.Api/ ./
RUN dotnet publish Gateway.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
