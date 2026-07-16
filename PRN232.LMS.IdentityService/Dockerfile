FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PRN232_LAB3.sln ./
COPY PRN232.LMS.Gateway/PRN232.LMS.Gateway.csproj PRN232.LMS.Gateway/
COPY PRN232.LMS.IdentityService/PRN232.LMS.IdentityService.csproj PRN232.LMS.IdentityService/
COPY PRN232.LMS.StudentService/PRN232.LMS.StudentService.csproj PRN232.LMS.StudentService/
COPY PRN232.LMS.CourseService/PRN232.LMS.CourseService.csproj PRN232.LMS.CourseService/

RUN dotnet restore PRN232_LAB3.sln

COPY . .
RUN dotnet publish PRN232.LMS.IdentityService/PRN232.LMS.IdentityService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PRN232.LMS.IdentityService.dll"]
