# Algebricks — Aplicación VR educativa (Factorización algebraica)

Algebricks es una aplicación de realidad virtual desarrollada en Unity que enseña factorización algebraica mediante la manipulación de bloques 3D. El usuario construye la factorización correcta de expresiones usando bloques que representan $x^2$, $x$ y términos independientes, recibiendo retroalimentación visual, sonora y háptica.

## Resumen rápido
- Plataforma principal: Unity 2022.3.37f1 (LTS).  
- Objetivo: PC (Windows) + Meta Quest 2 vía Oculus Link/Air Link.  
- Soporte opcional: Android (Quest 2 standalone) si se configura el target a Android.

## Dependencias principales
- Unity XR Interaction Toolkit  
- OpenXR Plugin  
- Integración de Oculus/Meta (opcional para soporte Touch)  
- Render pipeline: Built-in o URP (ver Package Manager para la versión exacta)

## Hardware soportado
- Casco: Meta Quest 2  
- Mandos: Oculus/Meta Touch (izquierdo/derecho)  
- PC: Windows 10/11 con GPU compatible VR (NVIDIA GTX/RTX recomendada)

## Controles e interacción
- Gatillos (trigger / grip): agarrar y soltar bloques (XR Interaction Toolkit).  
- SnapZones en la mesa para colocar bloques correctamente.  
- Rotación manual de bloques mientras están agarrados.  
- Puntero/ray interactor para UI.

## Flujo de la actividad
1. Ver/escuchar la explicación del ejercicio.  
2. Manipular bloques para formar la factorización correcta.  
3. Recibir feedback (vibración, sonidos, efectos de color).  
4. Si completa el ejercicio: secuencia final de explicación y avance al siguiente nivel.

## Cómo ejecutar localmente (desarrollo)
1. Conecta el Meta Quest 2 y abre la app de Oculus en el PC.  
2. En Unity: File → Build Settings… → Add Open Scenes.  
3. En Player Settings → XR Plug-in Management: activa OpenXR y el feature group de Oculus Touch.  
4. Build o Build and Run. En PC ejecuta el .exe con la app Oculus activa.

## Feedback y accesibilidad
- Vibración en mandos al encajar bloques (o al cometer errores).  
- Sonidos y efectos visuales al completar ejercicios.  
- Pistas guiadas y una explicación final para reforzar el aprendizaje.

## Licencia y autores
Creative Commons 
© 2025 – Uso académico en la Universidad Autónoma de Occidente. No distribuido comercialmente.

Autores:
- Juan David Carvajal  
- Nicolás Cortés Restrepo  
- Juan David Mendoza Gaspar  
- Maria Paula Llano Bravo  
Estudiantes de Ingeniería Multimedia, Universidad Autónoma de Occidente (UAO).

## Archivos relacionados en este repositorio
- [`.gitignore`](.gitignore)  
- [`.vsconfig`](.vsconfig)  
- [000001c100000006ffffffff69666564_fce8395c8fd8a9d9_a6b778d5e18d8817_0_3.0.toc](000001c100000006ffffffff69666564_fce8395c8fd8a9d9_a6b778d5e18d8817_0_3.0.toc)

---
Para añadir más detalles (por ejemplo, versiones exactas de paquetes desde Package Manager, configuraciones URP/HDRP o comandos de build automatizados), actualiza este README con esa información.
