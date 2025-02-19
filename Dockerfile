# Используем SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app
# Открываем порты
EXPOSE 80
EXPOSE 443
# Копируем файл проекта и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируем весь исходный код в контейнер
COPY . ./
RUN dotnet publish -c Release -o out

# Копируем папку Uploads в финальный образ
RUN mkdir -p /app/out/uploads
COPY Uploads /app/out/uploads

# Используем более легкий runtime-образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final-env
WORKDIR /app
COPY --from=build-env /app/out .



# Запуск приложения
ENTRYPOINT ["dotnet", "DashboardAPI.dll"]
