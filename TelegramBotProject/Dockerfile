FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /TelegramBotProject

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /TelegramBotProject
COPY --from=build-env /TelegramBotProject/out .
RUN mkdir -p /Logs
RUN mkdir -p /DB
ENTRYPOINT ["dotnet", "TelegramBotProject.dll"]
