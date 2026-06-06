FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PRN232_LAB2.sln ./
COPY PRN232.LMS.API/PRN232.LMS.API.csproj PRN232.LMS.API/
COPY PRN232.LMS.Services/PRN232.LMS.Services.csproj PRN232.LMS.Services/
COPY PRN232.LMS.Repositories/PRN232.LMS.Repositories.csproj PRN232.LMS.Repositories/

RUN dotnet restore PRN232_LAB2.sln

COPY . .
RUN dotnet publish PRN232.LMS.API/PRN232.LMS.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PRN232.LMS.API.dll"]
