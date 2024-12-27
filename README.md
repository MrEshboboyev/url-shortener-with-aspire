# UrlShortenerWithAspire

**UrlShortenerWithAspire** is a modern, high-performance web API that transforms long, cumbersome URLs into sleek, shareable short codes. This project combines the power of cutting-edge tools and frameworks to deliver an efficient, scalable, and robust URL-shortening service.

## ✨ Features

- **URL Shortening**: Convert long URLs into short, manageable links.
- **Scalability**: Designed to handle high volumes of requests using Redis caching and PostgreSQL.
- **Performance**: Optimized database interactions with Dapper ORM.
- **Observability**: Includes advanced logging and tracing with Open Telemetry.
- **Resiliency**: Implements robust error handling and monitoring to ensure reliability.
- **Developer Friendly**: Clean architecture with easy-to-extend modularity.

## 🚀 Technologies Used

- **ASP.NET Core**: The backbone of the API.
- **PostgreSQL**: For robust and scalable data storage.
- **Redis**: Caching layer to improve performance.
- **Dapper**: Lightweight ORM for efficient database access.
- **Aspire**: Ensures modularity and clear service separation.
- **Open Telemetry**: For tracing and monitoring requests.
- **Logger**: Comprehensive logging to track API performance and errors.

## 🛠️ Installation

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Redis](https://redis.io/download)
- [Docker](https://www.docker.com/get-started) (optional for containerized deployment)

### Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/MrEshboboyev/UrlShortenerWithAspire.git
   cd UrlShortenerWithAspire
   ```

2. Set up the database:
   - Create a PostgreSQL database and update the connection string in `appsettings.json`.
   - Run the provided migrations (if applicable).

3. Run the application:
   ```bash
   dotnet build
   dotnet run
   ```

4. Access the API at `http://localhost:5041`.

## 📖 API Endpoints

| Method | Endpoint              | Description              |
|--------|-----------------------|--------------------------|
| POST   | `/shorten`            | Create a shortened URL. |
| GET    | `/{shortCode}`        | Redirect to the original URL. |
| GET    | `/urls`               | View stats for a short URL. |

## 📊 Architecture

- **Controller Layer**: Handles API requests.
- **Service Layer**: Business logic implementation.
- **Data Layer**: Database interactions using Dapper.
- **Caching**: Redis integration for improved performance.

## 🧰 Development Tools

- **Unit Testing**: Ensure code reliability.
- **Open Telemetry**: Gain insights into API performance.
- **Logging**: Diagnose and debug with comprehensive logs.

## 🎨 UI (Future Work)
A user-friendly web interface for managing and viewing shortened URLs is planned for future development.

## 🤝 Contribution

We welcome contributions! Please follow these steps:

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature-name`).
3. Commit your changes (`git commit -m 'Add new feature'`).
4. Push to the branch (`git push origin feature-name`).
5. Open a pull request.

## 📜 License

This project is licensed under the [MIT License](LICENSE).

## 🙌 Acknowledgments

Special thanks to all the contributors and the community for their support!

---

⭐ If you find this project useful, please give it a star and share it with others!
