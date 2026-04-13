FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IsLabApp.csproj", "./"]
RUN dotnet restore "IsLabApp.csproj"
COPY . .
RUN dotnet publish "IsLabApp.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IsLabApp.dll"]