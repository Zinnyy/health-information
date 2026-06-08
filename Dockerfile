# Stage 1: Build the .NET Web API
FROM ://microsoft.com AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy remaining source code and publish
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Create Final Runtime Image with BOTH .NET 8 and Python 3
FROM ://microsoft.com AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Install Python 3, pip, and required compilation packages
RUN apt-get update && apt-get install -y \
    python3 \
    python3-pip \
    python3-venv \
    && rm -rf /var/lib/apt/lists/*

# Install Pandas and Matplotlib globally inside the container
RUN pip3 install --no-cache-dir --break-system-packages pandas matplotlib

# Copy the compiled .NET binaries from Stage 1
COPY --from=build /app/publish .

# Create the folder where your Python engine outputs charts
RUN mkdir -p wwwroot/charts

# Start the Web API
ENTRYPOINT ["dotnet", "health information.dll"]

