FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "SkillBridge.csproj"
RUN dotnet publish "SkillBridge.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:${PORT}

EXPOSE 10000

ENTRYPOINT ["dotnet", "SkillBridge.dll"]
