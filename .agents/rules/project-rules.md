# IIQF Project Rules & Context

Welcome to the IIQF Project! This file outlines the project structure, tech stack, and conventions to ensure consistent, clean development.

## 1. Project Structure
The repository is structured as a monorepo with three major components:
- **Backend (`/Service/DotNetService/`)**:
  - C# .NET solution (`BhDream.slnx`) containing the Domain, Application, Infrastructure, and WebAPI projects.
- **Frontend (`/Ui/quant/`)**:
  - React + Vite + Recharts frontend application.
- **Machine Learning (`/Ml/ml_trainer/`)**:
  - Python application containing ML models, trainer scripts, and trainers/orchestrators (virtual environment: `.venv`).

---

## 2. Command Reference

### Backend (.NET)
- **Navigate to**: `Service/DotNetService/`
- **Build**: `dotnet build`
- **Run Web API**: `dotnet run --project BhDream.WebAPI`

### Frontend (React/Vite)
- **Navigate to**: `Ui/quant/`
- **Install Dependencies**: `npm install`
- **Run Dev Server**: `npm run dev`
- **Build Production**: `npm run build`

### Machine Learning (Python)
- **Navigate to**: `Ml/ml_trainer/`
- **Activate Virtual Environment**:
  - Windows: `.venv\Scripts\Activate.ps1` or `.venv\Scripts\activate`
- **Run Application**: `python app/main.py`

---

## 3. Coding Guidelines

### C# / .NET
- Use async/await end-to-end for all async operations.
- Adhere to Clean Architecture:
  - **Domain**: Entities, Domain Events.
  - **Application**: DTOs, interfaces/abstractions, service implementations.
  - **Infrastructure**: DbContext, concrete repository implementations.
  - **WebAPI**: Controllers, routes, dependency injection configuration.

### React / Frontend
- Prefer functional components and hooks.
- Use styling guidelines defined in `index.css`.
- Ensure component separation of concerns (keep components modular and clean).

### Python / ML
- Keep dependencies updated using virtual environments.
- Follow PEP-8 styling standards.
