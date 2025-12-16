FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HotelBookingApp.WebAPI/HotelBookingApp.WebAPI.csproj", "HotelBookingApp.WebAPI/"]
COPY ["HotelBookingApp.Infrastructure/HotelBookingApp.Infrastructure.csproj", "HotelBookingApp.Infrastructure/"]
COPY ["HotelBookingApp.Application/HotelBookingApp.Application.csproj", "HotelBookingApp.Application/"]
COPY ["HotelBookingApp.Domain/HotelBookingApp.Domain.csproj", "HotelBookingApp.Domain/"]
RUN dotnet restore "HotelBookingApp.WebAPI/HotelBookingApp.WebAPI.csproj"
COPY . .
WORKDIR "/src/HotelBookingApp.WebAPI"
RUN dotnet build "HotelBookingApp.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HotelBookingApp.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HotelBookingApp.WebAPI.dll"]