FROM microsoft/aspnetcore-build:2.0 AS build-env
WORKDIR /app

# copy everything else and build
COPY . ./

WORKDIR /app/webapp

RUN dotnet restore
RUN dotnet publish -c Release -o out

# build runtime image
FROM microsoft/aspnetcore:2.0
WORKDIR /app
COPY --from=build-env /app/webapp/out .
ENTRYPOINT ["dotnet", "webapp.dll"]