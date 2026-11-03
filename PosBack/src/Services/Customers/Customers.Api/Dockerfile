FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Services/Customers/Customers.Api/Customers.Api.csproj ./
COPY src/BuildingBlocks/SharedKernel/SharedKernel.csproj /src/../../../BuildingBlocks/SharedKernel/
RUN dotnet restore
COPY src/Services/Customers/Customers.Api/ ./
COPY src/BuildingBlocks/SharedKernel/ /BuildingBlocks/SharedKernel/
RUN dotnet publish Customers.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Customers.Api.dll"]
