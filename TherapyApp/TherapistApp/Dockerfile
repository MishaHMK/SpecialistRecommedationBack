
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TherapistApp/TherapyApp.csproj", "TherapistApp/"]
RUN dotnet restore "TherapistApp/TherapyApp.csproj"
COPY . .
WORKDIR "/src/TherapistApp"
RUN dotnet build "TherapyApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TherapyApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TherapyApp.dll"]