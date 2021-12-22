FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Manager.csproj", "./"]
RUN dotnet restore "Manager.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Manager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Manager.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /src/Configs ./Configs
ENTRYPOINT ["dotnet", "Manager.dll"]
