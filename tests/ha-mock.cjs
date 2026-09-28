const http = require('node:http');
let online = true, calls = 0, state = 'off', brightness = 128, volume = .2, levelCalls = 0;
const entities = () => [
  { entity_id: 'light.test', state, attributes: { friendly_name: 'Test lamp', brightness, supported_color_modes: ['brightness'] } },
  { entity_id: 'sensor.temperature', state: '21.5', attributes: { friendly_name: 'Room temperature', unit_of_measurement: '°C' } },
  { entity_id: 'lock.door', state: 'locked', attributes: { friendly_name: 'Excluded lock' } },
  { entity_id: 'media_player.speaker', state: 'playing', attributes: { friendly_name: 'Speaker', supported_features: 4, volume_level: volume } },
  { entity_id: 'camera.front', state: 'idle', attributes: { friendly_name: 'Front camera' } },
  { entity_id: 'image.picture', state: '2026-09-28', attributes: { friendly_name: 'Wall picture' } },
  { entity_id: 'light.simple', state: 'off', attributes: { friendly_name: 'On-off only', supported_color_modes: ['onoff'] } },
];
http.createServer((req, res) => {
  const reply = (status, value) => { res.writeHead(status, {'Content-Type': 'application/json'}); res.end(JSON.stringify(value)); };
  if (req.url === '/test/offline') { online = false; return reply(200, {}); }
  if (req.url === '/test/online') { online = true; return reply(200, {}); }
  if (req.url === '/test/stats') return reply(200, {calls, levelCalls, brightness, volume});
  if (req.headers.authorization !== 'Bearer test-token') return reply(401, {message: 'Unauthorized'});
  if (!online) return reply(503, {message: 'Offline'});
  if (req.url === '/api/states' && req.method === 'GET') return reply(200, entities());
  if (req.url === '/api/camera_proxy/camera.front' || req.url === '/api/image_proxy/image.picture') {
    res.writeHead(200, {'Content-Type':'image/png'});
    return res.end(require('node:fs').readFileSync(require('node:path').join(__dirname,'../Assets/HAgoesQuest/Resources/Icons/switch.png')));
  }
  if (req.url === '/api/template' && req.method === 'POST') return reply(200, [
    { entity_id: 'light.test', area_id: 'living', area_name: 'Wohnzimmer' },
    { entity_id: 'sensor.temperature', area_id: 'kitchen', area_name: 'Küche' },
    { entity_id: 'lock.door', area_id: null, area_name: null },
    { entity_id: 'media_player.speaker', area_id: 'bedroom', area_name: 'Schlafzimmer' },
    { entity_id: 'camera.front', area_id: 'hall', area_name: 'Flur' },
  ]);
  if (req.method === 'POST' && (/^\/api\/services\/light\/turn_(on|off)$/.test(req.url) || req.url === '/api/services/media_player/volume_set')) {
    let body = '';
    req.on('data', chunk => { body += chunk; });
    req.on('end', () => {
      try {
        const payload=JSON.parse(body);
        if(req.url.endsWith('volume_set')) {
          if(payload.entity_id!=='media_player.speaker'||payload.volume_level<0||payload.volume_level>1)return reply(400,{});
          volume=payload.volume_level;levelCalls++;return reply(200,[]);
        }
        if (payload.entity_id !== 'light.test') return reply(400, {});
        if(payload.brightness_pct!==undefined) {
          if(payload.brightness_pct<0||payload.brightness_pct>100)return reply(400,{});
          brightness=Math.round(payload.brightness_pct*255/100);levelCalls++;state='on';return reply(200,[]);
        }
        calls++; state = req.url.endsWith('turn_on') ? 'on' : 'off';
        return reply(200, [entities()[0]]);
      } catch { return reply(400, {}); }
    });
    return;
  }
  reply(404, {});
}).listen(18123, '127.0.0.1', () => console.log('Home Assistant mock listening on loopback:18123'));
