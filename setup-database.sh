#!/bin/bash

# Function to show usage
show_usage() {
    echo "Database configuration is required for setup."
    echo ""
    echo "Usage: $0 <db_host> <db_user> <db_password> [db_port]"
    echo ""
    echo "Example:"
    echo "  $0 localhost sa SU2orange! 1433"
    echo "  $0 localhost sa SU2orange!    # Uses default port 1433"
    echo ""
    echo "Note: These are the default development credentials from docker-compose.yaml"
    echo "      Default port is 1433 if not specified"
    echo ""
    echo "Press Enter to keep this terminal open, or type 'exit' to close it:"
    read -r choice
    if [ "$choice" = "exit" ]; then
        exit 1
    fi
    return 1
}

# Check if required parameters are provided
if [ "$#" -lt 3 ]; then
    show_usage
    exit 0
fi

DB_HOST=$1
DB_USER=$2
DB_PASSWORD=$3
DB_PORT=${4:-1433}  # Default to 1433 if not provided

# Function to handle errors
handle_error() {
    echo "Error: $1"
    echo ""
    echo "Press Enter to keep this terminal open, or type 'exit' to close it:"
    read -r choice
    if [ "$choice" = "exit" ]; then
        exit 1
    fi
    return 1
}

# Install required dotnet tools
echo "Installing dotnet-ef tool..."
if ! dotnet tool install --global dotnet-ef; then
    handle_error "Failed to install dotnet-ef tool"
    exit 0
fi
export PATH="$PATH:$HOME/.dotnet/tools"

# Update connection string in appsettings.json
echo "Configuring database connection..."
connection_string="Server=$DB_HOST,$DB_PORT;Database=canvas_lms;User Id=$DB_USER;Password=$DB_PASSWORD;TrustServerCertificate=True;Encrypt=False"
escaped_connection_string=$(echo "$connection_string" | sed 's/[\/&]/\\&/g')
if ! sed -i '' "s|\"ApplicationDBContext\": \".*\"|\"ApplicationDBContext\": \"$escaped_connection_string\"|" appsettings.json; then
    handle_error "Failed to update connection string"
    exit 0
fi

# Restore packages
echo "Restoring packages..."
if ! dotnet restore; then
    handle_error "Failed to restore packages"
    exit 0
fi

# Create migrations if they don't exist
if [ ! -d "Migrations" ]; then
    echo "Creating database migrations..."
    if ! dotnet ef migrations add InitialCreate; then
        handle_error "Failed to create migrations"
        exit 0
    fi
fi

# Apply migrations
echo "Applying migrations..."
if ! dotnet ef database update; then
    handle_error "Failed to apply migrations"
    exit 0
fi

# Run the seeding
echo "Seeding initial data..."
if ! dotnet run --project . seed-data; then
    handle_error "Failed to seed data"
    exit 0
fi

echo "Database setup completed successfully!"
echo ""
echo "Starting application in production mode..."
echo "=================================================="
echo ""
echo "Application is starting at:"
echo "  🌐 http://localhost:5285 (HTTP)"
echo ""
echo "To stop the application:"
echo "  Press Ctrl+C (this will stop the app but keep the terminal open)"
echo ""
echo "=================================================="
echo ""

# Start the application in production mode and open browser
(sleep 5 && open http://localhost:5285) &  # Start browser after 5 seconds
dotnet run --configuration Release