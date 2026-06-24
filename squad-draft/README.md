<div align="center">
<img width="1200" height="475" alt="GHBanner" src="https://github.com/user-attachments/assets/0aa67016-6eaf-458a-adb2-6e31a0763ed6" />
</div>

# Ultimate 11: Squad Draft - UI Service

This directory contains the frontend/UI service for **Ultimate 11: Squad Draft**, a daily soccer drafting game where players build their dream teams by selecting players from legendary club and international squads.

The UI is built using **React 19**, **TypeScript**, and **Vite** for fast, optimized development and builds, with styling handled by **Tailwind CSS**.

---

## Getting Started

### Prerequisites
- **Node.js** (v18.x or later recommended)
- **npm** (v9.x or later)

---

## Environment Configuration

The application uses environment variables for routing API and serverless function requests. You can configure these in a `.env` or `.env.local` file at the root of the project:

| Variable | Description | Default Value |
| :--- | :--- | :--- |
| `VITE_API_BASE_URL` | Base URL of the backend API service | `http://localhost:5212` |
| `VITE_FUNCTIONS_BASE_URL` | Base URL of the Azure Functions / serverless backend | `http://localhost:7172` |

### Configuring Local Environments

1. **Local Development (Mock/Local Services):**
   Use the default settings in `.env` to point to local services running on your machine:
   ```env
   VITE_API_BASE_URL=http://localhost:5212
   VITE_FUNCTIONS_BASE_URL=http://localhost:7172
   ```

2. **Connecting to Staging/Production Services:**
   Create a `.env.local` file (which is ignored by Git) to point to remote services:
   ```env
   VITE_API_BASE_URL=https://whoplayapi-hdfcfkhvcpf7gzg8.westeurope-01.azurewebsites.net
   VITE_FUNCTIONS_BASE_URL=https://sfuncrunners.azurewebsites.net
   ```

---

## Running Locally

### 1. Install Dependencies
Run the following command to install required node modules:
```bash
npm install
```

### 2. Run the Development Server
Start the Vite development server:
```bash
npm run dev
```
Once started, the application will usually be accessible at `http://localhost:5173`.

### 3. Build for Production
To build a highly optimized static bundle of the UI service for deployment:
```bash
npm run build
```
This compiles the output into the `dist/` directory.

### 4. Preview the Production Build Locally
Verify the production-ready assets locally before deploying:
```bash
npm run preview
```

---

## Deployment & Routing

- **Routing and Navigation:** The frontend is configured for deployment as a static website. The `staticwebapp.config.json` handles client-side routing fallback redirects and API path rewrites for Azure Static Web Apps.
- **Development Guidelines:** After each successful merge into the main branch, update `CHANGELOG.md` with the new changes and update relevant documentation.

