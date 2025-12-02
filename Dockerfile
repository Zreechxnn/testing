FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["testing.csproj", "./"]
RUN dotnet restore "testing.csproj"

COPY . .
RUN dotnet publish "testing.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=7860
EXPOSE 7860

ENTRYPOINT ["dotnet", "testing.dll"]