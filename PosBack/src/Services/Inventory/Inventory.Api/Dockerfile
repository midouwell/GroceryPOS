FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Services/Inventory/Inventory.Api/Inventory.Api.csproj ./
COPY src/BuildingBlocks/SharedKernel/SharedKernel.csproj /src/../../../BuildingBlocks/SharedKernel/
RUN dotnet restore
COPY src/Services/Inventory/Inventory.Api/ ./
COPY src/BuildingBlocks/SharedKernel/ /BuildingBlocks/SharedKernel/
RUN dotnet publish Inventory.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Inventory.Api.dll"]
