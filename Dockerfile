# ---------- Stage 1: Build & publish ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY RealtimeChat.API/RealtimeChat.API.csproj RealtimeChat.API/
COPY RealtimeChat.Application/RealtimeChat.Application.csproj RealtimeChat.Application/
COPY RealtimeChat.Domain/RealtimeChat.Domain.csproj RealtimeChat.Domain/
COPY RealtimeChat.Infrastructure/RealtimeChat.Infrastructure.csproj RealtimeChat.Infrastructure/

RUN dotnet restore RealtimeChat.API/RealtimeChat.API.csproj

COPY . .
RUN dotnet publish RealtimeChat.API/RealtimeChat.API.csproj -c Release -o /app/publish --no-restore

# ---------- Stage 2: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# OpenSSL 3.x defaults to SECLEVEL=2, which some MongoDB Atlas TLS handshakes fail
# against with "tlsv1 alert internal error". Relax to SECLEVEL=1 to fix it.
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/' /etc/ssl/openssl.cnf

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet RealtimeChat.API.dll"]