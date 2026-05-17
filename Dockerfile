FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DigitalCoach.sln ./
COPY src/DigitalCoach.Domain/DigitalCoach.Domain.csproj src/DigitalCoach.Domain/
COPY src/DigitalCoach.Application/DigitalCoach.Application.csproj src/DigitalCoach.Application/
COPY src/DigitalCoach.Infrastructure/DigitalCoach.Infrastructure.csproj src/DigitalCoach.Infrastructure/
COPY src/DigitalCoach.Api/DigitalCoach.Api.csproj src/DigitalCoach.Api/

RUN dotnet restore DigitalCoach.sln

COPY . .
RUN dotnet publish src/DigitalCoach.Api/DigitalCoach.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DigitalCoach.Api.dll"]
