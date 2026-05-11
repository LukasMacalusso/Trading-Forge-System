#!/usr/bin/env node
/**
 * Demo recorder using Puppeteer (puppeteer-core) and local Chrome.
 *
 * Usage (from TraderForge.Frontend):
 *   npm install
 *   npm install puppeteer-core chrome-launcher fs-extra
 *   node scripts/demo_record.js
 *
 * The script will:
 * - Start the Vite dev server (assumes `npm run dev` is available)
 * - Launch Chrome via chrome-launcher with remote debugging
 * - Use puppeteer-core to automate the UI and take screenshots
 * - Save frames under presentations/recordings/frames/
 * - If ffmpeg is installed, it will attempt to assemble frames into MP4
 */

const { spawn } = require('child_process');
const chromeLauncher = require('chrome-launcher');
const puppeteer = require('puppeteer-core');
const fs = require('fs-extra');
const path = require('path');

const FRONTEND_DIR = path.resolve(__dirname, '..');
const OUT_DIR = path.resolve(FRONTEND_DIR, '..', 'presentations', 'recordings');

async function startVite() {
  console.log('Starting Vite dev server...');
  const npmPath = process.env.npm_execpath || 'npm';
  const proc = spawn(npmPath, ['run', 'dev'], { cwd: FRONTEND_DIR, stdio: 'inherit' });
  // give it some time to boot up
  await new Promise((res) => setTimeout(res, 3000));
  return proc;
}

async function launchChrome() {
  console.log('Launching Chrome...');
  const chrome = await chromeLauncher.launch({
    chromeFlags: ['--remote-debugging-port=9222', '--no-default-browser-check', '--no-first-run']
  });
  console.log('Chrome PID', chrome.pid);
  return chrome;
}

async function recordFlow() {
  await fs.remove(OUT_DIR);
  await fs.ensureDir(path.join(OUT_DIR, 'frames'));

  const viteProc = await startVite();
  const chrome = await launchChrome();
  const browser = await puppeteer.connect({ browserURL: 'http://127.0.0.1:9222' });
  const page = await browser.newPage();
  await page.setViewport({ width: 1280, height: 800 });

  const framesDir = path.join(OUT_DIR, 'frames');
  let frameCount = 0;
  async function takeFrame() {
    const file = path.join(framesDir, `frame_${String(frameCount).padStart(5, '0')}.png`);
    await page.screenshot({ path: file });
    frameCount++;
    console.log('Captured', file);
  }

  // Navigate to app
  await page.goto('http://localhost:5173', { waitUntil: 'networkidle' });
  await takeFrame();

  // Go to register
  await page.click('a[href="/register"]');
  await page.waitForTimeout(1000);
  await takeFrame();

  // Fill register form
  await page.type('input[name="email"]', 'lmacalusso@traderforge.com');
  await page.type('input[name="password"]', 'Stormblessed4321..');
  // if there's a username field
  const username = await page.$('input[name="username"]');
  if (username) await page.type('input[name="username"]', 'lmacalusso');
  await takeFrame();
  await page.click('button[type="submit"]');
  await page.waitForTimeout(2000);
  await takeFrame();

  // Attempt login (some apps auto-login after register)
  await page.goto('http://localhost:5173/login', { waitUntil: 'networkidle' });
  await page.waitForTimeout(500);
  await page.type('input[name="email"]', 'lmacalusso@traderforge.com');
  await page.type('input[name="password"]', 'Stormblessed4321..');
  await takeFrame();
  await page.click('button[type="submit"]');
  await page.waitForTimeout(2500);
  await takeFrame();

  // Navigate to dashboard
  await page.goto('http://localhost:5173/dashboard', { waitUntil: 'networkidle' });
  await page.waitForTimeout(1500);
  await takeFrame();

  // Place a manual order if order panel exists
  const orderSelector = 'button[data-test="place-order"]';
  const orderBtn = await page.$(orderSelector);
  if (orderBtn) {
    await page.click(orderSelector);
    await page.waitForTimeout(1000);
    await takeFrame();
  }

  // Final capture of portfolio
  await page.goto('http://localhost:5173/portfolio', { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);
  await takeFrame();

  console.log('Done recording — closing browser');
  await browser.disconnect();
  await chrome.kill();
  // kill vite
  viteProc.kill();

  // try to assemble with ffmpeg if available
  try {
    const ffmpeg = require('child_process').spawnSync('ffmpeg', [ // NOSONAR:S4036 — dev-only script, optional tool
      '-y',
      '-framerate', '10',
      '-i', path.join(framesDir, 'frame_%05d.png'),
      '-c:v', 'libx264',
      '-pix_fmt', 'yuv420p',
      path.join(OUT_DIR, 'demo.mp4')
    ], { stdio: 'inherit' });
    if (ffmpeg.status === 0) console.log('Generated', path.join(OUT_DIR, 'demo.mp4'));
    else console.log('ffmpeg failed or not present — frames are in', framesDir);
  } catch (e) {
    console.log('ffmpeg not available. Frames saved to', framesDir);
  }
}

recordFlow().catch((err) => {
  console.error(err);
  process.exit(1);
});
