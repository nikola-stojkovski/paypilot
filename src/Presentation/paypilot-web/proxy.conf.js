// Aspire passes the API address in as environment variables (from WithReference(api))
const target = process.env.API_HTTPS || process.env.API_HTTP;

module.exports = {
  '/api': {
    target,
    secure: false,
    changeOrigin: true,
    pathRewrite: { '^/api': '' },
  },
};