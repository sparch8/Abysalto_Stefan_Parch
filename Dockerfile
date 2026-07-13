FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Abysalto.StefanParch.Api.csproj", "./"]
RUN dotnet restore "Abysalto.StefanParch.Api.csproj"
COPY . .
RUN dotnet build "Abysalto.StefanParch.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Abysalto.StefanParch.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Abysalto.StefanParch.Api.dll"]
