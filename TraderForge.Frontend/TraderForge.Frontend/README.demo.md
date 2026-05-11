Demo recording script

1) Instalación (desde TraderForge.Frontend):

   npm install
   npm install puppeteer-core chrome-launcher fs-extra

2) Ejecutar la grabación:

   node scripts/demo_record.js

Notas:
- El script asume que el frontend corre en http://localhost:5173 (vite).
- Si quieres convertir frames a MP4 necesitas ffmpeg instalado en tu máquina.
- El video (o frames) se guardan en ../presentations/recordings/ dentro del repo.
