# 📚 LibrarySystem

Система управления библиотекой на базе микросервисной архитектуры. REST API на ASP.NET Core 10, PostgreSQL, Redis, Nginx, мониторинг через Prometheus + Grafana, CI через GitHub Actions.

---

## 🗂 Структура репозитория

![Music](https://284baef4-3d14-4ca5-8247-4811f0d6b14b.selstorage.ru/d13d5483-522f-40ca-8d49-07d096e34f3f_f0f21383-40fb-4d13-a2b5-ce81b242f788.png)


---

## ⚙️ Стек технологий

| Компонент | Технология | Версия |
|---|---|---|
| REST API | ASP.NET Core | .NET 10 |
| База данных | PostgreSQL | 15 |
| Кэш | Redis | 7 |
| ORM | Entity Framework Core + Npgsql | 10.x |
| Прокси | Nginx | Alpine |
| Метрики | prometheus-net.AspNetCore | 8.2 |
| Мониторинг | Prometheus + Grafana | latest |
| Документация API | Swagger / Swashbuckle | 10.x |
| CI | GitHub Actions | — |
| Контейнеры | Docker + Docker Compose | 3.9 |

---

## 🚀 Быстрый старт

### Требования

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) ≥ 24.x
- [Docker Compose](https://docs.docker.com/compose/) ≥ 2.x

### Запуск

```bash
# 1. Клонировать репозиторий
git clone https://github.com/<your-username>/LibrarySystem.git
cd LibrarySystem

# 2. Запустить все сервисы
docker compose up --build -d

# 3. Дождаться готовности (≈ 30 секунд)
docker compose ps
```

После запуска доступны:

| Сервис | URL |
|---|---|
| Swagger UI | http://localhost/swagger |
| REST API | http://localhost/api/ |
| Grafana | http://localhost:3000 |
| Prometheus | http://localhost:9090 |
| Метрики API | http://localhost/metrics |

> **Grafana**: логин `admin`, пароль `admin`

### Остановка

```bash
docker compose down

# С удалением volumes (сбросить базу):
docker compose down -v
```

---

## 🗄 Сущности базы данных

### Book

| Поле | Тип | Описание |
|---|---|---|
| `Id` | int | Первичный ключ |
| `Title` | string(200) | Название книги |
| `Author` | string(100) | Автор |
| `ISBN` | string(17) | ISBN (уникальный) |
| `PublicationYear` | int | Год издания |
| `Genre` | string(100) | Жанр |
| `TotalCopies` | int | Всего экземпляров |
| `AvailableCopies` | int | Доступных экземпляров |

### Reader

| Поле | Тип | Описание |
|---|---|---|
| `Id` | int | Первичный ключ |
| `FullName` | string(100) | ФИО |
| `Email` | string(150) | Email (уникальный) |
| `Phone` | string(20) | Телефон |
| `RegistrationDate` | DateTime | Дата регистрации (UTC) |

### BookLoan

| Поле | Тип | Описание |
|---|---|---|
| `Id` | int | Первичный ключ |
| `BookId` | int | FK → Books.Id |
| `ReaderId` | int | FK → Readers.Id |
| `LoanDate` | DateTime | Дата выдачи (UTC) |
| `DueDate` | DateTime | Срок возврата (+14 дней) |
| `ReturnDate` | DateTime? | Дата возврата (`null` — не возвращена) |

Связи: `Books` 1→N `BookLoans` N←1 `Readers`. Ограничение `RESTRICT` на удаление при наличии активных выдач.

---

## 🔌 API эндпоинты

### Books — `/api/books`

| Метод | URL | Описание | Тело запроса | Ответ |
|---|---|---|---|---|
| `GET` | `/api/books` | Список книг | — | `200` массив Book *(кэш Redis 30 с)* |
| `GET` | `/api/books/{id}` | Книга по ID | — | `200` Book / `404` |
| `POST` | `/api/books` | Создать книгу | JSON Book | `201` Book |
| `PUT` | `/api/books/{id}` | Обновить книгу | JSON Book | `204` / `404` |
| `DELETE` | `/api/books/{id}` | Удалить книгу | — | `204` / `404` |

### Readers — `/api/readers`

| Метод | URL | Описание | Тело запроса | Ответ |
|---|---|---|---|---|
| `GET` | `/api/readers` | Список читателей | — | `200` массив Reader |
| `GET` | `/api/readers/{id}` | Читатель по ID | — | `200` Reader / `404` |
| `POST` | `/api/readers` | Создать читателя | JSON Reader | `201` Reader |
| `PUT` | `/api/readers/{id}` | Обновить читателя | JSON Reader | `204` / `404` |
| `DELETE` | `/api/readers/{id}` | Удалить читателя | — | `204` / `404` |

### BookLoans — `/api/bookloans`

| Метод | URL | Описание | Параметры | Ответ |
|---|---|---|---|---|
| `GET` | `/api/bookloans` | Все выдачи | — | `200` массив BookLoan |
| `POST` | `/api/bookloans/loan` | Выдать книгу | `?bookId=&readerId=` | `200` / `400` / `404` |
| `POST` | `/api/bookloans/return` | Вернуть книгу | `?loanId=` | `200` / `400` / `404` |

Полная интерактивная документация: **http://localhost/swagger**

---

## 🏗 Архитектура

<img width="406" height="407" alt="image" src="https://github.com/user-attachments/assets/f99dadf6-3f90-449e-8406-aeb5ef45d349" />


**Кэширование**: `GET /api/books` проверяет ключ `"books"` в Redis. При cache miss записывает результат с TTL 30 сек. Мутирующие операции (`POST`/`PUT`/`DELETE`) вызывают `KeyDeleteAsync` для инвалидации. Все обращения к Redis обёрнуты в `try/catch` — при недоступности Redis API продолжает работу через БД.

---

## 🖥 Клиентские приложения

### LibraryConsoleClient

Консольный клиент демонстрирует применение **многоадресных делегатов** C#:

```csharp
// Объявление делегата в ApiService
public delegate void RequestCompletedHandler(string endpoint, int status, long ms);
public event RequestCompletedHandler? RequestCompleted;

// Два независимых обработчика
api.RequestCompleted += ConsoleLogger;  // вывод в консоль
api.RequestCompleted += FileLogger;     // запись в requests.log

// После операции 3 — динамическая отписка
api.RequestCompleted -= FileLogger;
```

Клиент последовательно выполняет 5 операций: создание книги → получение списка → получение по ID → обновление → удаление.

Запуск (требует запущенного API):

```bash
cd LibraryConsoleClient
dotnet run
```

### LibraryClient (WPF)

Windows-приложение для управления книгами и читателями с графическим интерфейсом.

> Требует Windows и .NET 10 Desktop Runtime.

---

## 📊 Мониторинг

Prometheus собирает метрики ASP.NET Core с эндпоинта `/metrics` каждые 10 секунд.

Основные метрики:

| Метрика | Описание |
|---|---|
| `http_requests_received_total` | Кол-во HTTP-запросов по маршрутам |
| `http_request_duration_seconds` | Длительность запросов (гистограмма) |
| `library_loans_total` | Кол-во выданных книг |
| `library_returns_total` | Кол-во возвратов |
| `library_loan_denied_total` | Отказы в выдаче (нет экземпляров) |

**Grafana** доступна на http://localhost:3000. Datasource Prometheus настроен автоматически через provisioning.

---

## 🔄 CI/CD

GitHub Actions запускает пайплайн при каждом `push` и `pull_request` в ветку `main`:

```
Trigger → Checkout → Setup .NET 10 → Restore LibraryApi → Build LibraryApi
                                   → Restore ConsoleClient → Build ConsoleClient → ✓ Passed
```

Файл: [`.github/workflows/ci.yml`](.github/workflows/ci.yml)

---

## 🛠 Локальная разработка без Docker

### Требования

- .NET 10 SDK
- PostgreSQL 15
- Redis 7

### API

```bash
cd LibraryApi

# Задать переменные окружения
export CONNECTION_STRING="Host=localhost;Database=librarydb;Username=postgres;Password=postgres"
export REDIS_HOST="localhost:6379"

dotnet run
# API доступен на https://localhost:5001
# Swagger: https://localhost:5001/swagger
```

### Консольный клиент

```bash
cd LibraryConsoleClient
dotnet run
```

### Миграции EF Core

```bash
cd LibraryApi

# Применить миграции вручную
dotnet ef database update

# Создать новую миграцию
dotnet ef migrations add <MigrationName>
```

---

## 🔧 Переменные окружения

| Переменная | По умолчанию (локально) | Описание |
|---|---|---|
| `CONNECTION_STRING` | `Host=localhost;Database=librarydb;Username=postgres;Password=postgres` | Строка подключения к PostgreSQL |
| `REDIS_HOST` | `localhost` | Адрес Redis (`host:port`) |
| `GF_SECURITY_ADMIN_PASSWORD` | `admin` | Пароль администратора Grafana |

---

## 📦 Зависимости (NuGet)

**LibraryApi:**
- `Swashbuckle.AspNetCore` — Swagger UI
- `Npgsql.EntityFrameworkCore.PostgreSQL` — провайдер EF Core для PostgreSQL
- `StackExchange.Redis` — клиент Redis
- `prometheus-net.AspNetCore` — экспорт метрик Prometheus
- `Microsoft.EntityFrameworkCore.Design` — инструменты EF Core

---

## 📝 Лицензия

MIT
