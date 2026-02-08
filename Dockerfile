# OpenMir2 Server Dockerfile
# 多阶段构建，减小镜像体积

# ============================================
# 阶段1: 构建阶段
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制解决方案和项目文件
COPY OpenMir2.sln .
COPY src/ ./src/
COPY global.json ./

# 还原依赖
RUN dotnet restore OpenMir2.sln

# 编译发布
RUN dotnet publish src/DBSrv/DBSrv.csproj -c Release -o /app/DBSrv
RUN dotnet publish src/LoginSrv/LoginSrv.csproj -c Release -o /app/LoginSrv
RUN dotnet publish src/GameSrv/GameSrv.csproj -c Release -o /app/GameSrv
RUN dotnet publish src/WebApi/WebApi.csproj -c Release -o /app/WebApi

# ============================================
# 阶段2: 运行阶段 - DBSrv
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS dbsrv
WORKDIR /app
COPY --from=build /app/DBSrv .
EXPOSE 6000
ENTRYPOINT ["dotnet", "DBSrv.dll"]

# ============================================
# 阶段3: 运行阶段 - LoginSrv
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS loginsrv
WORKDIR /app
COPY --from=build /app/LoginSrv .
EXPOSE 7000
ENTRYPOINT ["dotnet", "LoginSrv.dll"]

# ============================================
# 阶段4: 运行阶段 - GameSrv
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS gamesrv
WORKDIR /app
COPY --from=build /app/GameSrv .
COPY MirServer/Mir200/Envir ./Envir
EXPOSE 7100
ENTRYPOINT ["dotnet", "GameSrv.dll"]

# ============================================
# 阶段5: 运行阶段 - WebApi
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS webapi
WORKDIR /app
COPY --from=build /app/WebApi .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "WebApi.dll"]
