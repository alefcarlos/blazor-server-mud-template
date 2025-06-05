FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR source
COPY . .

RUN dotnet new install ./

RUN dotnet new mudblazor-server-solution -n AppName -o ./app

RUN dotnet format --verify-no-changes ./app/AppName.sln

RUN dotnet build ./app/AppName.sln