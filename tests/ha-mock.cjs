const http = require('node:http');
let online = true, calls = 0, state = 'off';
const entities = () => [
  { entity_id: 'light.test', state, attributes: { friendly_name: 'Test lamp' } },
  { entity_id: 'sensor.temperature', state: '21.5', attributes: { friendly_name: 'Room temperature', unit_of_measurement: '°C' } },
  { entity_id: 'lock.door', state: 'locked', attributes: { friendly_name: 'Excluded lock' } },
];
http.createServer((req, res) => {
  const reply = (status, value) => { res.writeHead(status, {'Content-Type': 'application/json'}); res.end(JSON.stringify(value)); };
  if (req.url === '/test/offline') { online = false; return reply(200, {}); }
  if (req.url === '/test/online') { online = true; return reply(200, {}); }
  if (req.url === '/test/stats') return reply(200, {calls});
  if (req.headers.authorization !== 'Bearer test-token') return reply(401, {message: 'Unauthorized'});
  if (!online) return reply(503, {message: 'Offline'});
  if (req.url === '/api/states' && req.method === 'GET') return reply(200, entities());
  if (req.method === 'POST' && /^\/api\/services\/light\/turn_(on|off)$/.test(req.url)) {
    let body = '';
    req.on('data', chunk => { body += chunk; });
    req.on('end', () => {
      try {
        if (JSON.parse(body).entity_id !== 'light.test') return reply(400, {});
        calls++; state = req.url.endsWith('turn_on') ? 'on' : 'off';
        return reply(200, [entities()[0]]);
      } catch { return reply(400, {}); }
    });
    return;
  }
  reply(404, {});
}).listen(18123, '127.0.0.1', () => console.log('Home Assistant mock listening on loopback:18123'));
