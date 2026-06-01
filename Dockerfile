FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ReserveBag.Api.csproj", "./"]
RUN dotnet restore "./ReserveBag.Api.csproj"
COPY . .
RUN dotnet build "ReserveBag.Api.csproj" -c Release -o /app/build
RUN dotnet publish "ReserveBag.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ReserveBag.Api.dll"]