const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:7062';

const PROXY_CONFIG = [
  {
    context: [
      "/weatherforecast",
      "/api/job",
      "/api/profile",
      "/api/application",
      "/api/webhook",
      "/api/user",
      "/api/auth",
      "/api/admin"
    ],
    target,
    secure: false,
    changeOrigin: true,
    logLevel: "debug"
  }
]

module.exports = PROXY_CONFIG;
