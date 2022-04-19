FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /term

# Copy everything related to the term project - This will happen in a temporary image
COPY /term ./
# Restore as distinct layers
RUN dotnet restore 
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /term

# Copy over the published application
COPY --from=build-env /term/out .

# Need this file and environment variable to access Firebase
COPY mtg-inventory-main-service-account.json .
ENV GOOGLE_APPLICATION_CREDENTIALS=/term/mtg-inventory-main-service-account.json

# Need to install ncurses to get Terminal.Gui to work (https://en.wikipedia.org/wiki/Ncurses, https://invisible-island.net/ncurses/)
RUN apt-get update
RUN apt-get install libncursesw6 -y

# This mostly works, but ascii-art lines and characters aren't drawn
ENV TERM=xterm-256color

ENTRYPOINT ["dotnet", "term.dll"]
#ENTRYPOINT /bin/bash