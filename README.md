# ⚡ OptiWin — Windows Performance & Privacy Optimizer

OptiWin es una aplicación de escritorio nativa de alto rendimiento desarrollada en **C# (.NET 8.0) y WPF**, inspirada en la arquitectura y recetas comprobadas de [**optimizerNXT**](https://github.com/hellzerg/optimizerNXT).

Cuenta con una interfaz moderna en **Modo Oscuro**, diseñada para ofrecer máxima transparencia y control sobre el rendimiento, privacidad y recursos del sistema Windows.

---

## 🚀 Características Principales

### 1. ⚡ Rendimiento & Gaming
- **Network Throttling:** Desactiva la limitación artificial de paquetes de red en Windows (`NetworkThrottlingIndex: 0xFFFFFFFF`) para reducir la latencia y el ping en juegos multijugador.
- **System Responsiveness:** Asigna el 100% de la capacidad de procesamiento de CPU al juego o aplicación en primer plano (en lugar de reservar 20% para tareas secundarias).
- **Prioridad de GPU para Juegos:** Eleva la prioridad del programador multimedia de Windows a GPU Priority 8 para evitar stuttering y caídas de frames.
- **AutoEndTasks:** Cierre instantáneo de aplicaciones colgadas al apagar o reiniciar el equipo (evita esperas de 20 segundos).
- **Respuesta de Menús:** Reduce el retardo de apertura de menús contextuales (`MenuShowDelay`) a 0 ms.

### 2. 🛡️ Privacidad & Telemetría
- **Desactivación de DiagTrack:** Detiene y deshabilita el servicio de telemetría y recolección de diagnósticos de Windows en segundo plano.
- **dmwappushservice:** Desactiva el servicio de enrutamiento WAP Push analítico.
- **Windows Error Reporting (WerSvc):** Bloquea el envío continuo de volcados de memoria y reportes de fallo hacia servidores externos.
- **ID de Publicidad:** Deshabilita el identificador publicitario de Windows para evitar el rastreo entre aplicaciones.
- **Búsqueda Web en Inicio:** Elimina las consultas a Bing desde el menú de inicio para una búsqueda local más rápida y privada.

### 3. 🧹 Limpiador de Disco Profundo
- Escanea y elimina archivos basura en:
  - `%TEMP%` del usuario
  - `C:\Windows\Temp`
  - `C:\Windows\Prefetch`
  - Volcados de error (`%LocalAppData%\CrashDumps`)
  - Cachés de Google Chrome y Microsoft Edge
- Vacía la Papelera de Reciclaje mediante la API Win32 nativa (`SHEmptyRecycleBin`).

### 4. 🧠 Liberador de Memoria RAM en Tiempo Real
- Monitor de consumo de memoria en vivo (Porcentaje, memoria usada, disponible y total).
- Vaciado del *Working Set* de los procesos en ejecución (`EmptyWorkingSet` de la API de Windows `psapi.dll`) para recuperar cientos de megabytes de RAM instantáneamente con un solo clic.

### 5. ⚙️ Sistema & Atajos
- **Rutas Largas (> 260 caracteres):** Elimina la restricción `MAX_PATH` de Windows (esencial para desarrolladores con carpetas profundas como `node_modules`).
- **Desactivar Teclas Especiales (Sticky Keys):** Evita la molesta ventana emergente al presionar la tecla `Shift` cinco veces consecutivas.

### 6. 📋 Consola de Actividad en Vivo
- Cada cambio en el registro, inicio o detención de un servicio queda registrado con marca de tiempo en la parte inferior de la ventana para total transparencia.

---

## 🛠️ Estructura del Código

```plaintext
OptiWin/
├── Core/
│   ├── AppLogger.cs          # Sistema de logging reactivo para la consola de la UI
│   ├── RegistryHelper.cs     # Manipulación segura de HKLM y HKCU (DWord, String, Delete)
│   ├── ServiceHelper.cs      # Control de servicios Windows (Detener, Deshabilitar, Iniciar)
│   ├── CleanerService.cs     # Escaneo y eliminación segura de archivos temporales
│   ├── RamOptimizer.cs       # Lectura GlobalMemoryStatusEx y vaciado EmptyWorkingSet
│   └── Tweaks/
│       ├── TweakModel.cs     # Modelo de datos con enlace bidireccional (INotifyPropertyChanged)
│       └── TweakManager.cs   # Catálogo de optimizaciones extraídas de optimizerNXT
├── MainWindow.xaml           # Interfaz gráfica moderna en Modo Oscuro (WPF)
├── MainWindow.xaml.cs        # Lógica de presentación, hilos en segundo plano y timers
├── app.manifest              # Solicitud de elevación como Administrador (requireAdministrator)
└── OptiWin.csproj            # Configuración de compilación .NET 8.0 Windows
```

---

## 📦 Compilación y Distribución

### Compilar en modo Debug:
```bash
dotnet build
```

### Generar un único ejecutable portátil (.exe independiente):
```bash
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
```
El archivo final compilado quedará listo en:
`bin\Release\net8.0-windows\win-x64\publish\OptiWin.exe`
# OptiWin
